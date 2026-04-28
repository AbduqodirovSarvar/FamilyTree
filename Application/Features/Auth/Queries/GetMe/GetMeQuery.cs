using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;

namespace Application.Features.Auth.Queries.GetMe
{
    public record GetMeQuery : IRequest<Response<UserViewModel>>
    {
    }
}
