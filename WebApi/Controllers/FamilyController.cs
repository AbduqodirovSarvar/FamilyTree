using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Common.Models.ViewModels.Tree;
using Application.Features.Family.Commands.Create;
using Application.Features.Family.Commands.Delete;
using Application.Features.Family.Commands.Update;
using Application.Features.Family.Queries.CheckExist;
using Application.Features.Family.Queries.GetFamilyTree;
using Application.Features.Family.Queries.GetList;
using Application.Features.Family.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController(
        IMediator mediator,
        IFamilyRepository familyRepository,
        IFamilyViewRepository familyViewRepository,
        IFamilyViewRecorder familyViewRecorder)
        : BaseServiceController<
            CreateFamilyCommand,
            UpdateFamilyCommand,
            DeleteFamilyCommand,
            GetFamilyQuery,
            GetFamilyListQuery,
            CheckFamilyExistQuery>(mediator)
    {
        private readonly IFamilyRepository _familyRepository = familyRepository;
        private readonly IFamilyViewRepository _familyViewRepository = familyViewRepository;
        private readonly IFamilyViewRecorder _familyViewRecorder = familyViewRecorder;

        [HttpGet("tree/{familyId:guid}")]
        public async Task<IActionResult> GetTree(Guid familyId)
        {
            var result = await _mediator.Send(new GetFamilyTreeQuery { FamilyId = familyId });
            return Ok(result);
        }

        /// <summary>
        /// Public family-tree lookup by surname — no auth required so the
        /// FamilyTreeUi viewer can render a tree from a /:familyName URL
        /// without forcing visitors to log in. Skips MediatR/GetFamilyQuery on
        /// purpose: that path goes through IFamilyService → PermissionService,
        /// which throws UnauthorizedAccessException for anonymous callers.
        /// We resolve the family via the repository directly (no permission
        /// check), then dispatch GetFamilyTreeQuery whose handler also uses
        /// repositories only.
        /// </summary>
        [HttpGet("public/tree/{familyName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicTreeByName(string familyName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(familyName))
                return BadRequest(Response<FamilyTreeViewModel>.Fail("familyName is required"));

            var family = await _familyRepository.GetAsync(f => f.FamilyName == familyName, cancellationToken);
            if (family == null)
                return NotFound(Response<FamilyTreeViewModel>.Fail("Family not found"));

            var treeResp = await _mediator.Send(new GetFamilyTreeQuery { FamilyId = family.Id }, cancellationToken);

            // Hot-path analytics — record into the in-memory buffer only.
            // FamilyViewFlushService persists batches every few minutes so
            // public-tree refreshes never hit the DB on this path.
            var ip = ResolveClientIp();
            if (!string.IsNullOrEmpty(ip))
            {
                _familyViewRecorder.Record(family.Id, ip);
            }

            return Ok(treeResp);
        }

        /// <summary>
        /// Daily visitor counts for one family. Owner / family member sees
        /// their own family; admin sees any. Anonymous / non-member callers
        /// get 403 — the family-tree itself is public, but its analytics are
        /// not.
        /// </summary>
        [HttpGet("{familyId:guid}/views/stats")]
        [Authorize]
        public async Task<IActionResult> GetViewStats(
            Guid familyId,
            [FromQuery] int days,
            [FromServices] ICurrentUserService currentUser,
            [FromServices] IAdminAuthorizationService adminAuth,
            CancellationToken cancellationToken)
        {
            // Clamp the range so a malicious or sloppy caller can't ask for
            // 100k days. 90 covers a calendar quarter, which is the longest
            // window the dashboard exposes.
            var window = days <= 0 ? 30 : Math.Min(days, 90);

            var family = await _familyRepository.GetByIdAsync(familyId, cancellationToken);
            if (family == null)
                return NotFound(Response<object>.Fail("Family not found"));

            var isAdmin = await adminAuth.IsCurrentUserAdminAsync(cancellationToken);
            if (!isAdmin)
            {
                var user = await currentUser.GetCurrentUserAsync(cancellationToken);
                if (user == null)
                    return Unauthorized(Response<object>.Fail("Not authenticated"));

                // Allowed: family owner, or a user attached to the family
                // (FamilyId === family.Id). Everyone else gets 403 even if
                // they can render the tree publicly.
                var isOwner = family.OwnerId == user.Id;
                var isMember = user.FamilyId == family.Id;
                if (!isOwner && !isMember)
                    return Forbid();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = today.AddDays(-(window - 1));

            var raw = await _familyViewRepository.GetDailyCountsAsync(
                familyId, from, today, cancellationToken);
            var byDate = raw.ToDictionary(r => r.Date, r => r.Count);

            // Fill missing days with 0 so the chart line stays continuous.
            var points = new List<FamilyViewStatsPoint>(window);
            for (int i = 0; i < window; i++)
            {
                var d = from.AddDays(i);
                points.Add(new FamilyViewStatsPoint(
                    d.ToString("yyyy-MM-dd"),
                    byDate.TryGetValue(d, out var c) ? c : 0));
            }

            var payload = new FamilyViewStatsResponse(
                family.Id,
                family.FamilyName,
                from.ToString("yyyy-MM-dd"),
                today.ToString("yyyy-MM-dd"),
                points.Sum(p => p.Count),
                points);

            return Ok(Response<FamilyViewStatsResponse>.Ok(payload));
        }

        /// <summary>
        /// Best-effort client IP. Honours <c>X-Forwarded-For</c> when set by
        /// our reverse proxy (Caddy), falling back to the raw connection IP.
        /// </summary>
        private string ResolveClientIp()
        {
            var http = HttpContext;
            if (http == null) return string.Empty;

            // Caddy puts the original client IP first in X-Forwarded-For;
            // any subsequent proxies append. We take the leftmost entry.
            var forwarded = http.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var first = forwarded.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(first)) return first;
            }

            return http.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
