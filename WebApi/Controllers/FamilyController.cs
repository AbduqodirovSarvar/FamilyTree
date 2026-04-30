using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
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
    public class FamilyController(IMediator mediator, IFamilyRepository familyRepository)
        : BaseServiceController<
            CreateFamilyCommand,
            UpdateFamilyCommand,
            DeleteFamilyCommand,
            GetFamilyQuery,
            GetFamilyListQuery,
            CheckFamilyExistQuery>(mediator)
    {
        private readonly IFamilyRepository _familyRepository = familyRepository;

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
            return Ok(treeResp);
        }
    }
}
