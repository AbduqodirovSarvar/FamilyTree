using Application.Features.UserRolePermission.Commands.Create;
using Application.Features.UserRolePermission.Commands.Delete;
using Application.Features.UserRolePermission.Commands.Update;
using Application.Features.UserRolePermission.Queries.GetList;
using Application.Features.UserRolePermission.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolePermissionController(IMediator mediator) 
        : BaseServiceController<
            CreateUserRolePermissionCommand,
            UpdateUserRolePermissionCommand,
            DeleteUserRolePermissionCommand,
            GetUserRolePermissionQuery,
            GetUserRolePermissionListQuery>(mediator)
    {
    }
}
