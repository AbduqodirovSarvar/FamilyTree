using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum Permission
    {
        // Family Operations
        GET_FAMILY = 1,
        CREATE_FAMILY = 2,
        UPDATE_FAMILY = 3,
        DELETE_FAMILY = 4,

        // Member Operations
        GET_MEMBER = 5,
        CREATE_MEMBER = 6,
        UPDATE_MEMBER = 7,
        DELETE_MEMBER = 8,

        // User Operations
        GET_USER = 9,
        UPDATE_USER = 10,
        DELETE_USER = 11,

        // Role Operations
        GET_ROLE = 12,
        CREATE_ROLE = 13,
        UPDATE_ROLE = 14,
        DELETE_ROLE = 15
    }
}
