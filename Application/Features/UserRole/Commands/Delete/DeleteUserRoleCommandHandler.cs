using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Commands.Delete
{
    public class DeleteUserRoleCommandHandler(
        IUserRoleService userRoleService
        ) : IRequestHandler<DeleteUserRoleCommand, Response<bool>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;
        public async Task<Response<bool>> Handle(DeleteUserRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRoleService.DeleteAsync(request.Id, cancellationToken);
            if (result)
            {
                return Response<bool>.Ok(true, "User role deleted successfully.");
            }
            return Response<bool>.Fail("Failed to delete user role.");
        }
    }
}
