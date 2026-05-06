using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Single check for "is the current user an ADMIN", deduplicating the
    /// claim-vs-DB-lookup pattern that <c>FamilyService</c> and
    /// <c>MemberService</c> currently each carry as private helpers. Used
    /// by the admin-only controllers so the role gate is enforced
    /// consistently and isn't easily bypassed by a typo.
    ///
    /// <para><b>Why not <c>[Authorize(Roles = "ADMIN")]</c>?</b> The JWT
    /// stores <c>ClaimTypes.Role</c> as the role's <c>RoleId</c> GUID,
    /// not its <c>DesignedName</c>. Resolving "ADMIN" requires hitting
    /// the user-roles repository, which the framework attribute can't
    /// do.</para>
    /// </summary>
    public interface IAdminAuthorizationService
    {
        Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default);
    }
}
