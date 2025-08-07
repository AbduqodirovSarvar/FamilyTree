using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserRole : AudiTableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = null;
        public string? DesignedName { get; set; } = null;

        public Guid? FamilyId { get; set; }
        public Family? Family { get; set; }

        public ICollection<UserRolePermission> Permissions { get; set; } = [];
    }
}
