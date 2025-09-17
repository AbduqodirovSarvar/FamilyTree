using Application.Common.Models.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.UserRole
{
    public record CreateUserRoleDto : BaseCreateDto
    {
        public string Name { get; init; } = null!;
        public string? Description { get; init; } = null;
        public string? DesignedName { get; init; } = null;
        public Guid? FamilyId { get; init; }
    }
}
