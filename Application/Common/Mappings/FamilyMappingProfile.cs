using Application.Common.Models.ViewModels;
using Application.Features.Family.Commands.Create;
using Application.Features.Family.Commands.Update;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Mappings
{
    public class FamilyMappingProfile : Profile
    {
        public FamilyMappingProfile() {
            CreateMap<Family, FamilyViewModel>().ReverseMap();
            CreateMap<CreateFamilyCommand, Family>()
                .ForMember(f => f.ImageId, opt => opt.Ignore());
            CreateMap<UpdateFamilyCommand, Family>()
                .ForMember(f => f.Name, opt => opt.Condition(ufc => ufc.Name != null))
                .ForMember(f => f.Description, opt => opt.MapFrom(ufc => ufc.Description))
                .ForMember(f => f.FamilyName, opt => opt.Condition(ufc => ufc.FamilyName != null))
                .ForMember(f => f.ImageId, opt => opt.Ignore())
                .ForMember(f => f.OwnerId, opt => opt.Condition(ufc => ufc.OwnerId != null));
        }
    }
}
