using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.ViewModels;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    internal class UserService(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        IMapper mapper)
        : GenericEntityService<User, CreateUserDto, UpdateUserDto, UserViewModel>(userRepository, permissionService, mapper), IUserService
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<User?> GetCurrentUser(CancellationToken cancellationToken = default)
        {
            return await _currentUserService.GetCurrentUserAsync(cancellationToken);
        }
    }
}
