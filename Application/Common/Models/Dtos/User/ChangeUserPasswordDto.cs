using Application.Common.Models.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Dtos.User
{
    public record ChangeUserPasswordDto : BaseUpdateDto
    {
        [Required(ErrorMessage = "Old password is required.")]
        public string OldPassword { get; init; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        public string NewPassword { get; init; } = null!;

        [Required(ErrorMessage = "Confirm password is required.")]
        public string ConfirmPassword { get; init; } = null!;
    }
}
