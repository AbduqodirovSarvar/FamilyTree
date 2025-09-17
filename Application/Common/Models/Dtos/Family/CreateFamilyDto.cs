using Application.Common.Models.Dtos.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.Family
{
    public record CreateFamilyDto : BaseCreateDto
    {
        [Required]
        public string Name { get; init; } = null!;
        [Required]
        public string FamilyName { get; init; } = null!;
        [Required]
        public Guid OwnerId { get; init; }
        public string? Description { get; init; } = null;
        public Guid? ImageId { get; init; } = null;
    }
}
