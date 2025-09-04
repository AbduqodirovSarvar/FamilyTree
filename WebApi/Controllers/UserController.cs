using Application.Common.Models.Dtos.Common;
using Application.Features.User.Commands.Delete;
using Application.Features.User.Commands.Update;
using Application.Features.User.Queries.GetList;
using Application.Features.User.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IMediator mediator) 
        : BaseServiceController<
                    BaseCreateDto,
                    UpdateUserCommand,
                    DeleteUserCommand,
                    GetUserQuery,
                    GetUserListQuery>(mediator)
    {
        public override Task<IActionResult> Post([FromForm] BaseCreateDto command)
        {
            throw new NotImplementedException("User creation is not supported via this endpoint.");
        }
    }
}
