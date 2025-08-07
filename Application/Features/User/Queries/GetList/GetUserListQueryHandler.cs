using Application.Common.Interfaces.EntityServices;
using Application.Common.Models;
using Application.Common.Models.Result;
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
        IUserService userService,
        IMapper mapper
        ) : IRequestHandler<GetUserListQuery, Response<List<UserViewModel>>>
    {
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;
        public async Task<Response<List<UserViewModel>>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.User, bool>>? predicate = null;
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.ToLower();

                predicate = user =>
                    user.FirstName != null && user.FirstName.ToLower().Contains(search) ||
                    user.LastName != null && user.LastName.ToLower().Contains(search) ||
                    user.UserName != null && user.UserName.ToLower().Contains(search) ||
                    user.Email != null && user.Email.ToLower().Contains(search) ||
                    user.Phone != null && user.Phone.ToLower().Contains(search);
            }

            return await _userService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
