using Application.Common.Models.Request;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Family.Queries.CheckExist
{
    public record CheckFamilyExistQuery : BaseCheckExistQuery, IRequest<Response<bool>>
    {
        public Guid? Id { get; init; }
        public string? FamilyName { get; init; }
    }
}
