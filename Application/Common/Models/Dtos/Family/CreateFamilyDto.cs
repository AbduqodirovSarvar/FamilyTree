using Application.Common.Models.Dtos.Common;
using Microsoft.AspNetCore.Http;
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
        public string? Description { get; init; } = null;
        public IFormFile? Image { get; init; } = null;
    }
}
