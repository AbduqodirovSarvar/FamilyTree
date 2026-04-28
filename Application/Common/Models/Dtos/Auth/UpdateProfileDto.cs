using Domain.Behaviours;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Dtos.Auth
{
    public abstract record UpdateProfileDto
    {
        public string? FirstName { get; init; } = null;
        public string? LastName { get; init; } = null;
        public string? UserName { get; init; } = null;

        [PhoneNumberValidation]
        public string? Phone { get; init; } = null;

        [EmailAddress]
        public string? Email { get; init; } = null;

        public IFormFile? Image { get; init; } = null;
    }
}
