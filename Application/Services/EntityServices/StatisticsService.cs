using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services.EntityServices
{
    /// <summary>
    /// Single source of truth for the daily-stats aggregate. Both the
    /// scheduled <see cref="BackgroundServices.DailyStatisticsService"/>
    /// and the admin "send now" endpoint go through this service, so
    /// the message format and the count queries stay in lockstep.
    /// </summary>
    internal sealed class StatisticsService : IStatisticsService
    {
        private readonly IUserRepository _users;
        private readonly IFamilyRepository _families;
        private readonly IMemberRepository _members;
        private readonly INotificationService _notifications;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IUserRepository users,
            IFamilyRepository families,
            IMemberRepository members,
            INotificationService notifications,
            ILogger<StatisticsService> logger)
        {
            _users = users;
            _families = families;
            _members = members;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task GenerateAndSendAsync(CancellationToken cancellationToken = default)
        {
            // Sequential awaits, NOT Task.WhenAll. All four repositories share
            // the same scoped DbContext, and EF Core's DbContext is not
            // thread-safe — running counts in parallel triggered "A second
            // operation was started on this context instance before a previous
            // operation completed." The 3-extra-round-trips of latency cost is
            // negligible for a once-a-day stats dump.
            var totalUsers = await _users.CountAsync(cancellationToken: cancellationToken);
            var confirmedUsers = await _users.CountAsync(u => u.EmailConfirmed, cancellationToken);
            var totalFamilies = await _families.CountAsync(cancellationToken: cancellationToken);
            var totalMembers = await _members.CountAsync(cancellationToken: cancellationToken);

            var text = FormatStatistics(totalUsers, confirmedUsers, totalFamilies, totalMembers);

            await _notifications.SendAsync(
                "familytree.dev.stats",
                text,
                parseMode: "HTML",
                cancellationToken);

            _logger.LogInformation(
                "Statistics dispatched: {Users} users ({Confirmed} confirmed), {Families} families, {Members} members.",
                totalUsers, confirmedUsers, totalFamilies, totalMembers);
        }

        private static string FormatStatistics(
            int totalUsers,
            int confirmedUsers,
            int totalFamilies,
            int totalMembers)
        {
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
    }
}
