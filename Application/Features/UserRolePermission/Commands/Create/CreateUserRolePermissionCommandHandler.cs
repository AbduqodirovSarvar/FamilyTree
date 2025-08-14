using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Commands.Create
{
    public class CreateUserRolePermissionCommandHandler(
        IUserRolePermissionService userRolePermissionService
        ) : IRequestHandler<CreateUserRolePermissionCommand, Response<UserRolePermissionViewModel>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;
        public async Task<Response<UserRolePermissionViewModel>> Handle(CreateUserRolePermissionCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRolePermissionService.CreateAsync(request, cancellationToken)
                                    ?? throw new InvalidOperationException("User role permission creation failed.");

            return Response<UserRolePermissionViewModel>.Ok(result, "User role permission created successfully.");
        }
    }
}
