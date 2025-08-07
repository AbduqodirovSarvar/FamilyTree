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
        CREATE_USER = 10,
        UPDATE_USER = 11,
        DELETE_USER = 12,

        // Role Operations
        GET_ROLE = 13,
        CREATE_ROLE = 14,
        UPDATE_ROLE = 15,
        DELETE_ROLE = 16,

        // File Operations
        GET_FILE = 17,
        CREATE_FILE = 18,
        UPDATE_FILE = 19,
        DELETE_FILE = 20,

        // Role Permission Operations
        GET_ROLE_PERMISSION = 21,
        CREATE_ROLE_PERMISSION = 22,
        UPDATE_ROLE_PERMISSION = 23,
        DELETE_ROLE_PERMISSION = 24,
    }
}
