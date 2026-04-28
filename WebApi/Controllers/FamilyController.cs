using Application.Features.Family.Commands.Create;
using Application.Features.Family.Commands.Delete;
using Application.Features.Family.Commands.Update;
using Application.Features.Family.Queries.CheckExist;
using Application.Features.Family.Queries.GetFamilyTree;
using Application.Features.Family.Queries.GetList;
using Application.Features.Family.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController(IMediator mediator)
        : BaseServiceController<
            CreateFamilyCommand,
            UpdateFamilyCommand,
            DeleteFamilyCommand,
            GetFamilyQuery,
            GetFamilyListQuery,
            CheckFamilyExistQuery>(mediator)
    {
        [HttpGet("tree/{familyId:guid}")]
        public async Task<IActionResult> GetTree(Guid familyId)
        {
            var result = await _mediator.Send(new GetFamilyTreeQuery { FamilyId = familyId });
            return Ok(result);
        }
    }
}
