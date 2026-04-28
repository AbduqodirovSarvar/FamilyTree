using Application.Common.Models.Request;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.UploadedFile.Queries.CheckExist
{
    public record CheckUploadedFileExistQuery : BaseCheckExistQuery, IRequest<Response<bool>>
    {
        public Guid? Id { get; init; }
        public string? Path { get; init; }
        public string? Url { get; init; }
    }
}
