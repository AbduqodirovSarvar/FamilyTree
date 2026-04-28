using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;

namespace Application.Features.Auth.Commands.UpdateProfile
{
    public record UpdateProfileCommand : UpdateProfileDto, IRequest<Response<UserViewModel>>
    {
    }
}
