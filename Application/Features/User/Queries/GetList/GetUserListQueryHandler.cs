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
using UserEntity = Domain.Entities.User;

namespace Application.Features.User.Queries.GetList
{
    public class GetUserListQUeryHandler(
        IUserService userService
        ) : IRequestHandler<GetUserListQuery, Response<List<UserViewModel>>>
    {
        private readonly IUserService _userService = userService;

        public async Task<Response<List<UserViewModel>>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<UserEntity, bool>>? predicate =
                request.Filters.BuildPredicate<UserEntity>()
                    .AndAlso(FilterExpressionBuilder.BuildSearchPredicate<UserEntity>(
                        request.SearchText,
                        nameof(UserEntity.FirstName),
                        nameof(UserEntity.LastName),
                        nameof(UserEntity.UserName),
                        nameof(UserEntity.Email),
                        nameof(UserEntity.Phone)));

            return await _userService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
