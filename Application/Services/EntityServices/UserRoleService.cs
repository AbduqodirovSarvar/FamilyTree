using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.UserRole;
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
    internal class UserRoleService(
        IUserRoleRepository userRoleRepository,
        IPermissionService permissionService,
        IMapper mapper)
        : GenericEntityService<UserRole, CreateUserRoleDto, UpdateUserRoleDto, UserRoleViewModel>(userRoleRepository, permissionService, mapper), IUserRoleService
    {
        public override async Task<UserRoleViewModel> UpdateAsync(UpdateUserRoleDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(UserRole).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<UserRole>(entityUpdateDto);

            var result = await _repository.UpdateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<UserRoleViewModel>(result);

        }
    }
}
