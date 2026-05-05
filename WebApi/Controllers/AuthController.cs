using Application.Features.Auth.Commands.ChangePassword;
using Application.Features.Auth.Commands.Confirmation;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Reset;
using Application.Features.Auth.Commands.SignIn;
using Application.Features.Auth.Commands.SignUp;
using Application.Features.Auth.Commands.UpdateProfile;
using Application.Features.Auth.Queries.GetMe;
using Application.Features.Auth.Queries.GetMyPermissions;
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

        // Public — the confirmation link in the welcome email lands here.
        // Anonymous because the user might not be signed in when they click it.
        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // Public so the unconfirmed-user "didn't get the link" flow works
        // even before they have a session. The Settings → Resend button uses
        // this same endpoint with the user's own email.
        [AllowAnonymous]
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var result = await _mediator.Send(new GetMeQuery());
            return Ok(result);
        }

        /// <summary>
        /// Lightweight endpoint the SPA calls right after sign-in / on app
        /// boot so it can hide nav items and action buttons the current
        /// user's role doesn't grant. Returns permission names as strings
        /// (e.g. "GET_FAMILY") so the wire format survives enum-int reordering.
        /// </summary>
        [Authorize]
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMyPermissionsQuery(), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            if (command == null)
                return BadRequest("Request cannot be null");
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
