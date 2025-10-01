using Application.Common.Models.Dtos.UploadedFile;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Commands.Create
{
    public record CreateUploadedFileCommand : CreateUploadedFileDto, IRequest<Response<UploadedFileViewModel>>
    {
    }
}
