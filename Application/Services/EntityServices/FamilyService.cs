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
    ///   <item><description><b>List (read)</b> — admin sees every family; non-admins see only the
    ///   ones they created (filtered by <c>OwnerId == currentUser.Id</c>).</description></item>
    ///   <item><description><b>Update / Delete</b> — owner-only, <i>regardless of role</i>.
    ///   Even an admin cannot edit or delete a family someone else created.</description></item>
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
        IMapper mapper)
        : GenericEntityService<Family, CreateFamilyDto, UpdateFamilyDto, FamilyViewModel>(familyRepository, permissionService, mapper), IFamilyService
    {
        private const string AdminRoleDesignedName = "ADMIN";

        private readonly IUserService _userService = userService;
        private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
        private readonly IMediator _mediator = mediator;

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

            return _mapper.Map<FamilyViewModel>(result);
        }

        /// <summary>
        /// Augments the base list query with an ownership predicate when the
        /// current user isn't an admin. Admin still sees everything (e.g. for
        /// system-wide moderation in the preview module).
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
                Expression<Func<Family, bool>> ownPredicate = f => f.OwnerId == userId;
                predicate = ownPredicate.AndAlso(predicate);
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

            if (!string.IsNullOrWhiteSpace(entityUpdateDto.Name)) entity.Name = entityUpdateDto.Name;
            if (!string.IsNullOrWhiteSpace(entityUpdateDto.FamilyName)) entity.FamilyName = entityUpdateDto.FamilyName;
            if (entityUpdateDto.Description != null) entity.Description = entityUpdateDto.Description;
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
