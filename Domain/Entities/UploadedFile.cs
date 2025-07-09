using Domain.Behaviours;
using Domain.Common;

namespace Domain.Entities
{
    public class UploadedFile : AudiTableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        [UriValidation]
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Description { get; set; } = null;
        public string? Alt { get; set; } = null;
    }
}
