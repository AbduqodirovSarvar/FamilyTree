using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Commands.Update
{
    public class UpdateUserRoleCommandHandler(
        IUserRoleService userRoleService
        ) : IRequestHandler<UpdateUserRoleCommand, Response<UserRoleViewModel>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;
        public async Task<Response<UserRoleViewModel>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRoleService.UpdateAsync(request, cancellationToken)
                                 ?? throw new InvalidOperationException("User role update failed.");

            return Response<UserRoleViewModel>.Ok(result, "User role updated successfully.");
        }
    }
}
