using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using Application.Services.EntityServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Queries.GetList
{
    public class GetUserRolePermissionListQueryHandler(
        IUserRolePermissionService userRolePermissionService
        ) : IRequestHandler<GetUserRolePermissionListQuery, Response<List<UserRolePermissionViewModel>>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;
        public async Task<Response<List<UserRolePermissionViewModel>>> Handle(GetUserRolePermissionListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.UserRolePermission, bool>>? predicate = null;
            if (request.Filters != null && request.Filters.Any())
            {
                predicate = request.Filters.BuildPredicate<Domain.Entities.UserRolePermission>();
            }

            return await _userRolePermissionService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to retrieve user roles.");
        }
    }
}
