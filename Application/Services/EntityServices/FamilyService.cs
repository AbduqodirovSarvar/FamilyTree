using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Models.Dtos.Family;
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
    internal class FamilyService(
        IFamilyRepository familyRepository,
        IPermissionService permissionService,
        IMapper mapper) 
        : GenericEntityService<Family, CreateFamilyDto, UpdateFamilyDto, FamilyViewModel>(familyRepository, permissionService, mapper), IFamilyService
    {
    }
}
