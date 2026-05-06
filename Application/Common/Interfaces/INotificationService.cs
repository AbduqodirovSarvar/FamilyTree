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
        Task SendAsync(string destination, string text, CancellationToken cancellationToken = default);
    }
}
