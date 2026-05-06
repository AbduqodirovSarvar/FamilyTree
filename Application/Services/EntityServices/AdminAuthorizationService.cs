using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;

namespace Application.Services.EntityServices
{
    /// <summary>
    /// Resolves "is admin" by reading the role record from the repository.
    /// Constants and lookup are kept here (not in the controllers) so
    /// renaming the canonical admin role name is a single-file change.
    /// </summary>
    internal sealed class AdminAuthorizationService : IAdminAuthorizationService
    {
        private const string AdminRoleDesignedName = "ADMIN";

        private readonly ICurrentUserService _currentUser;
        private readonly IUserRoleRepository _roles;

        public AdminAuthorizationService(
            ICurrentUserService currentUser,
            IUserRoleRepository roles)
        {
            _currentUser = currentUser;
            _roles = roles;
        }

        public async Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default)
        {
            var user = await _currentUser.GetCurrentUserAsync(cancellationToken);
            if (user is null)
                return false;

            var role = await _roles.GetByIdAsync(user.RoleId, cancellationToken);
            return string.Equals(
                role?.DesignedName,
                AdminRoleDesignedName,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
