using Application.Common.Models;
using Application.Common.Models.Dtos.Auth;
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
        Task<bool> SignUpAsync(SignUpDto signUpDto, CancellationToken cancellationToken);
        Task<bool> ResetAsync(ResetSignInDto resetSignInDto, CancellationToken cancellationToken);
        Task<TokenViewModel> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<bool> SignOutAsync(string token, CancellationToken cancellationToken);
        Task<bool> SendEmailAsync(string email, CancellationToken cancellationToken);
    }
}
