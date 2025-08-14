using Application.Common.Models.Request;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Queries.GetOne
{
    public record GetUploadedFileQuery : BaseGetOneQuery, IRequest<byte[]>
    {
    }
}
