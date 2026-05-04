using Application.Common.Interfaces;
using Application.Common.Models.Result;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Persistence.Services
{
    public class TokenService(IOptions<JWTConfiguration> config) : ITokenService
    {
        private readonly JWTConfiguration _configuration = config.Value;

        public TokenViewModel GenerateToken(Claim[] userClaims)
        {
            var jwtClaim = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Secret)),
                SecurityAlgorithms.HmacSha256
            );

            var accessExpiry = DateTime.UtcNow.AddMinutes(_configuration.AccessTokenExpirationMinutes);
            var refreshExpiry = GetRefreshTokenExpiry();

            var token = new JwtSecurityToken(
                _configuration.ValidIssuer,
                _configuration.ValidAudience,
                userClaims.Concat(jwtClaim),
                expires: accessExpiry,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();

            return new TokenViewModel
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = GenerateRefreshToken(),
                AccessTokenExpiration = accessExpiry,
                RefreshTokenExpiration = refreshExpiry,
                UserId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            };
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public DateTime GetRefreshTokenExpiry()
            => DateTime.UtcNow.AddDays(_configuration.RefreshTokenExpirationDays);
    }
}
