using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserRolePermission : AudiTableEntity
    {
        public Guid UserRoleId { get; set; }
        public UserRole? UserRole { get; set; }
        public Permission Permission { get; set; } = Permission.GET_FAMILY;
    }
}
