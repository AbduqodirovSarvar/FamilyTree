using Application.Common.Models.Request;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.UserRole.Queries.CheckExist
{
    public record CheckUserRoleExistQuery : BaseCheckExistQuery, IRequest<Response<bool>>
    {
        public Guid? Id { get; init; }
        public Guid? FamilyId { get; init; }
        public string? Name { get; init; }
        public string? DesignedName { get; init; }
    }
}
