using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.UserRolePermission;
using Application.Common.Models.ViewModels;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    internal class UserRolePermissionService(
        IUserRolePermissionRepository userRolePermissionRepository,
        IPermissionService permissionService,
        IMapper mapper) 
        : GenericEntityService<UserRolePermission, CreateUserRolePermissionDto, UpdateUserRolePermissionDto, UserRolePermissionViewModel>(userRolePermissionRepository, permissionService, mapper), IUserRolePermissionService
    {
    }
}
