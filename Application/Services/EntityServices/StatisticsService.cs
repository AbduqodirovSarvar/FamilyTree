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
            // Run independent counts in parallel — they hit different tables
            // and the Postgres pool can serve them simultaneously. Saves a
            // round-trip's worth of latency per extra metric.
            var userTask = _users.CountAsync(cancellationToken: cancellationToken);
            var confirmedUserTask = _users.CountAsync(u => u.EmailConfirmed, cancellationToken);
            var familyTask = _families.CountAsync(cancellationToken: cancellationToken);
            var memberTask = _members.CountAsync(cancellationToken: cancellationToken);

            await Task.WhenAll(userTask, confirmedUserTask, familyTask, memberTask);

            var text = FormatStatistics(
                totalUsers: userTask.Result,
                confirmedUsers: confirmedUserTask.Result,
                totalFamilies: familyTask.Result,
                totalMembers: memberTask.Result);

            await _notifications.SendAsync(
                "familytree.dev.stats",
                text,
                parseMode: "HTML",
                cancellationToken);

            _logger.LogInformation(
                "Statistics dispatched: {Users} users ({Confirmed} confirmed), {Families} families, {Members} members.",
                userTask.Result, confirmedUserTask.Result, familyTask.Result, memberTask.Result);
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
