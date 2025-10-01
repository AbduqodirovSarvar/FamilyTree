using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Update;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Mappings
{
    public class UploadedFileMappingProfile : Profile
    {
        public UploadedFileMappingProfile()
        {
            CreateMap<UploadedFile, UploadedFileViewModel>().ReverseMap();
            CreateMap<UpdateUploadedFileCommand, UploadedFile>()
                .ForMember(uf => uf.Description, opt => opt.MapFrom(ufc => ufc.Description))
                .ForMember(uf => uf.Alt, opt => opt.MapFrom(ufc => ufc.Alt));
        }
    }
}
