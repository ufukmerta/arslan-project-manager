using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ArslanProjectManager.Service.Services
{
    public class UserService : GenericService<User>, IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ITokenHandler _tokenHandler;
        public UserService(IGenericRepository<User> repository, IUnitOfWork unitOfWork, IUserRepository userRepository, ITokenHandler tokenHandler) : base(repository, unitOfWork)
        {
            _repository = userRepository;
            _tokenHandler = tokenHandler;
        }

        public async Task<User?> GetByEmail(string email)
        {
            User? user = await _repository.Where(u => u.Email == email)
                .FirstOrDefaultAsync();
            return user;
        }

        public async Task<User?> GetUserWithDetailsAsync(int userId)
        {            
            User? user = await _repository.GetUserWithTeamsProjectsTasksAsync(userId);
            return user;
        }

        public async Task<Token?> Login(UserLoginDto userLoginDto)
        {
            Token token = new Token();
            var user = await GetByEmail(userLoginDto.Email);
            if (user == null)
            {
                return null;
            }

            var result = BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password);
            List<Role> roles = new List<Role>();
            if (result)
            {
                token = _tokenHandler.CreateToken(user, roles);
                return token;
            }
            else
            {
                return null;
            }
        }
    }
}
