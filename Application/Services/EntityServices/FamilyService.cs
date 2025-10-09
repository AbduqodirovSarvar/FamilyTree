using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Family;
using Application.Common.Models.Dtos.UploadedFile;
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
    internal class FamilyService(
        IFamilyRepository familyRepository,
        IPermissionService permissionService,
        IUserService userService,
        IMediator mediator,
        IMapper mapper) 
        : GenericEntityService<Family, CreateFamilyDto, UpdateFamilyDto, FamilyViewModel>(familyRepository, permissionService, mapper), IFamilyService
    {
        private readonly IUserService _userService = userService;
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
            if(entityCreateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityCreateDto.Image, Alt = entity.Name, Description = entityCreateDto.Description }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if(image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<FamilyViewModel>(result);
        }

        public override async Task<FamilyViewModel> UpdateAsync(UpdateFamilyDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<Family>(entityUpdateDto);

            if (entityUpdateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityUpdateDto.Image, Alt = entity.Name, Description = entityUpdateDto.Description }, cancellationToken)
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
