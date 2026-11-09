using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.UpdateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Service.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class UserService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ITeamInviteRepository teamInviteRepository,
        ITeamUserRepository teamUserRepository,
        IRoleRepository roleRepository,
        IAuthService authService,
        IPasswordHasher passwordHasher,
        IMapper mapper) : GenericService<User>(userRepository, unitOfWork), IUserService
    {
        private const string DefaultProfilePicturePath = "/img/profile.png";

        private static string FormatProfilePicture(byte[]? profilePicture) =>
            profilePicture is { Length: > 0 }
                ? $"data:image/png;base64,{Convert.ToBase64String(profilePicture)}"
                : DefaultProfilePicturePath;

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await userRepository.Where(u => u.Email == email)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await userRepository.GetUserWithTeamsProjectsTasksAsync(userId);
            if (user is null)
            {
                return null;
            }

            var userTeam = user.TeamUsers.FirstOrDefault();
            if (userTeam == null)
            {
                return new UserProfileDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    ProfilePicture = FormatProfilePicture(user.ProfilePicture),
                    RegisterDate = user.CreatedDate,
                    OwnProfile = true,
                    CurrentTeam = "No Team",
                    Role = "No Role",
                    TotalProjects = 0,
                    CompletedProjects = 0,
                    TotalTasks = 0,
                    CompletedTasks = 0
                };
            }

            var userTeamIds = user.TeamUsers.Select(tu => tu.Id).ToHashSet();
            var viewableProjects = user.TeamUsers
                .Where(tu => PermissionResolver.HasPermission(tu, tu.Role, x => x.CanViewProjects))
                .SelectMany(tu => tu.Team.Projects)
                .ToList();

            var viewableAssignedTasks = user.TeamUsers
                .Where(tu => PermissionResolver.HasPermission(tu, tu.Role, x => x.CanViewTasks))
                .SelectMany(tu => tu.Team.Projects)
                .SelectMany(p => p.ProjectTasks)
                .Where(t => userTeamIds.Contains(t.AppointeeId))
                .ToList();

            return new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                ProfilePicture = FormatProfilePicture(user.ProfilePicture),
                RegisterDate = user.CreatedDate,
                OwnProfile = true,
                CurrentTeam = userTeam.Team.TeamName,
                Role = user.Teams.Count != 0 || user.TeamUsers.Any(tu => tu.Team.ManagerId == user.Id) ? "Team Manager" : "Team Member",
                TotalProjects = viewableProjects.Count,
                CompletedProjects = viewableProjects.Count(p =>
                    p.ProjectTasks.Count > 0 &&
                    p.ProjectTasks.All(t => t.BoardId == SystemBoards.DoneId || t.Board.BoardName == SystemBoards.DoneName)),
                TotalTasks = viewableAssignedTasks.Count,
                CompletedTasks = viewableAssignedTasks.Count(t =>
                    t.BoardId == SystemBoards.DoneId || t.Board.BoardName == SystemBoards.DoneName)
            };
        }

        public async Task<CustomResponseDto<UserProfileDto>> GetProfileForUserAsync(int userId)
        {
            var userProfileDto = await GetUserProfileAsync(userId);
            if (userProfileDto is null)
            {
                return CustomResponseDto<UserProfileDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            return CustomResponseDto<UserProfileDto>.Success(userProfileDto, 200);
        }

        public async Task<CustomResponseDto<TokenDto>> GetUserByTokenAsync(Token token)
        {
            var user = await GetByIdAsync(token.UserId);
            if (user is null)
            {
                return CustomResponseDto<TokenDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            var tokenDto = mapper.Map<TokenDto>(token);
            return CustomResponseDto<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<CustomResponseDto<UserUpdateDto>> GetEditMetaAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user is null)
            {
                return CustomResponseDto<UserUpdateDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            var userUpdateDto = mapper.Map<UserUpdateDto>(user);
            userUpdateDto.ProfilePicture = FormatProfilePicture(user.ProfilePicture);
            return CustomResponseDto<UserUpdateDto>.Success(userUpdateDto, 200);
        }

        public async Task<CustomResponseDto<NoContentDto>> UpdateUserProfileAsync(int userId, UserUpdateDto userDto)
        {
            var existingUser = await GetByIdAsync(userId);
            if (existingUser is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                var existingEmailUser = await GetByEmailAsync(userDto.Email);
                if (existingEmailUser != null && existingEmailUser.Id != userId)
                {
                    return CustomResponseDto<NoContentDto>.Fail(409, ErrorMessages.EmailAlreadyExistsForUpdate);
                }
            }

            existingUser.Name = userDto.Name;
            existingUser.Email = userDto.Email;

            if (!string.IsNullOrEmpty(userDto.ProfilePicture))
            {
                existingUser.ProfilePicture = Convert.FromBase64String(userDto.ProfilePicture);
            }

            await UpdateAsync(existingUser);
            return CustomResponseDto<NoContentDto>.Success(204);
        }

        public async Task<CustomResponseDto<NoContentDto>> RemovePictureAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            user.ProfilePicture = null;
            await UpdateAsync(user);
            return CustomResponseDto<NoContentDto>.Success(204);
        }

        public async Task<CustomResponseDto<TokenDto?>> ChangePasswordAsync(int userId, UserPasswordUpdateDto dto, string? currentRefreshToken)
        {
            var existingUser = await GetByIdAsync(userId);
            if (existingUser is null)
            {
                return CustomResponseDto<TokenDto?>.Fail(404, ErrorMessages.UserNotFound);
            }

            if (string.IsNullOrEmpty(dto.Password) || string.IsNullOrEmpty(dto.NewPassword))
            {
                return CustomResponseDto<TokenDto?>.Fail(400, ErrorMessages.PasswordsRequired);
            }

            if (!passwordHasher.VerifyPassword(dto.Password, existingUser.Password))
            {
                return CustomResponseDto<TokenDto?>.Fail(400, ErrorMessages.CurrentPasswordIncorrect);
            }

            existingUser.Password = passwordHasher.HashPassword(dto.NewPassword);
            await UpdateAsync(existingUser);

            var rotationResult = await authService.LogoutAllAsync(userId, currentRefreshToken);
            return CustomResponseDto<TokenDto?>.Success(rotationResult.Data, 204);
        }

        public async Task<CustomResponseDto<List<PendingInviteDto>>> GetPendingInvitesAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user is null)
            {
                return CustomResponseDto<List<PendingInviteDto>>.Fail(404, ErrorMessages.UserNotFound);
            }

            var pendingInvites = await teamInviteRepository.GetPendingInvitesByEmailAsync(user.Email);
            var inviteDtos = mapper.Map<List<PendingInviteDto>>(pendingInvites);

            return CustomResponseDto<List<PendingInviteDto>>.Success(inviteDtos, 200);
        }

        public Task<CustomResponseDto<NoContentDto>> AcceptInviteAsync(int userId, int inviteId) =>
            ProcessInviteAsync(userId, inviteId, accept: true);

        public Task<CustomResponseDto<NoContentDto>> RejectInviteAsync(int userId, int inviteId) =>
            ProcessInviteAsync(userId, inviteId, accept: false);

        private async Task<CustomResponseDto<NoContentDto>> ProcessInviteAsync(int userId, int inviteId, bool accept)
        {
            var user = await GetByIdAsync(userId);
            if (user is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.UserNotFound);
            }

            var invite = await teamInviteRepository.GetInviteWithTeamByIdAndEmailAsync(inviteId, user.Email);
            if (invite is null)
            {
                return CustomResponseDto<NoContentDto>.Fail(404, ErrorMessages.InviteNotFound);
            }

            if (invite.Status != TeamInvite.InviteStatus.Pending)
            {
                return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InviteAlreadyProcessed);
            }

            try
            {
                if (accept)
                {
                    var existingTeamUser = await teamUserRepository
                        .Where(x => x.TeamId == invite.TeamId && x.UserId == user.Id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    if (existingTeamUser != null)
                    {
                        return CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.UserAlreadyTeamMember);
                    }

                    var defaultRole = await roleRepository.GetDefaultRoleAsync();
                    if (defaultRole == null)
                    {
                        return CustomResponseDto<NoContentDto>.Fail(500, ErrorMessages.DefaultRolesNotFound);
                    }

                    await teamUserRepository.AddAsync(new TeamUser
                    {
                        TeamId = invite.TeamId,
                        UserId = user.Id,
                        RoleId = defaultRole.Id
                    });

                    invite.Status = TeamInvite.InviteStatus.Accepted;
                    invite.StatusChangeNote = $"Accepted by {user.Name}";
                }
                else
                {
                    invite.Status = TeamInvite.InviteStatus.Rejected;
                    invite.StatusChangeNote = $"Rejected by {user.Name}";
                }

                invite.UpdatedDate = DateTime.UtcNow;
                teamInviteRepository.Update(invite);
                await UnitOfWork.CommitAsync();

                return CustomResponseDto<NoContentDto>.Success(204);
            }
            catch
            {
                return CustomResponseDto<NoContentDto>.Fail(
                    500,
                    accept ? ErrorMessages.FailedToAcceptInvite : ErrorMessages.FailedToRejectInvite);
            }
        }
    }
}
