using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.Member.Queries.CheckExist
{
    public class CheckMemberExistQueryHandler(IMemberService memberService)
        : IRequestHandler<CheckMemberExistQuery, Response<bool>>
    {
        private readonly IMemberService _memberService = memberService;

        public async Task<Response<bool>> Handle(CheckMemberExistQuery request, CancellationToken cancellationToken)
        {
            bool hasId = request.Id != null && request.Id != Guid.Empty;
            bool hasFamilyId = request.FamilyId != null && request.FamilyId != Guid.Empty;
            bool hasFirstName = !string.IsNullOrWhiteSpace(request.FirstName);
            bool hasLastName = !string.IsNullOrWhiteSpace(request.LastName);

            if (!hasId && !hasFamilyId && !hasFirstName && !hasLastName)
                return Response<bool>.Fail("At least one filter must be provided.");

            Expression<Func<Domain.Entities.Member, bool>> predicate = x =>
                (hasId && x.Id == request.Id!.Value)
                || (hasFamilyId && x.FamilyId == request.FamilyId!.Value
                        && (!hasFirstName || x.FirstName == request.FirstName)
                        && (!hasLastName || x.LastName == request.LastName));

            var exists = await _memberService.ExistsAsync(predicate, cancellationToken);
            return Response<bool>.Ok(exists);
        }
    }
}
