using Application.Features.UserRole.Commands.Create;
using Application.Features.UserRole.Commands.Delete;
using Application.Features.UserRole.Commands.Update;
using Application.Features.UserRole.Queries.GetList;
using Application.Features.UserRole.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController(IMediator mediator) 
        : BaseServiceController<
            CreateUserRoleCommand,
            UpdateUserRoleCommand,
            DeleteUserRoleCommand,
            GetUserRoleQuery,
            GetUserRoleListQuery>(mediator)
    {
    }
}
