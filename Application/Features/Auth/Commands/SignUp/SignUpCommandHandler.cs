using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.SignUp
{
    public class SignUpCommandHandler(
        IAuthService authService
        ) : IRequestHandler<SignUpCommand, Response<bool>>
    {
        private readonly IAuthService _authService = authService;
        public async Task<Response<bool>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.SignUpAsync(request, cancellationToken);

            return Response<bool>.Ok(result, "Sign up successful.");
        }
    }
}
