using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.UserRolePermission.Queries.CheckExist
{
    public class CheckUserRolePermissionExistQueryHandler(IUserRolePermissionService userRolePermissionService)
        : IRequestHandler<CheckUserRolePermissionExistQuery, Response<bool>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;

        public async Task<Response<bool>> Handle(CheckUserRolePermissionExistQuery request, CancellationToken cancellationToken)
        {
            bool hasId = request.Id != null && request.Id != Guid.Empty;
            bool hasUserRoleId = request.UserRoleId != null && request.UserRoleId != Guid.Empty;
            bool hasPermission = request.Permission.HasValue;

            if (!hasId && !hasUserRoleId)
                return Response<bool>.Fail("At least one of Id or UserRoleId must be provided.");

            Expression<Func<Domain.Entities.UserRolePermission, bool>> predicate = x =>
                (hasId && x.Id == request.Id!.Value)
                || (hasUserRoleId
                        && x.UserRoleId == request.UserRoleId!.Value
                        && (!hasPermission || x.Permission == request.Permission!.Value));

            var exists = await _userRolePermissionService.ExistsAsync(predicate, cancellationToken);
            return Response<bool>.Ok(exists);
        }
    }
}
