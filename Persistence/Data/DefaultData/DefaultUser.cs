using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.DefaultData
{
    public sealed class DefaultUser
    {
        public const string DefaultPassword = "Admin123!";

        public static readonly User Instance = new()
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            UserName = "admin",
            Email = "admin@familytree.uz",
            Phone = "+998901234567",
            RoleId = DefaultUserRole.Instance[0].Id
        };

        private DefaultUser() { }
    }
}
