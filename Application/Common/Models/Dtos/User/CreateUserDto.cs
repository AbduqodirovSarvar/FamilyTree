using Application.Common.Models.Dtos.Common;
using Domain.Behaviours;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Dtos.User
{
    public record CreateUserDto : BaseCreateDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? UserName { get; init; } = null;
        [PhoneNumberValidation]
        public string? Phone { get; init; } = null;
        [EmailAddress]
        public string Email { get; init; } = "youremail@gmail.com";
        public string Password { get; init; } = null!;
        public string ConfirmPassword { get; init; } = null!;
        public Guid? FamilyId { get; init; } = null;
        public IFormFile? Image { get; init; } = null;
        public Guid? RoleId { get; init; } = null;
    }
}
