using Application.Common.Models.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.Family
{
    public record UpdateFamilyDto : BaseUpdateDto
    {
        public string? Name { get; init; } = null;
        public string? Description { get; init; } = null;
        public string? FamilyName { get; init; } = null;
        public Guid? ImageId { get; init; } = null;
        public Guid? OwnerId { get; init; } = null;
    }
}
