using Application.Common.Interfaces.Repositories.Common;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories
{
    public interface IFamilyViewRepository : IGenericRepository<FamilyView>
    {
        /// <summary>
        /// Daily counts (UTC) for the given family between [from, to].
        /// Returns one entry per day that has at least one view; days with
        /// no views are absent — callers fill zeros.
        /// </summary>
        Task<List<(DateOnly Date, int Count)>> GetDailyCountsAsync(
            Guid familyId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Idempotent multi-row INSERT. Conflicts on the (FamilyId,
        /// IpAddress, ViewDate) unique index are silently skipped via
        /// <c>ON CONFLICT DO NOTHING</c>, so the caller doesn't have to
        /// dedup against state lost on a process restart.
        /// </summary>
        Task<int> BulkInsertIfMissingAsync(
            IReadOnlyList<Application.Common.Interfaces.FamilyViewBufferEntry> entries,
            CancellationToken cancellationToken = default);
    }
}
