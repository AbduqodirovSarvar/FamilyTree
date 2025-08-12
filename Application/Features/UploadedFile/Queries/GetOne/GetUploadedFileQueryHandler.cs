using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Queries.GetOne
{
    public class GetUploadedFileQueryHandler(
        IFileService fileService,
        IUploadedFileService uploadedFileService
        ) : IRequestHandler<GetUploadedFileQuery, byte[]>
    {
        private readonly IFileService _fileService = fileService;
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;
        public async Task<byte[]> Handle(GetUploadedFileQuery request, CancellationToken cancellationToken)
        {
            var uploadedFile = await _uploadedFileService.GetByIdAsync(request.Id, cancellationToken)
                                        ?? throw new InvalidOperationException("File not found.");

            var file = await _fileService.GetFileAsync(uploadedFile.Name)
                                    ?? throw new FileNotFoundException("File not found.");

            return file;
        }
    }
}
