using Application.Common.Interfaces;
using Application.Common.Models.Result;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

            var token = new JwtSecurityToken(
                _configuration.ValidIssuer,
                _configuration.ValidAudience,
                userClaims.Concat(jwtClaim),
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();

            return new TokenViewModel
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = string.Empty
            };
        }

    }
}
