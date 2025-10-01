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

            var entity = await _fileService.SaveFileAsync(entityCreateDto.File, Guid.NewGuid().ToString());

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<UploadedFileViewModel>(result);
        }
    }
}
