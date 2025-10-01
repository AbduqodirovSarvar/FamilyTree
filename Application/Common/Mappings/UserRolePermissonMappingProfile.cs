using Application.Common.Models.Dtos.UserRolePermission;
using Application.Common.Models.ViewModels;
using Application.Features.UserRolePermission.Commands.Create;
using Application.Features.UserRolePermission.Commands.Update;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Mappings
{
    public class UserRolePermissonMappingProfile : Profile
    {
        public UserRolePermissonMappingProfile()
        {
            CreateMap<UserRolePermission, UserRolePermissionViewModel>().ReverseMap();
            CreateMap<CreateUserRolePermissionDto, UserRolePermission>().ReverseMap();
            CreateMap<UpdateUserRolePermissionDto, UserRolePermission>()
                .ForMember(urp => urp.UserRoleId, opt => opt.Condition(urpc => urpc.UserRoleId != null))
                .ForMember(urp => urp.Permission, opt => opt.Condition(urpc => urpc.Permission != null));
        }
    }
}
