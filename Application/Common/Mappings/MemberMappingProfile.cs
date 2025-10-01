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
            CreateMap<Member, MemberViewModel>().ReverseMap();
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
