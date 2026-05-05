using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.Confirmation
{
    public record ResendConfirmationCommand : IRequest<Response<bool>>
    {
        public string Email { get; init; } = string.Empty;
    }
}
