using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.Family.Queries.CheckExist
{
    public class CheckFamilyExistQueryHandler(IFamilyService familyService)
        : IRequestHandler<CheckFamilyExistQuery, Response<bool>>
    {
        private readonly IFamilyService _familyService = familyService;

        public async Task<Response<bool>> Handle(CheckFamilyExistQuery request, CancellationToken cancellationToken)
        {
            if ((request.Id == null || request.Id == Guid.Empty) && string.IsNullOrWhiteSpace(request.FamilyName))
                return Response<bool>.Fail("At least one of Id or FamilyName must be provided.");

            // Familiya nomini registrga sezgir bo'lmagan holda solishtiramiz —
            // "Xolmurodov" va "xolmurodov" bir xil oila deb hisoblanadi.
            var normalizedFamilyName = string.IsNullOrWhiteSpace(request.FamilyName)
                ? null
                : request.FamilyName.Trim().ToLower();

            Expression<Func<Domain.Entities.Family, bool>> predicate = x =>
                (request.Id != null && request.Id != Guid.Empty && x.Id == request.Id!.Value)
                || (normalizedFamilyName != null && x.FamilyName.ToLower() == normalizedFamilyName);

            var exists = await _familyService.ExistsAsync(predicate, cancellationToken);
            return Response<bool>.Ok(exists);
        }
    }
}
