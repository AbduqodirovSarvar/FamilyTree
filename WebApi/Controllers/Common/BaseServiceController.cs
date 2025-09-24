using Application.Common.Models.Dtos.Common;
using Application.Common.Models.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public abstract class BaseServiceController<TCreateCommand, TUpdateCommand, TDeleteCommand, TGetQuery, TGetAllQuery>(IMediator mediator)
        : BaseController(mediator)
        where TCreateCommand : BaseCreateDto
        where TUpdateCommand : BaseUpdateDto
        where TDeleteCommand : BaseDeleteDto
        where TGetQuery : BaseGetOneQuery
        where TGetAllQuery : BaseGetListQuery
    {
        [HttpPost]
        public virtual async Task<IActionResult> Post([FromForm] TCreateCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Put([FromForm] TUpdateCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete(TDeleteCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get([FromQuery] TGetQuery request)
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("list")]
        public virtual async Task<IActionResult> GetAll([FromQuery] TGetAllQuery request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
