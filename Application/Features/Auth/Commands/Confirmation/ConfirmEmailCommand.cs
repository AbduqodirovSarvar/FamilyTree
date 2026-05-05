using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.Confirmation
{
    public record ConfirmEmailCommand : IRequest<Response<bool>>
    {
        public string Token { get; init; } = string.Empty;
    }
}
