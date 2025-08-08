using Application.Common.Models;
using Application.Common.Models.Dtos.UploadedFile;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Commands.Update
{
    public record UpdateUploadedFileCommand : UpdateUploadedFileDto, IRequest<Response<UploadedFileViewModel>>
    {
    }
}
