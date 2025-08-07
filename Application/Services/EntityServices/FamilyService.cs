using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Services.EntityServices.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    internal class FamilyService(
        IFamilyRepository familyRepository,
        IPermissionService permissionService) 
        : GenericEntityService<Family>(familyRepository, permissionService), IFamilyService
    {
    }
}
