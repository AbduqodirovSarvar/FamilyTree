using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.SignUp
{
    public class SignUpCommandHandler(
        IAuthService authService,
        INotificationService notifications
        ) : IRequestHandler<SignUpCommand, Response<bool>>
    {
        private readonly IAuthService _authService = authService;
        private readonly INotificationService _notifications = notifications;

        public async Task<Response<bool>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.SignUpAsync(request, cancellationToken);

            // Fire-and-forget — TelegramNotificationService swallows failures,
            // so a Telegram outage never turns a successful sign-up into a 500.
            // Awaited (rather than `_ = ...`) so the gateway call inherits the
            // same cancellation token and any logging happens inside the
            // request scope where the logger has the right correlation ID.
            await _notifications.SendAsync(
                "familytree.dev.users",
                FormatNewUserMessage(request),
                cancellationToken);

            return Response<bool>.Ok(result, "Sign up successful.");
        }

        private static string FormatNewUserMessage(SignUpCommand request)
        {
            // Plain text, not Markdown/HTML — keeps the gateway from having
            // to escape special characters and avoids accidentally breaking
            // formatting if a user's name contains '*' or '_'.
            var name = string.Join(' ',
                new[] { request.FirstName, request.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            return $"Yangi foydalanuvchi: {name} ({request.Email})";
        }
    }
}
