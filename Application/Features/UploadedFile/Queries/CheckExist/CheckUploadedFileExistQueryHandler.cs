using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.UploadedFile.Queries.CheckExist
{
    public class CheckUploadedFileExistQueryHandler(IUploadedFileService uploadedFileService)
        : IRequestHandler<CheckUploadedFileExistQuery, Response<bool>>
    {
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;

        public async Task<Response<bool>> Handle(CheckUploadedFileExistQuery request, CancellationToken cancellationToken)
        {
            bool hasId = request.Id != null && request.Id != Guid.Empty;
            bool hasPath = !string.IsNullOrWhiteSpace(request.Path);
            bool hasUrl = !string.IsNullOrWhiteSpace(request.Url);

            if (!hasId && !hasPath && !hasUrl)
                return Response<bool>.Fail("At least one of Id, Path, or Url must be provided.");

            Expression<Func<Domain.Entities.UploadedFile, bool>> predicate = x =>
                (hasId && x.Id == request.Id!.Value)
                || (hasPath && x.Path == request.Path)
                || (hasUrl && x.Url == request.Url);

            var exists = await _uploadedFileService.ExistsAsync(predicate, cancellationToken);
            return Response<bool>.Ok(exists);
        }
    }
}
