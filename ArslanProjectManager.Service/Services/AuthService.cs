using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.DTOs.CreateDTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using AutoMapper;

namespace ArslanProjectManager.Service.Services
{
    public class AuthService(IUnitOfWork unitOfWork, IUserService userService, ITokenHandler tokenHandler, IMapper mapper) : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUserService _userService = userService;
        private readonly ITokenHandler _tokenHandler = tokenHandler;
        private readonly IMapper _mapper = mapper;
        public async Task<Token?> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _userService.GetByEmailAsync(userLoginDto.Email);
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

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TokenDto?> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomResponseDto<UserDto>> RegisterAsync(UserCreateDto userDto)
        {
            var existingUser = await _userService.AnyAsync(x => x.Email == userDto.Email);
            if (existingUser)
            {
                return CustomResponseDto<UserDto>.Fail(400, ErrorMessages.EmailAlreadyExists);
            }

            var user = _mapper.Map<User>(userDto);

            if (userDto.ProfilePicture is not null && userDto.ProfilePicture.Length > 0)
            {
                var byteArray = Convert.FromBase64String(userDto.ProfilePicture);
                user.ProfilePicture = byteArray;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _userService.AddAsync(user);
            await _unitOfWork.CommitAsync();
            var newUserDto = _mapper.Map<UserDto>(user);
            return CustomResponseDto<UserDto>.Success(newUserDto, 201);

        }
    }
}
