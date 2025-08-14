using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Reset
{
    public class ResetSignInCommandHandler(
        IAuthService authService
        ) : IRequestHandler<ResetSignInCommand, Response<bool>>
    {
        private readonly IAuthService _authService = authService;
        public async Task<Response<bool>> Handle(ResetSignInCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResetAsync(request, cancellationToken);
            if (result)
            {
                return Response<bool>.Ok(true, "Sign-in reset successfully.");
            }
            return Response<bool>.Fail("Failed to reset sign-in.");
        }
    }
}
