using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Models.Dtos.Family;
using Application.Common.Models.ViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices
{
    public interface IFamilyService 
        : IGenericEntityService<Family, CreateFamilyDto, UpdateFamilyDto, FamilyViewModel>
    {
    }
}
