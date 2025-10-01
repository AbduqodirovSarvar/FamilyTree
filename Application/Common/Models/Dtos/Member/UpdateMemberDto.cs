using Application.Common.Models.Dtos.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.Member
{
    public record UpdateMemberDto : BaseUpdateDto
    {
        public string? FirstName { get; init; } = null;
        public string? LastName { get; init; } = null;
        public string? Description { get; init; } = null;
        public DateOnly? BirthDay { get; init; } = null;
        public DateOnly? DeathDay { get; init; } = null;
        public Gender? Gender { get; init; } = null;
        public Guid? FamilyId { get; init; } = null;
        public IFormFile? Image { get; init; } = null;
        public Guid? FatherId { get; init; } = null;
        public Guid? MotherId { get; init; } = null;
        public Guid? SpouseId { get; init; } = null;
    }
}
