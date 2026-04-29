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
using UserRoleEntity = Domain.Entities.UserRole;

namespace Application.Features.UserRole.Queries.GetList
{
    public class GetUserRoleListQueryHandler(
        IUserRoleService userRoleService
        ) : IRequestHandler<GetUserRoleListQuery, Response<List<UserRoleViewModel>>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;

        public async Task<Response<List<UserRoleViewModel>>> Handle(GetUserRoleListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<UserRoleEntity, bool>>? predicate =
                request.Filters.BuildPredicate<UserRoleEntity>()
                    .AndAlso(FilterExpressionBuilder.BuildSearchPredicate<UserRoleEntity>(
                        request.SearchText,
                        nameof(UserRoleEntity.Name),
                        nameof(UserRoleEntity.DesignedName),
                        nameof(UserRoleEntity.Description)));

            return await _userRoleService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to retrieve user roles.");
        }
    }
}
