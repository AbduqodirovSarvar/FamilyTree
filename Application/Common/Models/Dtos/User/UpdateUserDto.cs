using Application.Common.Models.Dtos.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.User
{
    public record UpdateUserDto : BaseUpdateDto
    {
        public string? FirstName { get; init; } = null;
        public string? LastName { get; init; } = null;
        public string? UserName { get; init; } = null;
        public string? Phone { get; init; } = null;
        public string? Email { get; init; } = null;
        public Guid? FamilyId { get; init; } = null;
        public IFormFile? Image { get; init; } = null;
        public Guid? RoleId { get; init; } = null;
        public ChangeUserPasswordDto? Password { get; init; } = null;
        // Admin-only override — only callers with the full User permission set
        // (or the ADMIN role) are allowed to flip this flag. UserService enforces
        // the gate so a tampered DTO can't bypass email verification.
        public bool? EmailConfirmed { get; init; } = null;
    }
}
