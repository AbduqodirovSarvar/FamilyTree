using Application.Common.Models.Dtos.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.Member
{
    public record CreateMemberDto : BaseCreateDto
    {
        public string? FirstName { get; init; } = null!;
        public string? LastName { get; init; } = null;
        public string? Description { get; init; } = null;
        public DateOnly BirthDay { get; init; }
        public DateOnly? DeathDay { get; init; } = null;
        public Gender Gender { get; init; } = Gender.MALE;
        public Guid FamilyId { get; init; }
        public Guid? ImageId { get; init; }
        public Guid? FatherId { get; init; } = null;
        public Guid? MotherId { get; init; } = null;
        public Guid? SpouseId { get; init; } = null;
    }
}
