using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    /// <summary>
    /// Admin-only operational endpoints. Each action runs the
    /// <see cref="IAdminAuthorizationService"/> gate first and short-circuits
    /// with 403 for non-admins; <c>[Authorize]</c> is also present so an
    /// unauthenticated caller never even reaches the role check.
    ///
    /// <para>The "send now" endpoints exist so an admin can trigger the
    /// daily statistics or DB backup ad-hoc — useful for verifying the
    /// pipeline after a deploy or when an investigation needs a fresh
    /// snapshot rather than yesterday's 09:00 UTC run.</para>
    /// </summary>
    [Authorize]
    public class AdminController(
        IMediator mediator,
        IAdminAuthorizationService adminGate,
        IStatisticsService statistics,
        IDatabaseBackupService backup,
        ILogger<AdminController> logger) : BaseController(mediator)
    {
        private readonly IAdminAuthorizationService _adminGate = adminGate;
        private readonly IStatisticsService _statistics = statistics;
        private readonly IDatabaseBackupService _backup = backup;
        private readonly ILogger<AdminController> _logger = logger;

        /// <summary>
        /// Lets the SPA decide whether to render admin-only UI (settings
        /// tab, send-now buttons) without sending requests that the gate
        /// would just 403. Returns 200 in either case so a non-admin
        /// doesn't see a noisy network error in the console — the
        /// boolean is the actual signal.
        /// </summary>
        [HttpGet("check")]
        public async Task<IActionResult> CheckAdminStatus(CancellationToken cancellationToken)
        {
            var isAdmin = await _adminGate.IsCurrentUserAdminAsync(cancellationToken);
            return Ok(new { isAdmin });
        }

        /// <summary>
        /// Generates the same statistics summary the daily background
        /// service sends and posts it to <c>familytree.dev.stats</c>
        /// immediately. Returns 202 so the dashboard can show
        /// "queued" without blocking on the gateway round-trip.
        /// </summary>
        [HttpPost("notifications/send-stats")]
        public async Task<IActionResult> SendStatisticsNow(CancellationToken cancellationToken)
        {
            if (!await _adminGate.IsCurrentUserAdminAsync(cancellationToken))
                return Forbid();

            try
            {
                await _statistics.GenerateAndSendAsync(cancellationToken);
                return Accepted(new { sent = true, destination = "familytree.dev.stats" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin-triggered statistics send failed.");
                return Problem(
                    detail: ex.Message,
                    title: "Failed to send statistics.",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Runs <c>pg_dump</c> against the FamilyTree database and uploads
        /// the gzip-compressed dump to <c>familytree.dev.dbarxiv</c>.
        /// Synchronous — admin sees success/failure inline rather than
        /// having to refresh and check the topic. Can be slow (tens of
        /// seconds for a multi-MB dump).
        /// </summary>
        [HttpPost("notifications/send-db-backup")]
        public async Task<IActionResult> SendDatabaseBackupNow(CancellationToken cancellationToken)
        {
            if (!await _adminGate.IsCurrentUserAdminAsync(cancellationToken))
                return Forbid();

            try
            {
                await _backup.RunAndSendAsync(cancellationToken);
                return Ok(new { sent = true, destination = "familytree.dev.dbarxiv" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin-triggered DB backup failed.");
                return Problem(
                    detail: ex.Message,
                    title: "Failed to send database backup.",
                    statusCode: 500);
            }
        }
    }
}
