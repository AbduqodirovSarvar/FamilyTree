using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Queries.GetLIst
{
    public class GetUploadedFileListQueryHandler(
        IUploadedFileService uploadedFileService
        ) : IRequestHandler<GetUploadedFileListQuery, Response<List<UploadedFileViewModel>>>
    {
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;
        public async Task<Response<List<UploadedFileViewModel>>> Handle(GetUploadedFileListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.UploadedFile, bool>>? predicate = null;
            if (request.Filters != null && request.Filters.Any())
            {
                predicate = request.Filters.BuildPredicate<Domain.Entities.UploadedFile>();
            }

            return await _uploadedFileService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
