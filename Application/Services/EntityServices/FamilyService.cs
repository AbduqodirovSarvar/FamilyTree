using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Family;
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
    internal class FamilyService(
        IFamilyRepository familyRepository,
        IPermissionService permissionService,
        IUserService userService,
        IMapper mapper) 
        : GenericEntityService<Family, CreateFamilyDto, UpdateFamilyDto, FamilyViewModel>(familyRepository, permissionService, mapper), IFamilyService
    {
        private readonly IUserService _userService = userService;
        public override async Task<FamilyViewModel> CreateAsync(CreateFamilyDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<Family>(entityCreateDto);

            var user = await _userService.GetCurrentUser(cancellationToken)
                        ?? throw new KeyNotFoundException("Current user not found.");

            entity.OwnerId = user.Id;

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<FamilyViewModel>(result);
        }
    }
}
