using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auth.Queries.GetMyPermissions
{
    /// <summary>
    /// Returns the permission names the current user's role is granted, e.g.
    /// <c>["GET_FAMILY","CREATE_FAMILY",...]</c>. The admin UI calls this once
    /// after sign-in (and again on page reload) and uses the result to hide
    /// nav items / actions the user can't perform — instead of letting the
    /// client send a request that the server then 500s on with
    /// UnauthorizedAccessException.
    /// </summary>
    public class GetMyPermissionsQueryHandler(
        ICurrentUserService currentUserService,
        IUserRolePermissionRepository permissionRepository
    ) : IRequestHandler<GetMyPermissionsQuery, Response<List<string>>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IUserRolePermissionRepository _permissionRepository = permissionRepository;

        public async Task<Response<List<string>>> Handle(GetMyPermissionsQuery request, CancellationToken cancellationToken)
        {
            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");

            var permissions = await _permissionRepository
                .Query(p => p.UserRoleId == user.RoleId, cancellationToken)
                .Select(p => p.Permission)
                .ToListAsync(cancellationToken);

            // Project to strings so the wire format is stable even if the
            // enum's int values shift.
            return Response<List<string>>.Ok(permissions.Select(p => p.ToString()).ToList());
        }
    }
}
