using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Queries.GetList
{
    public class GetUserListQUeryHandler(
        IUserService userService
        ) : IRequestHandler<GetUserListQuery, Response<List<UserViewModel>>>
    {
        private readonly IUserService _userService = userService;
        public async Task<Response<List<UserViewModel>>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.User, bool>>? predicate = null;
            if (request.Filters != null && request.Filters.Any())
            {
                predicate = request.Filters.BuildPredicate<Domain.Entities.User>();
            }

            return await _userService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
