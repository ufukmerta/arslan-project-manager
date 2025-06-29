using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.Services
{
    public interface IUserService : IGenericService<User>
    {
        Task<User?> GetByEmail(string email);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<Token?> Login(UserLoginDto userLoginDto);
    }
}
