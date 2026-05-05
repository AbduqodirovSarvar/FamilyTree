using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.Confirmation
{
    public class ResendConfirmationCommandHandler(IAuthService authService)
        : IRequestHandler<ResendConfirmationCommand, Response<bool>>
    {
        private readonly IAuthService _authService = authService;

        public async Task<Response<bool>> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResendConfirmationAsync(request.Email, cancellationToken);
            return Response<bool>.Ok(result, "Confirmation email sent.");
        }
    }
}
