using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Create;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
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
        IMapper mapper,
        IMediator mediator,
        IConfiguration configuration
        ) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IHashService _hashService = hashService;
        private readonly IEmailService _emailService = emailService;
        private readonly IRedisService _redisService = redisService;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;
        private readonly IConfiguration _configuration = configuration;

        // Confirmation tokens are 256 bits of entropy, encoded URL-safe so they
        // can ride in a query string. Hash-only storage means a stolen DB
        // dump is useless without each victim's email link.
        private const int ConfirmationTokenBytes = 32;
        private static readonly TimeSpan ConfirmationTokenLifetime = TimeSpan.FromHours(24);

        public async Task<TokenViewModel> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedAccessException("Refresh token is required.");

            var user = await _userRepository.GetAsync(x => x.RefreshToken == refreshToken, cancellationToken)
                       ?? throw new UnauthorizedAccessException("Invalid refresh token.");

            if (user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            var claims = BuildUserClaims(user);
            var tokens = _tokenService.GenerateToken([.. claims]);

            // Rotate refresh token — invalidate the old one and persist the new pair.
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = tokens.RefreshTokenExpiration;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return tokens;
        }

        public async Task<bool> ResetAsync(ResetSignInDto resetSignInDto, CancellationToken cancellationToken)
        {
            if(resetSignInDto.Password != resetSignInDto.ConfirmPassword)
                throw new ArgumentException("New password and confirm password do not match.");

            if(await _redisService.GetAsync<string>($"confirmation-code-for-{resetSignInDto.Email}") != resetSignInDto.ConfirmationCode.ToString())
                throw new UnauthorizedAccessException("Invalid confirmation code.");

            // Look up by email (not username) — the DTO carries an email and
            // SendEmailAsync above keys the Redis confirmation code by email,
            // so the lookup must be consistent. Previously this called
            // GetByUsernameAsync, which queried Users.UserName with the email
            // value and always returned null → KeyNotFoundException.
            var user = await _userRepository.GetByEmailAsync(resetSignInDto.Email, cancellationToken)
                            ?? throw new KeyNotFoundException("User with this email not found.");

            user.PasswordHash = _hashService.Hash(resetSignInDto.Password);
            var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
            return updatedUser != null;
        }

        public async Task<bool> SendEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken)
                        ?? throw new KeyNotFoundException("User with this email not found.");

            // Block all outbound mail until the address is confirmed — otherwise
            // an attacker can register a stranger's address and use the password
            // reset flow as a free email-relay.
            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Email is not confirmed. Please confirm your email first.");

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

            var claims = BuildUserClaims(user);
            var tokens = _tokenService.GenerateToken([.. claims]);

            // Persist the refresh token so /auth/refresh-token can validate it later.
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = tokens.RefreshTokenExpiration;
            await _userRepository.UpdateAsync(user, cancellationToken);

            return tokens;
        }

        private static List<Claim> BuildUserClaims(Domain.Entities.User user)
        {
            return new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, user.RoleId.ToString())
            };
        }

        public async Task<bool> SignOutAsync(string refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return true;

            var user = await _userRepository.GetAsync(x => x.RefreshToken == refreshToken, cancellationToken);
            if (user is null) return true;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> SignUpAsync(CreateUserDto signUpDto, CancellationToken cancellationToken)
        {
            if (signUpDto.Password != signUpDto.ConfirmPassword)
                throw new ArgumentException("Password and confirm password do not match.");

            var userRole = await _userRoleRepository.GetAsync(x => x.DesignedName == "NEW_USER", cancellationToken)
                                ?? throw new InvalidOperationException("Default user role not found.");

            var user = _mapper.Map<User>(signUpDto);

            // Foydalanuvchi nomi registrdan qat'i nazar yagona bo'lishi kerak —
            // "Xolmurodov" band bo'lsa, "xolmurodov" bilan ro'yxatdan o'tib bo'lmaydi.
            var normalizedUserName = user.UserName?.Trim().ToLower();
            if (!string.IsNullOrEmpty(normalizedUserName)
                && await _userRepository.AnyAsync(u => u.UserName!.ToLower() == normalizedUserName, cancellationToken))
                throw new InvalidOperationException("Bu foydalanuvchi nomi allaqachon band. Iltimos, boshqa nom tanlang.");

            user.RoleId = userRole.Id;

            if (signUpDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = signUpDto.Image, Alt = user.FirstName, Description = null }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    user.ImageId = image.Data.Id;
            }

            user.PasswordHash = _hashService.Hash(signUpDto.Password);

            // Generate confirmation token before persisting so the user row
            // and the link in the email can never disagree.
            var (rawToken, hash) = GenerateConfirmationToken();
            user.EmailConfirmed = false;
            user.EmailConfirmationTokenHash = hash;
            user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.Add(ConfirmationTokenLifetime);

            var result = await _userRepository.CreateAsync(user, cancellationToken);
            if (result == null) return false;

            // Email failures should not roll back the account — the user can
            // request a resend from Settings. Log and move on.
            try
            {
                await SendConfirmationEmailAsync(user.Email, rawToken);
            }
            catch
            {
                // swallow — surfaced via the resend flow instead
            }

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string rawToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                throw new ArgumentException("Token is required.", nameof(rawToken));

            var hash = HashToken(rawToken);
            var user = await _userRepository.GetAsync(
                u => u.EmailConfirmationTokenHash == hash,
                cancellationToken)
                ?? throw new UnauthorizedAccessException("Invalid or expired confirmation link.");

            if (user.EmailConfirmed)
                return true; // idempotent — clicking the link twice is fine

            if (user.EmailConfirmationTokenExpiresAt is null
                || user.EmailConfirmationTokenExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired confirmation link.");

            user.EmailConfirmed = true;
            user.EmailConfirmedAt = DateTime.UtcNow;
            // Single-use: clear the hash so the link can't be replayed.
            user.EmailConfirmationTokenHash = null;
            user.EmailConfirmationTokenExpiresAt = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> ResendConfirmationAsync(string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken)
                ?? throw new KeyNotFoundException("User with this email not found.");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("Email is already confirmed.");

            var (rawToken, hash) = GenerateConfirmationToken();
            user.EmailConfirmationTokenHash = hash;
            user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.Add(ConfirmationTokenLifetime);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return await SendConfirmationEmailAsync(user.Email, rawToken);
        }

        // ─── Helpers ─────────────────────────────────────────────────

        private static (string raw, string hash) GenerateConfirmationToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(ConfirmationTokenBytes);
            // URL-safe base64 (no '+' '/' '=' so the value survives query strings).
            var raw = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return (raw, HashToken(raw));
        }

        private static string HashToken(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes);
        }

        private async Task<bool> SendConfirmationEmailAsync(string email, string rawToken)
        {
            var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/')
                          ?? throw new InvalidOperationException("App:FrontendBaseUrl is not configured.");
            var link = $"{baseUrl}/auth/confirm-email?token={rawToken}";

            var body =
                "<p>Hello,</p>" +
                "<p>Confirm your email address by clicking the link below. The link is valid for 24 hours and can be used once.</p>" +
                $"<p><a href=\"{link}\">Confirm email</a></p>" +
                $"<p>If the button doesn't work, copy and paste this URL into your browser:<br>{link}</p>" +
                "<p>If you didn't create an account, ignore this email.</p>";

            return await _emailService.SendEmailAsync(email, "Confirm your email", body);
        }
    }
}
