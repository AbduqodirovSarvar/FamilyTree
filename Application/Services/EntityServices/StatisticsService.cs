using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services.EntityServices
{
    /// <summary>
    /// Single source of truth for the daily-stats aggregate. Both the
    /// scheduled <see cref="BackgroundServices.DailyStatisticsService"/>
    /// and the admin "send now" endpoint go through this service, so
    /// the message format and the count queries stay in lockstep.
    ///
    /// <para><b>Fresh scope per call.</b> The four count queries plus the
    /// outbound notification all run inside a dedicated DI scope created
    /// here, instead of sharing the caller's request scope. This isolates
    /// the stats DbContext from anything else the request thread is
    /// doing — admin role lookups, audit interceptors, you name it — and
    /// eliminates "A second operation was started on this context
    /// instance before a previous operation completed" by construction.
    /// The cost is one extra DbContext + service tree per call;
    /// trivial for a once-a-day or admin-triggered action.</para>
    /// </summary>
    internal sealed class StatisticsService : IStatisticsService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(
            IServiceScopeFactory scopeFactory,
            ILogger<StatisticsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task GenerateAndSendAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;

            var users = sp.GetRequiredService<IUserRepository>();
            var families = sp.GetRequiredService<IFamilyRepository>();
            var members = sp.GetRequiredService<IMemberRepository>();
            var notifications = sp.GetRequiredService<INotificationService>();

            // Sequential awaits within the dedicated scope. Even though the
            // scope already isolates the DbContext, running the counts in
            // sequence (rather than Task.WhenAll) keeps the call pattern
            // explicit and avoids tripping any future concurrent-DbContext
            // tripwire if a repository decision later starts holding state.
            var totalUsers = await users.CountAsync(cancellationToken: cancellationToken);
            var confirmedUsers = await users.CountAsync(u => u.EmailConfirmed, cancellationToken);
            var totalFamilies = await families.CountAsync(cancellationToken: cancellationToken);
            var totalMembers = await members.CountAsync(cancellationToken: cancellationToken);

            var text = FormatStatistics(totalUsers, confirmedUsers, totalFamilies, totalMembers);

            await notifications.SendAsync(
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
