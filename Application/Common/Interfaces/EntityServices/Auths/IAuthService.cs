using Application.Common.Models;
using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices.Auths
{
    public interface IAuthService
    {
        Task<TokenViewModel> SignInAsync(SignInDto signInDto, CancellationToken cancellationToken);
        Task<bool> SignUpAsync(CreateUserDto signUpDto, CancellationToken cancellationToken);
        Task<bool> ResetAsync(ResetSignInDto resetSignInDto, CancellationToken cancellationToken);
        Task<TokenViewModel> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<bool> SignOutAsync(string token, CancellationToken cancellationToken);
        Task<bool> SendEmailAsync(string email, CancellationToken cancellationToken);

        // ─── Email confirmation ─────────────────────────────────────────
        /// <summary>Confirms a user's email by exchanging the raw token from the
        /// email link for the stored hash. Single-use — clears the token on success.</summary>
        Task<bool> ConfirmEmailAsync(string rawToken, CancellationToken cancellationToken);

        /// <summary>Re-issues a fresh confirmation token and emails it. Used by
        /// both the public "resend" form and the in-app Settings → Resend button.</summary>
        Task<bool> ResendConfirmationAsync(string email, CancellationToken cancellationToken);
    }
}
