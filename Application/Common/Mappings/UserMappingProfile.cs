using Application.Common.Models.ViewModels;
using Application.Features.Auth.Commands.SignUp;
using Application.Features.User.Commands.Update;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() {
            CreateMap<User, UserViewModel>().ReverseMap();
            CreateMap<SignUpCommand, User>()
                .ForMember(sc => sc.PasswordHash, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<UpdateUserCommand, User>()
                .ForMember(u => u.FirstName, opt => opt.Condition(uc => uc.FirstName != null))
                .ForMember(u => u.LastName, opt => opt.Condition(uc => uc.LastName != null))
                .ForMember(u => u.UserName, opt => opt.MapFrom(uc => uc.UserName))
                .ForMember(u => u.Email, opt => opt.Condition(uc => uc.Email != null))
                .ForMember(u => u.Phone, opt => opt.MapFrom(uc => uc.Phone))
                .ForMember(u => u.FamilyId, opt => opt.MapFrom(uc => uc.FamilyId))
                .ForMember(u => u.RoleId, opt => opt.Condition(uc => uc.RoleId != null))
                .ForMember(u => u.PasswordHash, opt => opt.Ignore());
        }
    }
}
