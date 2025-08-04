using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum Role
    {
        User = 1,
        Admin = 2,
        SuperAdmin = 3,
        Moderator = 6,
        Editor = 7,
        Viewer = 8,
        Contributor = 9,
        Custom = 10
    }
}
