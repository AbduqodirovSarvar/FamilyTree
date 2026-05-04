using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler(
        IAuthService authService
        ) : IRequestHandler<RefreshTokenCommand, Response<TokenViewModel>>
    {
        private readonly IAuthService _authService = authService;

        public async Task<Response<TokenViewModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken)
                            ?? throw new InvalidOperationException("Token refresh failed.");

            return Response<TokenViewModel>.Ok(result, "Token refreshed successfully.");
        }
    }
}
