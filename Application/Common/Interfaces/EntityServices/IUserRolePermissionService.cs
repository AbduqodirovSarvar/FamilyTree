using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Models;
using Application.Common.Models.Dtos.UserRolePermission;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices
{
    internal interface IUserRolePermissionService
        : IGenericEntityService<UserRolePermission, CreateUserRolePermissionDto, UpdateUserRolePermissionDto, UserRolePermissionViewModel>
    {
    }
}
