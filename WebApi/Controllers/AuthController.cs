using Application.Features.Auth.Commands.Confirmation;
using Application.Features.Auth.Commands.Reset;
using Application.Features.Auth.Commands.SignIn;
using Application.Features.Auth.Commands.SignUp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : BaseController(mediator)
    {
        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromForm] SignUpCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetSignInCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("reset/{email}")]
        public async Task<IActionResult> GetConfirmationCode([FromRoute] string email)
        {
            if (email == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(new SendConfirmationCodeCommand() { Email = email});
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            return Ok();
        }
    }
}
