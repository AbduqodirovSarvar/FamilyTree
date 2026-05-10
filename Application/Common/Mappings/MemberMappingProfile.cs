using Application.Common.Models.Dtos.Member;
using Application.Common.Models.ViewModels;
using Application.Features.Member.Commands.Create;
using Application.Features.Member.Commands.Update;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Mappings
{
    public class MemberMappingProfile : Profile
    {
        public MemberMappingProfile()
        {
            // Self-referencing navigations (Father/Mother/Spouse/Children) are ignored
            // because EF Core's relationship fix-up populates them whenever multiple
            // members of the same family are materialized in one DbContext, and AutoMapper
            // would then expand the graph recursively — turning a 35-row /Member/list
            // response into multi-megabyte JSON. Frontend uses *Id scalars to look up
            // related members within the same list.
            CreateMap<Member, MemberViewModel>()
                .ForMember(d => d.Father, opt => opt.Ignore())
                .ForMember(d => d.Mother, opt => opt.Ignore())
                .ForMember(d => d.Spouse, opt => opt.Ignore())
                .ForMember(d => d.Children, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<CreateMemberDto, Member>()
                .ForMember(f => f.ImageId, opt => opt.Ignore())
                .ForMember(x => x.Image, opt => opt.Ignore());
            CreateMap<UpdateMemberDto, Member>()
                .ForMember(m => m.FirstName, opt => opt.Condition(umc => umc.FirstName != null))
                .ForMember(m => m.LastName, opt => opt.MapFrom(umc => umc.LastName))
                .ForMember(m => m.Description, opt => opt.MapFrom(umc => umc.Description))
                .ForMember(m => m.BirthDay, opt => opt.Condition(umc => umc.BirthDay != null))
                .ForMember(m => m.DeathDay, opt => opt.MapFrom(umc => umc.DeathDay))
                .ForMember(m => m.Gender, opt => opt.Condition(umc => umc.Gender != null))
                .ForMember(m => m.FamilyId, opt => opt.MapFrom(umc => umc.FamilyId))
                .ForMember(f => f.ImageId, opt => opt.Ignore())
                .ForMember(x => x.Image, opt => opt.Ignore())
                .ForMember(m => m.FatherId, opt => opt.MapFrom(umc => umc.FatherId))
                .ForMember(m => m.MotherId, opt => opt.MapFrom(umc => umc.MotherId))
                .ForMember(m => m.SpouseId, opt => opt.MapFrom(umc => umc.SpouseId));
        }
    }
}
