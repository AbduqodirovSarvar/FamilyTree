using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Confirmation
{
    public class SendConfirmationCodeCommandHandler(
        IAuthService authService
        ) : IRequestHandler<SendConfirmationCodeCommand, Response<bool>>
    {
        private readonly IAuthService _authService = authService;
        public async Task<Response<bool>> Handle(SendConfirmationCodeCommand request, CancellationToken cancellationToken)
        {
      

            var result = await _authService.SendEmailAsync(request.Email, cancellationToken);

            return Response<bool>.Ok(result, "Confirmation code sent successfully.");
        }
    }
}
