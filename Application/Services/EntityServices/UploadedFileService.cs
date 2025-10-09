using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.UploadedFile;
using Application.Common.Models.ViewModels;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    internal class UploadedFileService(
        IUploadedFileRepository uploadedFileRepository,
        IPermissionService permissionService,
        IFileService fileService,
        IMapper mapper) 
        : GenericEntityService<UploadedFile, CreateUploadedFileDto, UpdateUploadedFileDto, UploadedFileViewModel>(uploadedFileRepository, permissionService, mapper), IUploadedFileService
    {
        private readonly IFileService _fileService = fileService;
        public override async Task<UploadedFileViewModel> CreateAsync(CreateUploadedFileDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(UploadedFile).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _fileService.SaveFileAsync(entityCreateDto.File)
                                ?? throw new FileLoadException("Could not save this file.");

            entity.Description = entityCreateDto.Description;
            entity.Alt = entityCreateDto.Alt;

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<UploadedFileViewModel>(result);
        }

        public override async Task<UploadedFileViewModel> UpdateAsync(UpdateUploadedFileDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(UploadedFile).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE, null))
                throw new UnauthorizedAccessException("You do not have permission to update this entity.");

            var existingEntity = await _repository.GetByIdAsync(entityUpdateDto.Id, cancellationToken)
                                    ?? throw new KeyNotFoundException("Entity not found.");

            if(entityUpdateDto?.File != null)
            {
                var newFile = await _fileService.SaveFileAsync(entityUpdateDto.File)
                    ?? throw new FileLoadException("Could not save this file.");

                await _fileService.DeleteFileAsync(existingEntity.Name);

                existingEntity.Name = newFile.Name;
                existingEntity.Path = newFile.Path;
                existingEntity.Type = newFile.Type;
                existingEntity.Size = newFile.Size;
                existingEntity.Url = newFile.Url;
            }

            existingEntity.Description = entityUpdateDto?.Description ?? existingEntity.Description;
            existingEntity.Alt = entityUpdateDto?.Alt ?? existingEntity.Alt;

            var result = await _repository.UpdateAsync(existingEntity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<UploadedFileViewModel>(result);
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(UploadedFile).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.DELETE, null))
                throw new UnauthorizedAccessException("You do not have permission to delete this entity.");

            var entity = await _repository.GetByIdAsync(id, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");

            await _fileService.DeleteFileAsync(entity.Name);

            return await _repository.DeleteAsync(entity, cancellationToken);
        }
    }
}
