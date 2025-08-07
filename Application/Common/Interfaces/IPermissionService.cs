using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IPermissionService
    {
        bool CheckPermission(string entityName, OperationType operation, User? user = null);
    }
}
