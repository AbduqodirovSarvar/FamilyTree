using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Services.EntityServices.Common;
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
        IPermissionService permissionService)
        : GenericEntityService<User>(userRepository, permissionService), IUserService
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<User?> GetCurrentUser(CancellationToken cancellationToken = default)
        {
            if (_currentUserService.UserId != null)
            {
                return await _repository.GetAsync(x => x.Id == _currentUserService.UserId, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(_currentUserService.Email))
            {
                return await _repository.GetAsync(x => x.Email == _currentUserService.Email, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(_currentUserService.Username))
            {
                return await _repository.GetAsync(x => x.UserName == _currentUserService.Username, cancellationToken);
            }

            return null;
        }
    }
}
