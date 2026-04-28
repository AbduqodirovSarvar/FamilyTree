using Application.Common.Models.Request;
using Application.Common.Models.Result;
using Domain.Enums;
using MediatR;

namespace Application.Features.UserRolePermission.Queries.CheckExist
{
    public record CheckUserRolePermissionExistQuery : BaseCheckExistQuery, IRequest<Response<bool>>
    {
        public Guid? Id { get; init; }
        public Guid? UserRoleId { get; init; }
        public Permission? Permission { get; init; }
    }
}
