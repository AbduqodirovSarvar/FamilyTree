using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.BackgroundServices
{
    /// <summary>
    /// Wakes once per day and asks <see cref="IDatabaseBackupService"/> to
    /// produce a fresh dump and upload it to the dbarxiv Telegram topic.
    /// Same pattern as <see cref="DailyStatisticsService"/> — UTC schedule
    /// so the time is predictable across timezones, fresh DI scope per
    /// run so the DbContext isn't pinned for the full day, and a try
    /// /catch so a transient failure doesn't kill the loop.
    ///
    /// <para><b>Scheduled at 03:00 UTC = 08:00 Tashkent</b> — runs in the
    /// quiet hours so the database doesn't compete with daytime traffic.
    /// Stats run separately at 09:00 UTC so the two heavy workers
    /// don't overlap.</para>
    /// </summary>
    public sealed class DailyDatabaseBackupService : BackgroundService
    {
        private static readonly TimeSpan SendAt = new(03, 00, 00);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyDatabaseBackupService> _logger;

        public DailyDatabaseBackupService(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyDatabaseBackupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily DB backup service started; first run at {SendAt} UTC.", SendAt);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeUntilNextSend(DateTime.UtcNow);
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var backup = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                    await backup.RunAndSendAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Daily DB backup failed; will retry tomorrow.");
                }
            }
        }

        internal static TimeSpan TimeUntilNextSend(DateTime now)
        {
            var todayTarget = new DateTime(now.Year, now.Month, now.Day,
                SendAt.Hours, SendAt.Minutes, SendAt.Seconds, DateTimeKind.Utc);

            if (now >= todayTarget)
                todayTarget = todayTarget.AddDays(1);

            return todayTarget - now;
        }
    }
}
