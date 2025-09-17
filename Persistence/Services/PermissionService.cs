using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class PermissionService : IPermissionService
    {
        public bool CheckPermission(string entityName, OperationType operation, User? user = null)
        {
            throw new NotImplementedException();
        }
    }
}
