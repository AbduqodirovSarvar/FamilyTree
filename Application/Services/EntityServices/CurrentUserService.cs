using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Username { get; set; } = null;
        public string? Role { get; set; } = null;
        public Dictionary<string, string> AllClaims { get; set; } = [];

        private readonly IUserRepository _userRepository;

        public CurrentUserService(
            IHttpContextAccessor contextAccessor,
            IUserRepository userRepository
            )
        {
            _userRepository = userRepository;
            var httpContext = contextAccessor.HttpContext;
            if (httpContext == null)
                return;
            var userClaims = httpContext?.User.Claims;

            if (userClaims != null)
            {
                foreach (var claim in userClaims)
                {
                    AllClaims[claim.Type] = claim.Value;
                }

                var idClaim = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (idClaim != null && Guid.TryParse(idClaim.Value, out Guid value))
                    UserId = value;

                Email = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                Username = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                Role = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            }
        }

        public async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            if(UserId != null)
                return await _userRepository.GetByIdAsync(UserId.Value, cancellationToken);

            if (!string.IsNullOrWhiteSpace(Email))
                return await _userRepository.GetAsync(x => x.Email == Email, cancellationToken);

            if (!string.IsNullOrWhiteSpace(Username))
                return await _userRepository.GetAsync(x => x.UserName == Username, cancellationToken);

            return null;
        }
    }
}
