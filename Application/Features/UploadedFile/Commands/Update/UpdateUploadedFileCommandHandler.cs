using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Commands.Update
{
    public class UpdateUploadedFileCommandHandler(
        IUploadedFileService uploadedFileService
        ) : IRequestHandler<UpdateUploadedFileCommand, Response<UploadedFileViewModel>>
    {
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;
        public async Task<Response<UploadedFileViewModel>> Handle(UpdateUploadedFileCommand request, CancellationToken cancellationToken)
        {
            var result = await _uploadedFileService.UpdateAsync(request, cancellationToken)
                                 ?? throw new InvalidOperationException("Uploaded file update failed.");

            return Response<UploadedFileViewModel>.Ok(result, "Uploaded file updated successfully.");
        }
    }
}
