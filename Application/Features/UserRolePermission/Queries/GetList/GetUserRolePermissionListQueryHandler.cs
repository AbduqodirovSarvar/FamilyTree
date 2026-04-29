using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserRolePermissionEntity = Domain.Entities.UserRolePermission;

namespace Application.Features.UserRolePermission.Queries.GetList
{
    public class GetUserRolePermissionListQueryHandler(
        IUserRolePermissionService userRolePermissionService
        ) : IRequestHandler<GetUserRolePermissionListQuery, Response<List<UserRolePermissionViewModel>>>
    {
        private readonly IUserRolePermissionService _userRolePermissionService = userRolePermissionService;

        public async Task<Response<List<UserRolePermissionViewModel>>> Handle(GetUserRolePermissionListQuery request, CancellationToken cancellationToken)
        {
            // UserRolePermission has no obvious string fields to search; SearchText
            // is silently ignored here to keep the contract uniform across handlers.
            Expression<Func<UserRolePermissionEntity, bool>>? predicate = request.Filters.BuildPredicate<UserRolePermissionEntity>();

            return await _userRolePermissionService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to retrieve user role permissions.");
        }
    }
}
