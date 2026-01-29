using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class UserService(IGenericRepository<User> repository, IUnitOfWork unitOfWork, IUserRepository userRepository) : GenericService<User>(repository, unitOfWork), IUserService
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            User? user = await userRepository.Where(u => u.Email == email)
                .FirstOrDefaultAsync();
            return user;
        }
        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await userRepository.GetUserWithTeamsProjectsTasksAsync(userId);
            if (user is null)
            {
                return null;
            }
            UserProfileDto userProfileDto;
            var userTeam = user.TeamUsers.FirstOrDefault();
            //if the userTeam is null, it means the user is not part of any team. So, just return the profile without team/project details.
            if (userTeam == null)
            {
                userProfileDto = new UserProfileDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture != null ? Convert.ToBase64String(user.ProfilePicture).Insert(0, "data:image/png;base64,") : "/img/profile.png",
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
            else
            {
                userProfileDto = new UserProfileDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture != null ? Convert.ToBase64String(user.ProfilePicture).Insert(0, "data:image/png;base64,") : "/img/profile.png",
                    RegisterDate = user.CreatedDate,
                    OwnProfile = true,
                    CurrentTeam = userTeam!.Team.TeamName,
                    Role = user.Teams.Count != 0 ? "Team Manager" : "Team Member",
                    TotalProjects = user.TeamUsers
                        .SelectMany(tu => tu.Team.Projects)
                        .Count(),
                    CompletedProjects = user.TeamUsers
                        .SelectMany(tu => tu.Team.Projects)
                        .Count(p => p.ProjectTasks
                        .All(t => t.Board.BoardName == "Done")),
                    TotalTasks = user.TeamUsers
                        .SelectMany(tu => tu.Team.Projects)
                        .SelectMany(p => p.ProjectTasks)
                        .Count(t => t.AppointeeId == userTeam!.Id),
                    CompletedTasks = user.TeamUsers
                        .SelectMany(tu => tu.Team.Projects)
                        .SelectMany(p => p.ProjectTasks)
                        .Count(t => t.AppointeeId == userTeam!.Id && t.Board.BoardName == "Done")
                };
            }

            return userProfileDto;
        }
    }
}
