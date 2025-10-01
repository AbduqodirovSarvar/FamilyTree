using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Result;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices.Auths
{
    public class AuthService(
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        ITokenService tokenService,
        IHashService hashService,
        IEmailService emailService,
        IRedisService redisService,
        IMapper mapper
        ) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IHashService _hashService = hashService;
        private readonly IEmailService _emailService = emailService;
        private readonly IRedisService _redisService = redisService;
        private readonly IMapper _mapper = mapper;

        public Task<TokenViewModel> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ResetAsync(ResetSignInDto resetSignInDto, CancellationToken cancellationToken)
        {
            if(resetSignInDto.Password != resetSignInDto.ConfirmPassword)
                throw new ArgumentException("New password and confirm password do not match.");

            if(await _redisService.GetAsync<string>($"confirmation-code-for-{resetSignInDto.Email}") != resetSignInDto.ConfirmationCode.ToString())
                throw new UnauthorizedAccessException("Invalid confirmation code.");

            var user = await _userRepository.GetByUsernameAsync(resetSignInDto.Email, cancellationToken)
                            ?? throw new KeyNotFoundException("User with this email not found.");

            user.PasswordHash = _hashService.Hash(resetSignInDto.Password);
            var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
            return updatedUser != null;
        }

        public async Task<bool> SendEmailAsync(string email, CancellationToken cancellationToken)
        {
            _ = await _userRepository.GetByEmailAsync(email, cancellationToken)
                        ?? throw new KeyNotFoundException("User with this email not found.");

            var confirmationCode = new Random().Next(100000, 999999).ToString();
            await _redisService.SetAsync($"confirmation-code-for-{email}", confirmationCode);
            return await _emailService.SendEmailAsync(email, "Confirmation Code", $"Reset sign-in confirmation code: ${confirmationCode}");
        }

        public async Task<TokenViewModel> SignInAsync(SignInDto signInDto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(x => x.UserName == signInDto.Login 
                                                        || x.Email == signInDto.Login
                                                        || x.Phone == signInDto.Login, cancellationToken)
                       ?? throw new UnauthorizedAccessException("Invalid username or password.");

            if (!_hashService.Verify(signInDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, user.RoleId.ToString())
            };

            var accessToken = _tokenService.GenerateToken([.. claims]);

            return accessToken;
        }

        public Task<bool> SignOutAsync(string token, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> SignUpAsync(CreateUserDto signUpDto, CancellationToken cancellationToken)
        {
            if (signUpDto.Password != signUpDto.ConfirmPassword)
                throw new ArgumentException("Password and confirm password do not match.");

            var userRole = await _userRoleRepository.GetAsync(x => x.DesignedName == "NEW_USER", cancellationToken)
                                ?? throw new InvalidOperationException("Default user role not found.");

            var user = _mapper.Map<User>(signUpDto);

            user.RoleId = userRole.Id;

            user.PasswordHash = _hashService.Hash(signUpDto.Password);

            var result = await _userRepository.CreateAsync(user, cancellationToken);
            return result != null;
        }
    }
}
