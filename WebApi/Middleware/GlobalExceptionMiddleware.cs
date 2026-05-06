using Application.Common.Interfaces;
using System.Net;
using System.Text.Json;

namespace WebApi.Middleware
{
    /// <summary>
    /// Two-layer 5xx capture:
    ///
    /// <list type="number">
    ///   <item><description><b>Unhandled exceptions</b> — caught here, logged,
    ///   converted to a JSON 500 response, and dispatched to Telegram's
    ///   <c>familytree.dev.bugs</c> topic with type, message, and a trimmed
    ///   stack trace.</description></item>
    ///   <item><description><b>Manual 5xx responses</b> — controllers that
    ///   catch their own exceptions and return <c>Problem(500)</c> never
    ///   throw past us, so the exception <c>catch</c> block above can't see
    ///   them. The post-<c>_next</c> status check handles that case: any
    ///   response with status >= 500 also triggers a Telegram notification
    ///   (without a stack trace, since we don't have the original
    ///   exception).</description></item>
    /// </list>
    ///
    /// <para>Together these cover every 500+ the backend produces, whether
    /// from a thrown exception or a deliberate <c>return Problem(...)</c>.</para>
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private const string BugsDestination = "familytree.dev.bugs";
        private const int MaxStackLength = 1500;

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

                // Layer 2: controller-returned 5xx. By the time _next has
                // returned without throwing, any error response was set
                // explicitly (e.g. `return Problem(statusCode: 500)`).
                // Notify even though we have no exception to render.
                if (context.Response.StatusCode >= 500)
                {
                    _logger.LogWarning(
                        "Server response {StatusCode} on {Method} {Path} (trace {TraceId}).",
                        context.Response.StatusCode,
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);

                    await notifications.SendAsync(
                        BugsDestination,
                        FormatStatusMessage(context),
                        parseMode: null,
                        cancellationToken: context.RequestAborted);
                }
            }
            catch (Exception ex)
            {
                // Layer 1: unhandled exception that bubbled past the
                // controllers entirely.
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path} (trace {TraceId}).",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);

                await notifications.SendAsync(
                    BugsDestination,
                    FormatExceptionMessage(context, ex),
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

        private static string FormatExceptionMessage(HttpContext context, Exception ex)
        {
            // Trim long stack traces so a single deep exception doesn't blow
            // past Telegram's 4096-char message limit. The full stack still
            // lives in the application log via _logger.LogError above.
            var stack = ex.StackTrace ?? string.Empty;
            if (stack.Length > MaxStackLength)
                stack = stack.Substring(0, MaxStackLength) + "\n... [truncated]";

            return $"❌ {context.Request.Method} {context.Request.Path}\n" +
                   $"{ex.GetType().Name}: {ex.Message}\n" +
                   $"trace: {context.TraceIdentifier}\n" +
                   $"\n{stack}";
        }

        private static string FormatStatusMessage(HttpContext context)
        {
            // No exception object available — the controller already turned
            // it into a Problem result. Surface the basics so operators can
            // pick up the trace ID and dig through logs for the full story.
            return $"⚠️ HTTP {context.Response.StatusCode} — {context.Request.Method} {context.Request.Path}\n" +
                   $"trace: {context.TraceIdentifier}\n" +
                   $"(Controller returned a 5xx without throwing — see application logs by trace ID.)";
        }
    }
}
