using Application.Common.Interfaces;
using System.Net;
using System.Text.Json;

namespace WebApi.Middleware
{
    /// <summary>
    /// Catches any unhandled exception, logs it, fires a Telegram notification
    /// to the <c>familytree.dev.bugs</c> topic, and returns a JSON error
    /// envelope so the SPA gets a consistent shape.
    ///
    /// <para><b>Why a custom middleware instead of <c>UseExceptionHandler</c>?</b>
    /// We need the request scope (so we can resolve <see cref="INotificationService"/>
    /// from the current scope) and access to the original
    /// <see cref="HttpContext"/> for path/trace metadata. The built-in
    /// re-execute-the-pipeline handler complicates both.</para>
    ///
    /// <para><b>Notification call is awaited but tolerant.</b> The notification
    /// service swallows transport errors itself, so awaiting here is safe —
    /// it just means the JSON response goes back after the gateway either
    /// accepts the alert or fails gracefully.</para>
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, INotificationService notifications)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path} (trace {TraceId}).",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);

                await notifications.SendAsync(
                    "familytree.dev.bugs",
                    FormatBugMessage(context, ex),
                    parseMode: null,
                    cancellationToken: context.RequestAborted);

                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    var payload = new
                    {
                        success = false,
                        message = "Internal server error.",
                        traceId = context.TraceIdentifier
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                }
            }
        }

        private static string FormatBugMessage(HttpContext context, Exception ex)
        {
            // Trim long stack traces so a single deep exception doesn't blow
            // past Telegram's 4096-char message limit. The full stack still
            // lives in the application log via _logger.LogError above.
            const int maxStackLength = 1500;
            var stack = ex.StackTrace ?? string.Empty;
            if (stack.Length > maxStackLength)
                stack = stack.Substring(0, maxStackLength) + "\n... [truncated]";

            return $"❌ {context.Request.Method} {context.Request.Path}\n" +
                   $"{ex.GetType().Name}: {ex.Message}\n" +
                   $"trace: {context.TraceIdentifier}\n" +
                   $"\n{stack}";
        }
    }
}
