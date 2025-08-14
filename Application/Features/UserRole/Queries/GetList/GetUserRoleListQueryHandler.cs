using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Queries.GetList
{
    public class GetUserRoleListQueryHandler(
        IUserRoleService userRoleService
        ) : IRequestHandler<GetUserRoleListQuery, Response<List<UserRoleViewModel>>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;
        public async Task<Response<List<UserRoleViewModel>>> Handle(GetUserRoleListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.UserRole, bool>>? predicate = null;
            if (request.Filters != null && request.Filters.Any())
            {
                predicate = request.Filters.BuildPredicate<Domain.Entities.UserRole>();
            }

            return await _userRoleService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to retrieve user roles.");
        }
    }
}
