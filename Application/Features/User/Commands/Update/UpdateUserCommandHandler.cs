using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Commands.Update
{
    public class UpdateUserCommandHandler(
        IUserService userService
        ) : IRequestHandler<UpdateUserCommand, Response<UserViewModel>>
    {
        private readonly IUserService _userService = userService;
        public async Task<Response<UserViewModel>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateAsync(request, cancellationToken)
                                ?? throw new InvalidOperationException("User update failed.");

            return Response<UserViewModel>.Ok(result, "User updated successfully.");
        }
    }
}
