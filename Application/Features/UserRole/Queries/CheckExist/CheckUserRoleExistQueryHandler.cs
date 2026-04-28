using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.UserRole.Queries.CheckExist
{
    public class CheckUserRoleExistQueryHandler(IUserRoleService userRoleService)
        : IRequestHandler<CheckUserRoleExistQuery, Response<bool>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;

        public async Task<Response<bool>> Handle(CheckUserRoleExistQuery request, CancellationToken cancellationToken)
        {
            bool hasId = request.Id != null && request.Id != Guid.Empty;
            bool hasFamilyId = request.FamilyId != null && request.FamilyId != Guid.Empty;
            bool hasName = !string.IsNullOrWhiteSpace(request.Name);
            bool hasDesignedName = !string.IsNullOrWhiteSpace(request.DesignedName);

            if (!hasId && !hasName && !hasDesignedName)
                return Response<bool>.Fail("At least one of Id, Name, or DesignedName must be provided.");

            Expression<Func<Domain.Entities.UserRole, bool>> predicate = x =>
                (hasId && x.Id == request.Id!.Value)
                || (hasName
                        && x.Name == request.Name
                        && (!hasFamilyId || x.FamilyId == request.FamilyId))
                || (hasDesignedName && x.DesignedName == request.DesignedName);

            var exists = await _userRoleService.ExistsAsync(predicate, cancellationToken);
            return Response<bool>.Ok(exists);
        }
    }
}
