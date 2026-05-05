using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.Confirmation
{
    public class ConfirmEmailCommandHandler(IAuthService authService)
        : IRequestHandler<ConfirmEmailCommand, Response<bool>>
    {
        private readonly IAuthService _authService = authService;

        public async Task<Response<bool>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.ConfirmEmailAsync(request.Token, cancellationToken);
            return Response<bool>.Ok(result, "Email confirmed.");
        }
    }
}
