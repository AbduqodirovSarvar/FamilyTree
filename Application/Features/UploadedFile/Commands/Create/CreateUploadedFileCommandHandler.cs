using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UploadedFile.Commands.Create
{
    public class CreateUploadedFileCommandHandler(
        IUploadedFileService uploadedFileService
        ) : IRequestHandler<CreateUploadedFileCommand, Response<UploadedFileViewModel>>
    {
        private readonly IUploadedFileService _uploadedFileService = uploadedFileService;
        public async Task<Response<UploadedFileViewModel>> Handle(CreateUploadedFileCommand request, CancellationToken cancellationToken)
        {
            if(request == null || request?.File == null) 
                throw new ArgumentNullException(nameof(request));

            var result = await _uploadedFileService.CreateAsync(request, cancellationToken);

            return Response<UploadedFileViewModel>.Ok(result, "Uploaded file created successfully.");
        }
    }
}
