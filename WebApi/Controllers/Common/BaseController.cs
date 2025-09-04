using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController(IMediator mediator)
        : ControllerBase
    {
        protected readonly IMediator _mediator = mediator;
    }
}
