using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Commands.Create
{
    public class CreateUserRoleCommandHandler(
        IUserRoleService userRoleService
        ) : IRequestHandler<CreateUserRoleCommand, Response<UserRoleViewModel>>
    {
        private readonly IUserRoleService _userRoleService = userRoleService;
        public async Task<Response<UserRoleViewModel>> Handle(CreateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRoleService.CreateAsync(request, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create user role.");

            return Response<UserRoleViewModel>.Ok(result, "User role created successfully.");
        }
    }
}
