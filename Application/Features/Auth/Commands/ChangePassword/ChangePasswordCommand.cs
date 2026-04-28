using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.ChangePassword
{
    public record ChangePasswordCommand : ChangePasswordDto, IRequest<Response<bool>>
    {
    }
}
