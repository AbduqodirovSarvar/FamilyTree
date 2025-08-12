using Application.Common.Interfaces.EntityServices;
using Application.Common.Models;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Queries.GetOne
{
    public class GetUserRoleQueryHandler(
        IUserRoleService userRoleService
        ) : IRequestHandler<GetUserRoleQuery, Response<UserRoleViewModel>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;
        public async Task<Response<UserRoleViewModel>> Handle(GetUserRoleQuery request, CancellationToken cancellationToken)
        {
            var userRole = await _userRoleService.GetByIdAsync(request.Id, cancellationToken)
                                        ?? throw new InvalidOperationException("User role not found.");

            return Response<UserRoleViewModel>.Ok(userRole, "User role retrieved successfully.");
        }
    }
}
