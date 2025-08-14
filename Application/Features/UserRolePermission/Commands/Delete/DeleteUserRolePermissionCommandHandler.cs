using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Commands.Delete
{
    public class DeleteUserRolePermissionCommandHandler(
        IUserRolePermissionService userRolePermissionService
        ) : IRequestHandler<DeleteUserRolePermissionCommand, Response<bool>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;
        public async Task<Response<bool>> Handle(DeleteUserRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRolePermissionService.DeleteAsync(request.Id, cancellationToken);
            if (result)
            {
                return Response<bool>.Ok(true, "User role permission deleted successfully.");
            }
            return Response<bool>.Fail("Failed to delete user role permission.");
        }
    }
}
