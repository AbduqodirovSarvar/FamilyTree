using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Sends a one-line message to a named destination on the development
    /// notification gateway (Telegram bot fronted by an HTTP service).
    /// Implementations are expected to be <b>fire-and-forget safe</b> —
    /// any transport or upstream failure must be swallowed and logged so
    /// a Telegram outage doesn't take down the calling business flow
    /// (sign-up, family-create, etc.).
    /// </summary>
    public interface INotificationService
    {
        /// <param name="destination">
        /// Logical name configured server-side, e.g. <c>familytree.dev.users</c>.
        /// The gateway resolves it to a chat/topic — clients never see raw IDs.
        /// </param>
        /// <param name="parseMode">
        /// Optional Telegram formatting mode: <c>HTML</c>, <c>Markdown</c>,
        /// or <c>MarkdownV2</c>. Null = plain text. HTML is the safest
        /// choice for rich formatting because only three characters
        /// (<c>&amp;</c>, <c>&lt;</c>, <c>&gt;</c>) need escaping in text.
        /// </param>
        Task SendAsync(
            string destination,
            string text,
            string? parseMode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams a file to the gateway's <c>/api/notify/document</c>
        /// endpoint, which forwards it to Telegram as a document
        /// (<c>sendDocument</c>). Used for things like daily DB backups.
        /// Throws on transport or upstream errors so admin-triggered
        /// "send now" flows can surface the failure to the caller —
        /// fire-and-forget swallow semantics live on
        /// <see cref="SendAsync(string, string, string?, CancellationToken)"/>.
        /// </summary>
        Task SendDocumentAsync(
            string destination,
            Stream content,
            string fileName,
            string? caption = null,
            CancellationToken cancellationToken = default);
    }
}
