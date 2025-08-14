using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.SignIn
{
    public class SignInCommandHandler(
        IAuthService authService
        ) : IRequestHandler<SignInCommand, Response<SignInViewModel>>
    {
        private readonly IAuthService _authService = authService;
        public async Task<Response<SignInViewModel>> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.SignInAsync(request, cancellationToken)
                                    ?? throw new InvalidOperationException("Sign-in failed.");

            return Response<SignInViewModel>.Ok(result, "Sign-in successful.");
        }
    }
}
