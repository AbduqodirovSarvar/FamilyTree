using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Dtos.Auth
{
    public abstract record ChangePasswordDto
    {
        [Required(ErrorMessage = "Old password is required.")]
        public string OldPassword { get; init; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; init; } = null!;

        [Required(ErrorMessage = "Confirm password is required.")]
        public string ConfirmPassword { get; init; } = null!;
    }
}
