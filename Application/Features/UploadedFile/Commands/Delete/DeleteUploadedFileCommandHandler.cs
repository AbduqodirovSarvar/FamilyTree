using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Commands.Delete
{
    public class DeleteUploadedFileCommandHandler(
        IUploadedFileService uploadedFileService
        ) : IRequestHandler<DeleteUploadedFileCommand, Response<bool>>
    {
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;
        public async Task<Response<bool>> Handle(DeleteUploadedFileCommand request, CancellationToken cancellationToken)
        {
            var result = await _uploadedFileService.DeleteAsync(request.Id, cancellationToken);
            if(result)
            {
                return Response<bool>.Ok(true, "File deleted successfully.");
            }
            return Response<bool>.Fail("Failed to delete file.");
        }
    }
}
