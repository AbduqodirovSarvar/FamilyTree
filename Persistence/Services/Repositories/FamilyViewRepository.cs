using System.Text;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Services.Repositories.Common;

namespace Persistence.Services.Repositories
{
    public class FamilyViewRepository(AppDbContext context, IRedisService redisService)
        : GenericRepository<FamilyView>(context, redisService), IFamilyViewRepository
    {
        // Stay well below PostgreSQL's 65535 parameter cap; 5 params per row
        // * 1000 rows = 5000 params per chunk.
        private const int ChunkSize = 1000;

        private const string InsertSqlPrefix =
            "INSERT INTO \"FamilyViews\" (\"Id\",\"FamilyId\",\"IpAddress\",\"ViewDate\",\"CreatedAt\") VALUES ";
        private const string InsertSqlSuffix =
            " ON CONFLICT (\"FamilyId\",\"IpAddress\",\"ViewDate\") DO NOTHING";

        public async Task<List<(DateOnly Date, int Count)>> GetDailyCountsAsync(
            Guid familyId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            // Group on the server — we only ship aggregated rows back.
            var rows = await _dbSet.AsNoTracking()
                .Where(v => v.FamilyId == familyId
                         && v.ViewDate >= from
                         && v.ViewDate <= to)
                .GroupBy(v => v.ViewDate)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return rows.Select(r => (r.Date, r.Count)).ToList();
        }

        public async Task<int> BulkInsertIfMissingAsync(
            IReadOnlyList<FamilyViewBufferEntry> entries,
            CancellationToken cancellationToken = default)
        {
            if (entries.Count == 0) return 0;

            int written = 0;
            for (int i = 0; i < entries.Count; i += ChunkSize)
            {
                int end = Math.Min(i + ChunkSize, entries.Count);
                written += await InsertChunkAsync(entries, i, end, cancellationToken);
            }
            return written;
        }

        private async Task<int> InsertChunkAsync(
            IReadOnlyList<FamilyViewBufferEntry> entries,
            int start,
            int end,
            CancellationToken cancellationToken)
        {
            int rowCount = end - start;

            // Build a parametrised multi-row INSERT. Parameter ordinals match
            // the EF.Database parameter convention (@p0, @p1, ...).
            var sb = new StringBuilder(InsertSqlPrefix.Length + rowCount * 32);
            sb.Append(InsertSqlPrefix);

            var parameters = new object[rowCount * 5];
            var now = DateTime.UtcNow;

            for (int i = 0; i < rowCount; i++)
            {
                if (i > 0) sb.Append(',');
                int p = i * 5;
                sb.Append('(')
                  .Append('{').Append(p).Append('}').Append(',')
                  .Append('{').Append(p + 1).Append('}').Append(',')
                  .Append('{').Append(p + 2).Append('}').Append(',')
                  .Append('{').Append(p + 3).Append('}').Append(',')
                  .Append('{').Append(p + 4).Append('}')
                  .Append(')');

                var entry = entries[start + i];
                parameters[p]     = Guid.NewGuid();
                parameters[p + 1] = entry.FamilyId;
                parameters[p + 2] = entry.IpAddress;
                parameters[p + 3] = entry.ViewDate;
                parameters[p + 4] = now;
            }

            sb.Append(InsertSqlSuffix);

            return await _context.Database.ExecuteSqlRawAsync(
                sb.ToString(), parameters, cancellationToken);
        }
    }
}
