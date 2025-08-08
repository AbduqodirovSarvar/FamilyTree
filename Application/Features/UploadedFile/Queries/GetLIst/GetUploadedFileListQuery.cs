using Application.Common.Models;
using Application.Common.Models.Request;
using Application.Common.Models.Result;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Queries.GetLIst
{
    public record GetUploadedFileListQuery : BaseQuery, IRequest<Response<List<UploadedFileViewModel>>>
    {
    }
}
