using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand : IRequest<Response<TokenViewModel>>
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
