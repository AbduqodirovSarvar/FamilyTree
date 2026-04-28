using Application.Common.Models.Request;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Member.Queries.CheckExist
{
    public record CheckMemberExistQuery : BaseCheckExistQuery, IRequest<Response<bool>>
    {
        public Guid? Id { get; init; }
        public Guid? FamilyId { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}
