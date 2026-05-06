using Application.Common.Helpers;
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
                parseMode: "HTML",
                cancellationToken);

            return Response<bool>.Ok(result, "Sign up successful.");
        }

        /// <summary>
        /// HTML-formatted notification body. Every user-supplied value is run
        /// through <see cref="TelegramHtml.Escape"/> so a name like <c>&lt;b&gt;Bobby&lt;/b&gt;</c>
        /// can't break the formatting (or worse, smuggle markup past Telegram's parser).
        /// </summary>
        private static string FormatNewUserMessage(SignUpCommand request)
        {
            var fullName = $"{TelegramHtml.Escape(request.FirstName)} {TelegramHtml.Escape(request.LastName)}".Trim();
            var when = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

            return $"""
                🆕 <b>Yangi foydalanuvchi</b>

                👤 <b>Ism:</b> {fullName}
                🆔 <b>Username:</b> @{TelegramHtml.Escape(request.UserName)}
                📧 <b>Email:</b> <code>{TelegramHtml.Escape(request.Email)}</code>
                📞 <b>Telefon:</b> {TelegramHtml.Escape(request.Phone)}
                🕒 <b>Sana:</b> {when} UTC
                """;
        }
    }
}
