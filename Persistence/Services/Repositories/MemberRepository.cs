using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Persistence.Data;
using Persistence.Services.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services.Repositories
{
    public class MemberRepository(AppDbContext context, IRedisService redisService) 
        : GenericRepository<Member>(context, redisService), IMemberRepository
    {
    }
}
