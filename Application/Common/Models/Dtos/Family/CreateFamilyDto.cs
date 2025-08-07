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
        public string Name { get; set; } = null!;
        [Required]
        public string FamilyName { get; set; } = null!;
        [Required]
        public Guid OwnerId { get; set; }
        public string? Description { get; set; } = null;
        public Guid? ImageId { get; set; } = null;
    }
}
