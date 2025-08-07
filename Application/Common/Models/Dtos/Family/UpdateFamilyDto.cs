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
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? FamilyName { get; set; } = null;
        public Guid? ImageId { get; set; } = null;
        public Guid? OwnerId { get; set; } = null;
    }
}
