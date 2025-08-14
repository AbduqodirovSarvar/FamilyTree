using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Commands.Update
{
    public class UpdateUserRolePermissionCommandHandler(
        IUserRolePermissionService userRolePermissionService
        ) : IRequestHandler<UpdateUserRolePermissionCommand, Response<UserRolePermissionViewModel>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;
        public async Task<Response<UserRolePermissionViewModel>> Handle(UpdateUserRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRolePermissionService.UpdateAsync(request, cancellationToken)
                                     ?? throw new InvalidOperationException("User role permission update failed.");

            return Response<UserRolePermissionViewModel>.Ok(result, "User role permission updated successfully.");
        }
    }
}
