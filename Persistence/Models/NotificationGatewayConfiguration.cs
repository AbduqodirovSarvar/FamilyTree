namespace Persistence.Models
{
    /// <summary>
    /// Bound from the <c>NotificationGateway</c> section of appsettings.json.
    /// <see cref="ApiKey"/> is the shared secret the gateway expects in the
    /// <c>Authorization: Bearer ...</c> header — must come from a secret
    /// source (env var / user secrets), never committed.
    /// </summary>
    public sealed class NotificationGatewayConfiguration
    {
        public const string SectionName = "NotificationGateway";

        /// <summary>
        /// Gateway base URL. In Docker, prefer the internal DNS form
        /// <c>http://development-tg-bot:8080</c> — same network, no TLS
        /// handshake. From outside the cluster use the public HTTPS host
        /// (<c>https://development-tg-bot.api.svlab.uz</c>).
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;
    }
}
