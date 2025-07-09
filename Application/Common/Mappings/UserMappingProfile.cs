using Application.Common.Models;
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
        }
    }
}
