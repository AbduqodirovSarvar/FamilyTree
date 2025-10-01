using Application.Common.Models.Dtos.UserRole;
using Application.Common.Models.ViewModels;
using Application.Features.UserRole.Commands.Create;
using Application.Features.UserRole.Commands.Update;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Mappings
{
    public class UserRoleMappingProfile : Profile
    {
        public UserRoleMappingProfile()
        {
            CreateMap<UserRole, UserRoleViewModel>().ReverseMap();
            CreateMap<CreateUserRoleDto, UserRole>().ReverseMap();
            CreateMap<UpdateUserRoleDto, UserRole>()
                .ForMember(ur => ur.Name, opt => opt.Condition(urc => urc.Name != null))
                .ForMember(ur => ur.Description, opt => opt.MapFrom(urc => urc.Description))
                .ForMember(ur => ur.DesignedName, opt => opt.MapFrom(urc => urc.DesignedName))
                .ForMember(ur => ur.FamilyId, opt => opt.MapFrom(urc => urc.FamilyId));
        }
    }
}
