using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.DefaultData
{
    public sealed class DefaultUserRolePermissions
    {
        public static readonly List<UserRolePermission> Instances = [.. Enum
            .GetValues<Permission>()
            .Cast<Permission>()
            .Select(permission => new UserRolePermission
            {
                Id = Guid.NewGuid(),
                UserRoleId = DefaultUserRole.Instance[0].Id,
                Permission = permission
            })];

        private DefaultUserRolePermissions() { }
    }
}
