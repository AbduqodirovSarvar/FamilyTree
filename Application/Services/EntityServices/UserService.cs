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
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<User>(entityUpdateDto);

            if (entityUpdateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityUpdateDto.Image, Alt = entity.FirstName, Description = null }, cancellationToken)
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
