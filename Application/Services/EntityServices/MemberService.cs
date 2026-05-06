using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Member;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using Application.Features.UploadedFile.Commands.Create;
using Application.Features.UploadedFile.Commands.Delete;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    /// <summary>
    /// Member access rules — modeled to match Family:
    ///
    /// <list type="bullet">
    ///   <item><description><b>List (read)</b> — admin sees every member; others see members of
    ///   families they own <i>or</i> the family they're attached to (<c>User.FamilyId</c>).
    ///   Membership is admin/owner-granted, so attached users get the same read scope as the
    ///   owner.</description></item>
    ///   <item><description><b>Create / Update / Delete</b> — the target family must be owned by
    ///   the current user <i>or</i> the user must be attached to it. Admin can read any member
    ///   but can't write into a family they neither own nor belong to.</description></item>
    /// </list>
    /// </summary>
    internal class MemberService(
        IMemberRepository memberRepository,
        IFamilyRepository familyRepository,
        IUserRoleRepository userRoleRepository,
        IUserService userService,
        IPermissionService permissionService,
        IMediator mediator,
        IMapper mapper)
        : GenericEntityService<Member, CreateMemberDto, UpdateMemberDto, MemberViewModel>(memberRepository, permissionService, mapper), IMemberService
    {
        private const string AdminRoleDesignedName = "ADMIN";

        private readonly IFamilyRepository _familyRepository = familyRepository;
        private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
        private readonly IUserService _userService = userService;
        private readonly IMediator _mediator = mediator;

        public override async Task<MemberViewModel> CreateAsync(CreateMemberDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Member).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            // The DTO carries FamilyId — the user is asking us to attach the new
            // member to that family. They must own it OR be attached to it.
            await EnsureCanManageFamilyAsync(entityCreateDto.FamilyId, cancellationToken);

            var entity = _mapper.Map<Member>(entityCreateDto);

            if (entityCreateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityCreateDto.Image, Alt = entity.FirstName, Description = entityCreateDto.Description }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<MemberViewModel>(result);
        }

        /// <summary>
        /// Adds a visibility predicate to the list query when the caller isn't
        /// an admin: members of families they own OR members of the family
        /// they're attached to. Admins keep the unrestricted view used by the
        /// preview moderation flow.
        /// </summary>
        public override async Task<Response<List<MemberViewModel>>> GetAllAsync(
            Expression<Func<Member, bool>>? predicate = null,
            int pageIndex = 0,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentUser(cancellationToken);
            if (user != null && !await IsAdminAsync(user, cancellationToken))
            {
                var userId = user.Id;
                var attachedFamilyId = user.FamilyId ?? Guid.Empty;
                Expression<Func<Member, bool>> visibilityPredicate = m =>
                    m.Family != null
                    && (m.Family.OwnerId == userId
                        || (attachedFamilyId != Guid.Empty && m.FamilyId == attachedFamilyId));
                predicate = visibilityPredicate.AndAlso(predicate);
            }

            return await base.GetAllAsync(predicate, pageIndex, pageSize, cancellationToken);
        }

        public override async Task<MemberViewModel> UpdateAsync(UpdateMemberDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Member).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to update this entity.");

            var entity = await _repository.GetByIdAsync(entityUpdateDto.Id, cancellationToken)
                            ?? throw new KeyNotFoundException("Member not found");

            // The member's CURRENT family must be one the caller can manage
            // (owner or attached) — this catches "edit this member in another
            // user's tree" attempts. The DTO might also try to MOVE the member
            // into a different family; if so, that target family must be
            // manageable too.
            await EnsureCanManageFamilyAsync(entity.FamilyId, cancellationToken);
            if (entityUpdateDto.FamilyId.HasValue
                && entityUpdateDto.FamilyId.Value != Guid.Empty
                && entityUpdateDto.FamilyId.Value != entity.FamilyId)
            {
                await EnsureCanManageFamilyAsync(entityUpdateDto.FamilyId.Value, cancellationToken);
            }

            // PUT semantic for nullable fields — the DTO value IS the new value.
            // Frontend sends an empty string for cleared fields, which the model
            // binder maps to null for nullable types. Without this, clearing a
            // field on the form (e.g. wiping DeathDay) silently kept the old value
            // because the old PATCH "if not null" guard never overwrote it.
            entity.FirstName = string.IsNullOrEmpty(entityUpdateDto.FirstName) ? null : entityUpdateDto.FirstName;
            entity.LastName = string.IsNullOrEmpty(entityUpdateDto.LastName) ? null : entityUpdateDto.LastName;
            entity.Description = string.IsNullOrEmpty(entityUpdateDto.Description) ? null : entityUpdateDto.Description;
            entity.DeathDay = entityUpdateDto.DeathDay;
            entity.FatherId = entityUpdateDto.FatherId == Guid.Empty ? null : entityUpdateDto.FatherId;
            entity.MotherId = entityUpdateDto.MotherId == Guid.Empty ? null : entityUpdateDto.MotherId;
            entity.SpouseId = entityUpdateDto.SpouseId == Guid.Empty ? null : entityUpdateDto.SpouseId;

            // Required fields keep PATCH semantic — the DB column is non-nullable
            // so we only overwrite when the caller explicitly provided a value.
            if (entityUpdateDto.BirthDay.HasValue) entity.BirthDay = entityUpdateDto.BirthDay.Value;
            if (entityUpdateDto.Gender.HasValue) entity.Gender = entityUpdateDto.Gender.Value;
            if (entityUpdateDto.FamilyId.HasValue && entityUpdateDto.FamilyId.Value != Guid.Empty)
                entity.FamilyId = entityUpdateDto.FamilyId.Value;

            if (entityUpdateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand
                {
                    File = entityUpdateDto.Image,
                    Alt = entity.FirstName,
                    Description = entityUpdateDto.Description
                }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.UpdateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<MemberViewModel>(result);
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Member).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.DELETE))
                throw new UnauthorizedAccessException("You do not have permission to delete this entity.");

            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");

            await EnsureCanManageFamilyAsync(entity.FamilyId, cancellationToken);

            // Detach self-references from any other member that points at this one
            // (FatherId / MotherId / SpouseId) so PostgreSQL doesn't reject the delete
            // with FK_Members_Members_* constraint violations.
            var referencing = await _repository.GetAllAsync(
                m => m.FatherId == id || m.MotherId == id || m.SpouseId == id,
                cancellationToken);

            foreach (var m in referencing)
            {
                if (m.FatherId == id) m.FatherId = null;
                if (m.MotherId == id) m.MotherId = null;
                if (m.SpouseId == id) m.SpouseId = null;
                await _repository.UpdateAsync(m, cancellationToken);
            }

            if (entity.ImageId.HasValue && entity.ImageId.Value != Guid.Empty)
            {
                await _mediator.Send(new DeleteUploadedFileCommand
                {
                    Id = entity.ImageId.Value
                }, cancellationToken);
            }

            return await _repository.DeleteAsync(entity, cancellationToken);
        }

        // ─── Private helpers ───────────────────────────────────────

        /// <summary>
        /// Throws if the current user can't manage members of the given family —
        /// allowed if they own it OR are attached to it via <c>User.FamilyId</c>.
        /// Membership is admin/owner-granted upstream, so attached users are
        /// trusted to write within their own family tree.
        /// </summary>
        private async Task EnsureCanManageFamilyAsync(Guid familyId, CancellationToken cancellationToken)
        {
            var user = await _userService.GetCurrentUser(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");

            var family = await _familyRepository.GetByIdAsync(familyId, cancellationToken)
                         ?? throw new KeyNotFoundException("Family not found.");

            var isOwner = family.OwnerId == user.Id;
            var isAttached = user.FamilyId.HasValue && user.FamilyId.Value == family.Id;
            if (!isOwner && !isAttached)
                throw new UnauthorizedAccessException("Siz bu oilaga biriktirilmagansiz va uning egasi emassiz.");
        }

        private async Task<bool> IsAdminAsync(User user, CancellationToken cancellationToken)
        {
            var role = await _userRoleRepository.GetByIdAsync(user.RoleId, cancellationToken);
            return string.Equals(role?.DesignedName, AdminRoleDesignedName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
