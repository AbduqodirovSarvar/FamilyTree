using Application.Common.Models.Common;
using Domain.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.ViewModels
{
    public record UploadedFileViewModel : BaseViewModel
    {
        public string Name { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public long Size { get; init; }
        public string? Description { get; init; } = null;
        public string? Alt { get; init; } = null;
    }
}
