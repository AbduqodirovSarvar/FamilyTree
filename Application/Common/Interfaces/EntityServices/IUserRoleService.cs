using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Models;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Dtos.UserRole;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices
{
    internal interface IUserRoleService 
        : IGenericEntityService<UserRole, CreateUserRoleDto, UpdateUserRoleDto, UserRoleViewModel>
    {
    }
}
