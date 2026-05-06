using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Computes and dispatches the daily statistics summary. Extracted from
    /// the background service so the same logic is reachable from
    /// admin-triggered "send now" endpoints (dashboard) without
    /// duplicating the count queries or the message format.
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Aggregates current counts and posts the formatted message to
        /// <c>familytree.dev.stats</c>. Throws on transport or repository
        /// errors so callers can surface a 500 with the reason — unlike
        /// the fire-and-forget event notifications, an admin-triggered
        /// send is interactive and benefits from real failure feedback.
        /// </summary>
        Task GenerateAndSendAsync(CancellationToken cancellationToken = default);
    }
}
