﻿using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class UserService(IGenericRepository<User> repository, IUnitOfWork unitOfWork, IUserRepository userRepository, ITokenHandler tokenHandler) : GenericService<User>(repository, unitOfWork), IUserService
    {
        private readonly IUserRepository _repository = userRepository;
        private readonly ITokenHandler _tokenHandler = tokenHandler;

        public async Task<User?> GetByEmail(string email)
        {
            User? user = await _repository.Where(u => u.Email == email)
                .FirstOrDefaultAsync();
            return user;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _repository.GetUserWithTeamsProjectsTasksAsync(userId);
            if (user is null)
            {
                return null;
            }

            var userTeam = user.TeamUsers.FirstOrDefault();
            var userProfileDto = new UserProfileDto
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

            return userProfileDto;
        }

        public async Task<Token?> Login(UserLoginDto userLoginDto)
        {
            var user = await GetByEmail(userLoginDto.Email);
            if (user == null)
            {
                return null;
            }

            var result = BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password);
            List<Role> roles = [];
            if (result)
            {
                Token token = _tokenHandler.CreateToken(user, roles);
                return token;
            }
            else
            {
                return null;
            }
        }
    }
}
