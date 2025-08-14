using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Queries.GetOne
{
    public class GetUserRolePermissionQueryHandler(
        IUserRolePermissionService userRolePermissionService
        ) : IRequestHandler<GetUserRolePermissionQuery, Response<UserRolePermissionViewModel>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;
        public async Task<Response<UserRolePermissionViewModel>> Handle(GetUserRolePermissionQuery request, CancellationToken cancellationToken)
        {
            var result = await _userRolePermissionService.GetByIdAsync(request.Id, cancellationToken)
                                ?? throw new InvalidOperationException("User role permission not found.");

            return Response<UserRolePermissionViewModel>.Ok(result, "User role permission retrieved successfully.");
        }
    }
}
