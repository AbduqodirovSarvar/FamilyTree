using Application.Common.Interfaces.Repositories.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.Repositories
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
    }
}
