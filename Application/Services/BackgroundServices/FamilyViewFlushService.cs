using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.BackgroundServices
{
    /// <summary>
    /// Periodically drains the in-memory <see cref="IFamilyViewRecorder"/>
    /// buffer to a single batched INSERT. Saves PostgreSQL from the
    /// per-request write/read pressure the inline recorder created on the
    /// public-tree endpoint.
    ///
    /// <para>The actual SQL lives in
    /// <see cref="IFamilyViewRepository.BulkInsertIfMissingAsync"/> — using
    /// <c>ON CONFLICT DO NOTHING</c> means duplicates introduced by a process
    /// restart (in-memory state lost while the row already lives in the DB)
    /// silently skip instead of failing the batch.</para>
    /// </summary>
    public sealed class FamilyViewFlushService : BackgroundService
    {
        private static readonly TimeSpan FlushInterval = TimeSpan.FromMinutes(5);

        private readonly IFamilyViewRecorder _recorder;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FamilyViewFlushService> _logger;

        public FamilyViewFlushService(
            IFamilyViewRecorder recorder,
            IServiceScopeFactory scopeFactory,
            ILogger<FamilyViewFlushService> logger)
        {
            _recorder = recorder;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Family-view flush service started; interval = {Interval}.",
                FlushInterval);

            // Wait one interval before the first flush so the boot path stays
            // light and short-lived dev runs don't trigger a flush.
            try { await Task.Delay(FlushInterval, stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FlushAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    // Don't tear down the service on a single bad flush —
                    // keep looping; whatever's still in `_pending` rolls into
                    // the next pass.
                    _logger.LogError(ex, "Family-view flush failed; will retry.");
                }

                try { await Task.Delay(FlushInterval, stoppingToken); }
                catch (OperationCanceledException) { return; }
            }
        }

        private async Task FlushAsync(CancellationToken cancellationToken)
        {
            var entries = _recorder.DrainPending();

            // Always evict yesterday's keys so a server up for a month doesn't
            // hold thirty days of triples in memory.
            _recorder.EvictOlderThan(DateOnly.FromDateTime(DateTime.UtcNow));

            if (entries.Count == 0) return;

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IFamilyViewRepository>();
            var written = await repo.BulkInsertIfMissingAsync(entries, cancellationToken);

            _logger.LogInformation(
                "Flushed {Count} pending family-views (batched).",
                written);
        }
    }
}
