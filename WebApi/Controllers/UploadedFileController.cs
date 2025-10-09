using Application.Common.Models.Dtos.Common;
using Application.Features.UploadedFile.Commands.Delete;
using Application.Features.UploadedFile.Commands.Update;
using Application.Features.UploadedFile.Queries.GetLIst;
using Application.Features.UploadedFile.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadedFileController(IMediator mediator) 
        : BaseServiceController<
            BaseCreateDto,
            UpdateUploadedFileCommand,
            DeleteUploadedFileCommand,
            GetUploadedFileQuery,
            GetUploadedFileListQuery>(mediator)
    {
        public override Task<IActionResult> Post([FromForm] BaseCreateDto command)
        {
            throw new NotImplementedException("Uploaded file creation is not supported via this endpoint.");
        }

        [HttpGet("file")]
        public async Task<IActionResult> GetFile([FromQuery] Guid fileId)
        {
            var uploadedFile = await _mediator.Send(new GetUploadedFileQuery() { Id = fileId })
                                ?? throw new FileNotFoundException();

            byte[]? file = await _mediator.Send(new GetUploadedFileQuery { Id = fileId })
                                ?? throw new FileNotFoundException();
            return File(file, "application/octet-stream", $"file_{fileId}");
        }
    }
}
