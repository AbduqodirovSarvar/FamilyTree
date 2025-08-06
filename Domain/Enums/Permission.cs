using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum Permission
    {
        // Family Permissions
        FamilyCreate = 1,
        FamilyRead = 2,
        FamilyUpdate = 3,
        FamilyDelete = 4,

        // User Permissions
        UserCreate = 5,
        UserRead = 6,
        UserUpdate = 7,
        UserDelete = 8,

        // Role Permissions
        RoleCreate = 9,
        RoleRead = 10,
        RoleUpdate = 11,
        RoleDelete = 12,

        // Full Permissions
        FullCreate = 13,
        FullRead = 14,
        FullUpdate = 15,
        FullDelete = 16,
    }
}
