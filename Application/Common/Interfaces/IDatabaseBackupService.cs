using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Runs <c>pg_dump</c> against the configured PostgreSQL database and
    /// streams the (gzip-compressed) result to the notification gateway's
    /// document endpoint, which forwards it to the Telegram dbarxiv topic.
    ///
    /// <para>Failures throw — the daily background service catches them so
    /// it can keep running, but the admin "send now" endpoint surfaces
    /// them as 500 with the underlying message.</para>
    /// </summary>
    public interface IDatabaseBackupService
    {
        Task RunAndSendAsync(CancellationToken cancellationToken = default);
    }
}
