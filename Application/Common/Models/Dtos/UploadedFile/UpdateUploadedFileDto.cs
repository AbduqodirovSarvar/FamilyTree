using Application.Common.Models.Dtos.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.UploadedFile
{
    public record UpdateUploadedFileDto : BaseUpdateDto
    {
        public IFormFile? File { get; init; } = null!;
        public string? Description { get; init; } = null;
        public string? Alt { get; init; } = null;
    }
}
