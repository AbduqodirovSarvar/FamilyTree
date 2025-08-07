using Application.Common.Interfaces.EntityServices.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices
{
    internal interface IUserRoleService 
        : IGenericEntityService<UserRole>
    {
    }
}
