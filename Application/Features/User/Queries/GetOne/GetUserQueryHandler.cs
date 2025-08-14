using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Queries.GetOne
{
    public class GetUserQueryHandler(
        IUserService userService
        ) : IRequestHandler<GetUserQuery, Response<UserViewModel>>
    {
        private readonly IUserService _userService = userService;
        public async Task<Response<UserViewModel>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            UserViewModel? result = null;
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                result = await _userService.GetByIdAsync(request.Id.Value, cancellationToken)
                             ?? throw new KeyNotFoundException("User not found.");
                return Response<UserViewModel>.Ok(result);
            }
            if (request.Email is not null)
            {
                result = await _userService.GetAsync(x => x.Email == request.Email, cancellationToken)
                             ?? throw new KeyNotFoundException("User not found.");
                return Response<UserViewModel>.Ok(result);
            }

            if (request.UserName is not null)
            {
                result = await _userService.GetAsync(x => x.UserName == request.UserName, cancellationToken)
                             ?? throw new KeyNotFoundException("User not found.");
                return Response<UserViewModel>.Ok(result);
            }

            result = await _userService.GetAsync(x => x.Phone == request.Phone, cancellationToken)
                             ?? throw new KeyNotFoundException("User not found.");
            return Response<UserViewModel>.Ok(result);
        }
    }
}
