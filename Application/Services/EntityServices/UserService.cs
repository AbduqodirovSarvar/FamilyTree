using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Member;
using Application.Common.Models.Dtos.User;
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
    internal class UserService(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        IMediator mediator,
        IMapper mapper)
        : GenericEntityService<User, CreateUserDto, UpdateUserDto, UserViewModel>(userRepository, permissionService, mapper), IUserService
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IMediator _mediator = mediator;
        public async Task<User?> GetCurrentUser(CancellationToken cancellationToken = default)
        {
            return await _currentUserService.GetCurrentUserAsync(cancellationToken);
        }
        public override async Task<UserViewModel> CreateAsync(CreateUserDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<User>(entityCreateDto);

            if (entityCreateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityCreateDto.Image, Alt = entity.FirstName, Description = null }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<UserViewModel>(result);
        }

        public override async Task<UserViewModel> UpdateAsync(UpdateUserDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(User).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to update this entity.");

            // Load existing entity to preserve fields not present in the DTO.
            var entity = await _repository.GetByIdAsync(entityUpdateDto.Id, cancellationToken)
                            ?? throw new KeyNotFoundException("User not found");

            // PUT semantic for required strings: still guard against blank
            // so a partial save can't accidentally null out FirstName/UserName
            // (those columns aren't nullable).
            if (!string.IsNullOrWhiteSpace(entityUpdateDto.FirstName)) entity.FirstName = entityUpdateDto.FirstName;
            if (!string.IsNullOrWhiteSpace(entityUpdateDto.UserName)) entity.UserName = entityUpdateDto.UserName;

            // PUT semantic for nullable strings: empty string clears the column.
            entity.LastName = string.IsNullOrEmpty(entityUpdateDto.LastName) ? null : entityUpdateDto.LastName;
            entity.Phone = string.IsNullOrEmpty(entityUpdateDto.Phone) ? null : entityUpdateDto.Phone;
            entity.Email = string.IsNullOrEmpty(entityUpdateDto.Email) ? null : entityUpdateDto.Email;

            // Nullable FamilyId — Guid.Empty clears it, anything else (incl. null) is the new value.
            entity.FamilyId = entityUpdateDto.FamilyId == Guid.Empty ? null : entityUpdateDto.FamilyId;

            if (entityUpdateDto.RoleId.HasValue && entityUpdateDto.RoleId.Value != Guid.Empty)
                entity.RoleId = entityUpdateDto.RoleId.Value;

            if (entityUpdateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand
                {
                    File = entityUpdateDto.Image,
                    Alt = entity.FirstName,
                    Description = null
                }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.UpdateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<UserViewModel>(result);
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(User).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.DELETE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");

            if (entity.ImageId.HasValue && entity.ImageId.Value != Guid.Empty)
            {
                await _mediator.Send(new DeleteUploadedFileCommand()
                {
                    Id = entity.ImageId.Value
                }, cancellationToken);
            }

            return await _repository.DeleteAsync(entity, cancellationToken);
        }
    }
}
