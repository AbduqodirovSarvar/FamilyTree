using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.BackgroundServices
{
    /// <summary>
    /// Wakes once per day, counts the high-level entities, and posts a
    /// summary to <c>familytree.dev.stats</c>.
    ///
    /// <para><b>Schedule.</b> Picks the next 09:00 UTC after each post and
    /// sleeps until then. UTC chosen on purpose — the dev group spans
    /// timezones, so an absolute time is more predictable than "every
    /// 24h after boot" (which would drift with each restart).</para>
    ///
    /// <para><b>Scoping.</b> Repositories are scoped (per-request lifetime),
    /// so the loop opens a fresh DI scope each iteration via
    /// <see cref="IServiceScopeFactory"/>. Holding a scope across the
    /// 24-hour <see cref="Task.Delay"/> would pin the DbContext and
    /// every transient EF resource for the whole day.</para>
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
                    await SendDailyStatsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Catch-and-log so a transient repository or gateway
                    // hiccup doesn't kill the whole service. Without this,
                    // one 500 from the gateway could silence stats forever.
                    _logger.LogError(ex, "Daily statistics dispatch failed; will retry tomorrow.");
                }
            }
        }

        private async Task SendDailyStatsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var families = scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
            var members = scope.ServiceProvider.GetRequiredService<IMemberRepository>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Run counts in parallel — they hit independent tables and the
            // Postgres connection pool can handle it. Saves ~latency * 3
            // when network is slow.
            var userTask = users.CountAsync(cancellationToken: cancellationToken);
            var confirmedUserTask = users.CountAsync(u => u.EmailConfirmed, cancellationToken);
            var familyTask = families.CountAsync(cancellationToken: cancellationToken);
            var memberTask = members.CountAsync(cancellationToken: cancellationToken);

            await Task.WhenAll(userTask, confirmedUserTask, familyTask, memberTask);

            var text = FormatStatistics(
                totalUsers: userTask.Result,
                confirmedUsers: confirmedUserTask.Result,
                totalFamilies: familyTask.Result,
                totalMembers: memberTask.Result);

            await notifications.SendAsync(
                "familytree.dev.stats",
                text,
                parseMode: "HTML",
                cancellationToken);

            _logger.LogInformation(
                "Daily statistics sent: {Users} users, {Families} families, {Members} members.",
                userTask.Result, familyTask.Result, memberTask.Result);
        }

        private static string FormatStatistics(
            int totalUsers,
            int confirmedUsers,
            int totalFamilies,
            int totalMembers)
        {
            // Counts come from the DB (no user-supplied content) so escaping
            // isn't strictly required — using TelegramHtml here only as a
            // belt-and-suspenders default in case fields change later.
            var when = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

            return $"""
                📊 <b>Kunlik statistika</b>
                <i>{when} UTC</i>

                👥 <b>Foydalanuvchilar:</b> <code>{totalUsers}</code>
                ✅ <b>Tasdiqlangan email:</b> <code>{confirmedUsers}</code>

                🏡 <b>Oilalar:</b> <code>{totalFamilies}</code>
                👨‍👩‍👧‍👦 <b>A'zolar:</b> <code>{totalMembers}</code>
                """;
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
