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

        public TokenViewModel GetToken(Claim[] claims)
        {
            Claim[] jwtClaim =
            [
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, DateTime.UtcNow.ToString()),
            ];

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Secret)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                _configuration.ValidIssuer,
                _configuration.ValidAudience,
                claims.Concat(jwtClaim),
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();

            //var refreshClaims = new[]
            //{
            //    new Claim("type", "refresh"),
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            //};

            //var refreshToken = new JwtSecurityToken(
            //    issuer: config.ValidIssuer,
            //    audience: config.ValidAudience,
            //    claims: refreshClaims,
            //    expires: DateTime.UtcNow.AddDays(7),
            //    signingCredentials: credentials
            //);

            //var refreshTokenString = new JwtSecurityTokenHandler().WriteToken(refreshToken);

            return new TokenViewModel
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = string.Empty
            };
        }
    }
}
