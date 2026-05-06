using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// One row per anonymous public-tree fetch. We dedupe at the row level
    /// (one row per IP per family per UTC day) so refreshes don't inflate
    /// counts. Aggregation into daily totals happens at query time.
    /// </summary>
    public class FamilyView : BaseEntity
    {
        public Guid FamilyId { get; set; }
        public Family? Family { get; set; }

        /// <summary>UTC date only — the day on which the visit was recorded.</summary>
        public DateOnly ViewDate { get; set; }

        /// <summary>Source IP. Capped at 64 chars to fit IPv6 + zone id.</summary>
        public string IpAddress { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
