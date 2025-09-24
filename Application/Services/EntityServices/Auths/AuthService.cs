using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Result;
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
        ITokenService tokenService,
        IHashService hashService,
        IEmailService emailService,
        IRedisService redisService
        ) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IHashService _hashService = hashService;
        private readonly IEmailService _emailService = emailService;
        private readonly IRedisService _redisService = redisService;
        public Task<TokenViewModel> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ResetAsync(ResetSignInDto resetSignInDto, CancellationToken cancellationToken)
        {
            if(resetSignInDto.Password != resetSignInDto.ConfirmPassword)
                throw new ArgumentException("New password and confirm password do not match.");

            var user = await _userRepository.GetByUsernameAsync(resetSignInDto.Email, cancellationToken)
                            ?? throw new KeyNotFoundException("User with this email not found.");

            user.PasswordHash = _hashService.Hash(resetSignInDto.Password);
            var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
            return updatedUser != null;
        }

        public async Task<bool> SendEmailAsync(string email, CancellationToken cancellationToken)
        {
            var confirmationCode = new Random().Next(100000, 999999).ToString();
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

        public Task<bool> SignUpAsync(SignUpDto signUpDto, CancellationToken cancellationToken)
        {
            if (signUpDto.Password != signUpDto.ConfirmPassword)
                throw new ArgumentException("Password and confirm password do not match.");

            //if

            return Task.FromResult(true);
            throw new NotImplementedException();
        }
    }
}
