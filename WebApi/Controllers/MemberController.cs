using Application.Features.Member.Commands.Create;
using Application.Features.Member.Commands.Delete;
using Application.Features.Member.Commands.Update;
using Application.Features.Member.Queries.GetList;
using Application.Features.Member.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController(IMediator mediator) 
        : BaseServiceController<
            CreateMemberCommand,
            UpdateMemberCommand,
            DeleteMemberCommand,
            GetMemberQuery,
            GetMemberListQuery>(mediator)
    {
    }
}
