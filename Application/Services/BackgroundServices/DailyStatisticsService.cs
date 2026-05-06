using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.BackgroundServices
{
    /// <summary>
    /// Wakes once per day and asks <see cref="IStatisticsService"/> to
    /// post the summary. The actual aggregation lives in the service so
    /// the admin "send now" endpoint can call the exact same code path
    /// without duplicating the format or the count queries.
    ///
    /// <para><b>Schedule.</b> Picks the next 09:00 UTC after each post and
    /// sleeps until then. UTC chosen on purpose — the dev group spans
    /// timezones, so an absolute time is more predictable than "every
    /// 24h after boot" (which would drift with each restart).</para>
    /// </summary>
    public sealed class DailyStatisticsService : BackgroundService
    {
        // 09:00 UTC = 14:00 in Tashkent (UTC+5). Tweak via constant rather
        // than config — moving the time should be a deliberate code change
        // reviewed in PR, not a runtime knob someone can flip silently.
        private static readonly TimeSpan SendAt = new(09, 00, 00);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyStatisticsService> _logger;

        public DailyStatisticsService(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyStatisticsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily statistics service started; first send at {SendAt} UTC.", SendAt);

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
                    // Open a fresh DI scope per tick — repositories are
                    // scoped, and holding a scope across the 24h delay
                    // would pin the DbContext for the whole day.
                    using var scope = _scopeFactory.CreateScope();
                    var stats = scope.ServiceProvider.GetRequiredService<IStatisticsService>();
                    await stats.GenerateAndSendAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Catch-and-log so a transient repository or gateway
                    // hiccup doesn't kill the whole service. Without this,
                    // one failure would silence stats forever.
                    _logger.LogError(ex, "Daily statistics dispatch failed; will retry tomorrow.");
                }
            }
        }

        /// <summary>
        /// Returns the time until the next 09:00 UTC after <paramref name="now"/>.
        /// If we're already past today's window, schedules tomorrow.
        /// </summary>
        internal static TimeSpan TimeUntilNextSend(DateTime now)
        {
            var todayTarget = new DateTime(now.Year, now.Month, now.Day,
                SendAt.Hours, SendAt.Minutes, SendAt.Seconds, DateTimeKind.Utc);

            // If the current time is already past today's slot, jump to
            // tomorrow's slot — otherwise the loop would fire immediately
            // on every restart that happens after 09:00 UTC.
            if (now >= todayTarget)
                todayTarget = todayTarget.AddDays(1);

            return todayTarget - now;
        }
    }
}
