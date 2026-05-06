using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;

namespace Application.Features.Auth.Commands.LeaveFamily
{
    public record LeaveFamilyCommand : IRequest<Response<UserViewModel>>;
}
