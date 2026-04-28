using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Family;
using Application.Common.Models.Dtos.Member;
using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Create;
using Application.Features.UploadedFile.Commands.Delete;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    internal class MemberService(
        IMemberRepository memberRepository,
        IPermissionService permissionService,
        IMediator mediator,
        IMapper mapper) 
        : GenericEntityService<Member, CreateMemberDto, UpdateMemberDto, MemberViewModel>(memberRepository, permissionService, mapper), IMemberService
    {
        private readonly IMediator _mediator = mediator;
        public override async Task<MemberViewModel> CreateAsync(CreateMemberDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Member).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

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

        public override async Task<MemberViewModel> UpdateAsync(UpdateMemberDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Member).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to update this entity.");

            // Load existing entity to preserve fields not present in the DTO.
            var entity = await _repository.GetByIdAsync(entityUpdateDto.Id, cancellationToken)
                            ?? throw new KeyNotFoundException("Member not found");

            if (entityUpdateDto.FirstName != null) entity.FirstName = entityUpdateDto.FirstName;
            if (entityUpdateDto.LastName != null) entity.LastName = entityUpdateDto.LastName;
            if (entityUpdateDto.Description != null) entity.Description = entityUpdateDto.Description;
            if (entityUpdateDto.BirthDay.HasValue) entity.BirthDay = entityUpdateDto.BirthDay.Value;
            if (entityUpdateDto.DeathDay.HasValue) entity.DeathDay = entityUpdateDto.DeathDay;
            if (entityUpdateDto.Gender.HasValue) entity.Gender = entityUpdateDto.Gender.Value;
            if (entityUpdateDto.FamilyId.HasValue && entityUpdateDto.FamilyId.Value != Guid.Empty)
                entity.FamilyId = entityUpdateDto.FamilyId.Value;
            // Father/Mother/Spouse are nullable — null clears the link, a Guid sets it.
            entity.FatherId = entityUpdateDto.FatherId == Guid.Empty ? null : entityUpdateDto.FatherId ?? entity.FatherId;
            entity.MotherId = entityUpdateDto.MotherId == Guid.Empty ? null : entityUpdateDto.MotherId ?? entity.MotherId;
            entity.SpouseId = entityUpdateDto.SpouseId == Guid.Empty ? null : entityUpdateDto.SpouseId ?? entity.SpouseId;

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
    }
}
