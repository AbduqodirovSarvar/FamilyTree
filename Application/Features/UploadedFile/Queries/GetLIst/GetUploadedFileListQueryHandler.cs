using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UploadedFileEntity = Domain.Entities.UploadedFile;

namespace Application.Features.UploadedFile.Queries.GetLIst
{
    public class GetUploadedFileListQueryHandler(
        IUploadedFileService uploadedFileService
        ) : IRequestHandler<GetUploadedFileListQuery, Response<List<UploadedFileViewModel>>>
    {
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;

        public async Task<Response<List<UploadedFileViewModel>>> Handle(GetUploadedFileListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<UploadedFileEntity, bool>>? predicate =
                request.Filters.BuildPredicate<UploadedFileEntity>()
                    .AndAlso(FilterExpressionBuilder.BuildSearchPredicate<UploadedFileEntity>(
                        request.SearchText,
                        nameof(UploadedFileEntity.Name),
                        nameof(UploadedFileEntity.Alt),
                        nameof(UploadedFileEntity.Description)));

            return await _uploadedFileService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
