using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.DefaultData
{
    public sealed class DefaultUserRole
    {
        public static readonly UserRole Instance = new()
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "System Administrator with full permissions",
            DesignedName = "ADMIN"
        };

        private DefaultUserRole() { }
    }
}
