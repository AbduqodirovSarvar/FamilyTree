using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Models;
using Application.Common.Models.Dtos.User;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices
{
    public interface IUserService
        : IGenericEntityService<User, CreateUserDto, UpdateUserDto, UserViewModel>
    {
        Task<User?> GetCurrentUser(CancellationToken cancellationToken = default);
    }
}
