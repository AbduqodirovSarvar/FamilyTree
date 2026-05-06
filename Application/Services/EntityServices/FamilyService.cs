using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Family;
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
    /// Family-scoped read/write rules:
    ///
    /// <list type="bullet">
    ///   <item><description><b>List (read)</b> — admin sees every family; non-admins see families
    ///   they created (<c>OwnerId == currentUser.Id</c>) <i>or</i> the one they're attached to
    ///   (<c>Family.Id == currentUser.FamilyId</c>). Membership is admin/owner-granted, so it's
    ///   trusted as a visibility signal.</description></item>
    ///   <item><description><b>Update / Delete</b> — owner-only, <i>regardless of role</i>.
    ///   Even an admin cannot edit or delete a family someone else created. Attached members
    ///   can manage tree members but never the family record itself.</description></item>
    /// </list>
    ///
    /// Admin status is decided by the role's <c>DesignedName == "ADMIN"</c> rather than by
    /// permission shape, so adding/removing permissions doesn't accidentally grant cross-family read.
    /// </summary>
    internal class FamilyService(
        IFamilyRepository familyRepository,
        IUserRoleRepository userRoleRepository,
        IPermissionService permissionService,
        IUserService userService,
        IMediator mediator,
        IMapper mapper,
        INotificationService notifications)
        : GenericEntityService<Family, CreateFamilyDto, UpdateFamilyDto, FamilyViewModel>(familyRepository, permissionService, mapper), IFamilyService
    {
        private const string AdminRoleDesignedName = "ADMIN";

        private readonly IUserService _userService = userService;
        private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
        private readonly IMediator _mediator = mediator;
        private readonly INotificationService _notifications = notifications;

        public override async Task<FamilyViewModel> CreateAsync(CreateFamilyDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<Family>(entityCreateDto);

            var user = await _userService.GetCurrentUser(cancellationToken)
                        ?? throw new KeyNotFoundException("Current user not found.");

            entity.OwnerId = user.Id;
            if (entityCreateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityCreateDto.Image, Alt = entity.Name, Description = entityCreateDto.Description }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            // Fire-and-forget Telegram notification. Failures are swallowed
            // by TelegramNotificationService so a gateway outage doesn't
            // turn a successful family-create into a user-facing error.
            await _notifications.SendAsync(
                "familytree.dev.families",
                $"Yangi oila: {result.Name} {result.FamilyName} (egasi: {user.FirstName} {user.LastName})",
                cancellationToken);

            return _mapper.Map<FamilyViewModel>(result);
        }

        /// <summary>
        /// Augments the base list query with a visibility predicate when the
        /// current user isn't an admin: families they own OR the family they're
        /// attached to. Admin still sees everything (e.g. for system-wide
        /// moderation in the preview module).
        /// </summary>
        public override async Task<Response<List<FamilyViewModel>>> GetAllAsync(
            Expression<Func<Family, bool>>? predicate = null,
            int pageIndex = 0,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetCurrentUser(cancellationToken);
            if (user != null && !await IsAdminAsync(user, cancellationToken))
            {
                var userId = user.Id;
                // Empty Guid is the "no attached family" sentinel — picked over
                // a nullable so the EF translator emits a single SQL OR instead
                // of branching on HasValue at expression-build time.
                var attachedFamilyId = user.FamilyId ?? Guid.Empty;
                Expression<Func<Family, bool>> visibilityPredicate = f =>
                    f.OwnerId == userId
                    || (attachedFamilyId != Guid.Empty && f.Id == attachedFamilyId);
                predicate = visibilityPredicate.AndAlso(predicate);
            }

            return await base.GetAllAsync(predicate, pageIndex, pageSize, cancellationToken);
        }

        public override async Task<FamilyViewModel> UpdateAsync(UpdateFamilyDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to update this entity.");

            var entity = await _repository.GetByIdAsync(entityUpdateDto.Id, cancellationToken)
                                ?? throw new KeyNotFoundException("Family not found");

            await EnsureOwnerAsync(entity, cancellationToken);

            // PUT semantic for nullable Description — empty string clears it.
            // Required fields (Name, FamilyName, OwnerId) keep PATCH guard so a
            // blank submission can't accidentally null out a non-nullable column.
            if (!string.IsNullOrWhiteSpace(entityUpdateDto.Name)) entity.Name = entityUpdateDto.Name;
            if (!string.IsNullOrWhiteSpace(entityUpdateDto.FamilyName)) entity.FamilyName = entityUpdateDto.FamilyName;
            entity.Description = string.IsNullOrEmpty(entityUpdateDto.Description) ? null : entityUpdateDto.Description;
            if (entityUpdateDto.OwnerId.HasValue && entityUpdateDto.OwnerId.Value != Guid.Empty)
                entity.OwnerId = entityUpdateDto.OwnerId.Value;

            if (entityUpdateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand
                {
                    File = entityUpdateDto.Image,
                    Alt = entity.Name,
                    Description = entityUpdateDto.Description
                }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.UpdateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<FamilyViewModel>(result);
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.DELETE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");

            await EnsureOwnerAsync(entity, cancellationToken);

            if (entity.ImageId.HasValue && entity.ImageId.Value != Guid.Empty)
            {
                await _mediator.Send(new DeleteUploadedFileCommand()
                {
                    Id = entity.ImageId.Value
                }, cancellationToken);
            }

            return await _repository.DeleteAsync(entity, cancellationToken);
        }

        // ─── Private helpers ───────────────────────────────────────

        /// <summary>
        /// Throws if the current user isn't the family's owner. Applies to
        /// both admins and non-admins — write operations are owner-only.
        /// </summary>
        private async Task EnsureOwnerAsync(Family family, CancellationToken cancellationToken)
        {
            var user = await _userService.GetCurrentUser(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");
            if (family.OwnerId != user.Id)
                throw new UnauthorizedAccessException("Faqat o'zingiz yaratgan oilani tahrirlay yoki o'chira olasiz.");
        }

        private async Task<bool> IsAdminAsync(User user, CancellationToken cancellationToken)
        {
            var role = await _userRoleRepository.GetByIdAsync(user.RoleId, cancellationToken);
            return string.Equals(role?.DesignedName, AdminRoleDesignedName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
