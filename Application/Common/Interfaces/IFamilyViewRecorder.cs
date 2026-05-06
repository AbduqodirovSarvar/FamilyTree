namespace Application.Common.Interfaces
{
    /// <summary>
    /// Cheap, lock-free buffer for "this IP saw this family today". The
    /// public-tree endpoint is hit on every page-load and SSR; persisting
    /// each one inline would put the database under load it doesn't need to
    /// carry. Instead, callers pin the (familyId, ip, today) triple in
    /// memory; <see cref="BackgroundServices.FamilyViewFlushService"/>
    /// drains the buffer to the DB in a single batched INSERT every few
    /// minutes.
    ///
    /// <para><b>Trade-off.</b> A crash between two flushes loses up to
    /// ~5min of unique-visitor records. Acceptable — view counts are an
    /// approximate analytic, not a billing metric.</para>
    /// </summary>
    public interface IFamilyViewRecorder
    {
        /// <summary>
        /// Notes that <paramref name="ipAddress"/> visited
        /// <paramref name="familyId"/> at the call site's UTC date. No-op
        /// when the same triple has already been seen today (within this
        /// process). Returns immediately — never touches the DB.
        /// </summary>
        void Record(Guid familyId, string ipAddress);

        /// <summary>
        /// Returns and clears the set of (familyId, ip, date) triples that
        /// have been observed but not yet flushed. The seen-set retains
        /// today's entries so a refreshed flush still dedupes correctly.
        /// </summary>
        IReadOnlyList<FamilyViewBufferEntry> DrainPending();

        /// <summary>
        /// Drops seen-keys older than <paramref name="cutoff"/> from memory.
        /// Called by the flush worker after each pass so a long-running
        /// process doesn't accumulate days' worth of triples.
        /// </summary>
        void EvictOlderThan(DateOnly cutoff);
    }

    public readonly record struct FamilyViewBufferEntry(
        Guid FamilyId,
        string IpAddress,
        DateOnly ViewDate);
}
