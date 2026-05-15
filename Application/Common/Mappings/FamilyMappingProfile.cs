using Application.Common.Models.Dtos.Family;
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
            // Members is ignored to prevent the same recursive-graph blowup that hit
            // MemberMappingProfile: if anyone later adds .Include(f => f.Members) to a
            // Family query, EF fix-up + AutoMapper would expand each Member's Father/
            // Mother/Spouse/Children back into the response. Frontend uses /Member/list
            // to fetch members of a family, not the nested collection here.
            CreateMap<Family, FamilyViewModel>()
                .ForMember(d => d.Members, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<CreateFamilyDto, Family>()
                .ForMember(f => f.ImageId, opt => opt.Ignore())
                .ForMember(x => x.Image, opt => opt.Ignore());
            CreateMap<UpdateFamilyDto, Family>()
                .ForMember(f => f.Name, opt => opt.Condition(ufc => ufc.Name != null))
                .ForMember(f => f.Description, opt => opt.MapFrom(ufc => ufc.Description))
                .ForMember(f => f.FamilyName, opt => opt.Condition(ufc => ufc.FamilyName != null))
                .ForMember(f => f.ImageId, opt => opt.Ignore())
                .ForMember(x => x.Image, opt => opt.Ignore())
                .ForMember(f => f.OwnerId, opt => opt.Condition(ufc => ufc.OwnerId != null));
        }
    }
}
