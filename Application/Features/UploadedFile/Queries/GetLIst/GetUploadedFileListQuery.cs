using Application.Common.Models.Request;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Queries.GetLIst
{
    public record GetUploadedFileListQuery : BaseGetListQuery, IRequest<Response<List<UploadedFileViewModel>>>
    {
    }
}
