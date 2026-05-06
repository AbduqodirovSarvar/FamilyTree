using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Services
{
    /// <summary>
    /// HTTP client for the development notification gateway. Posts a small
    /// JSON envelope to <c>POST /api/notify</c> with the configured API key.
    ///
    /// <para><b>Failure policy.</b> Notification is observability, not part
    /// of the business transaction — sign-up succeeded, the user is happy,
    /// and a Telegram outage shouldn't bubble up as a 500. Every failure
    /// path here is caught and logged; nothing throws. The gateway itself
    /// already enqueues + retries internally, so we don't add a second
    /// retry loop on top.</para>
    /// </summary>
    public sealed class TelegramNotificationService : INotificationService
    {
        private readonly HttpClient _http;
        private readonly NotificationGatewayConfiguration _options;
        private readonly ILogger<TelegramNotificationService> _logger;

        public TelegramNotificationService(
            HttpClient http,
            IOptions<NotificationGatewayConfiguration> options,
            ILogger<TelegramNotificationService> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            string destination,
            string text,
            string? parseMode = null,
            CancellationToken cancellationToken = default)
        {
            // Empty config = "gateway not wired up in this environment". Don't
            // log noisy warnings on every call — a single startup-time check
            // would be ideal, but for now return silently so dev/test boxes
            // without the gateway keep working.
            if (string.IsNullOrEmpty(_options.BaseUrl) || string.IsNullOrEmpty(_options.ApiKey))
                return;

            try
            {
                // Build the body without the parseMode field when null —
                // mirrors what the gateway does internally and keeps wire
                // payloads small.
                object payload = string.IsNullOrEmpty(parseMode)
                    ? new { destination, text }
                    : new { destination, text, parseMode };

                var response = await _http.PostAsJsonAsync(
                    "/api/notify",
                    payload,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Notification gateway rejected destination {Destination}: {StatusCode} {Body}",
                        destination, (int)response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                // Swallow the exception so the caller's flow isn't affected.
                // Includes network errors, timeout, and the gateway being
                // unreachable. The log keeps the failure visible to operators.
                _logger.LogError(ex,
                    "Failed to dispatch notification to {Destination}.", destination);
            }
        }
    }
}
