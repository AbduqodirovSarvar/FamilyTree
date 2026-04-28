using Application.Common.Models.Request;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.User.Queries.CheckExist
{
    public record CheckUserExistQuery : BaseCheckExistQuery, IRequest<Response<bool>>
    {
        public Guid? Id { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
    }
}
