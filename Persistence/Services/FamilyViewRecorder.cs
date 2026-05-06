using System.Collections.Concurrent;
using Application.Common.Interfaces;

namespace Persistence.Services
{
    /// <summary>
    /// Singleton in-memory dedup + buffer. Two dictionaries, both lock-free:
    /// <list type="bullet">
    ///   <item><c>_seen</c> — the master "we already counted this triple"
    ///   set; lives across flushes so refresh storms never produce
    ///   duplicates within the same process lifetime.</item>
    ///   <item><c>_pending</c> — only the triples added since the last
    ///   <see cref="DrainPending"/>. Drains to zero on each flush.</item>
    /// </list>
    /// </summary>
    internal sealed class FamilyViewRecorder : IFamilyViewRecorder
    {
        private readonly ConcurrentDictionary<FamilyViewBufferEntry, byte> _seen = new();
        private readonly ConcurrentDictionary<FamilyViewBufferEntry, byte> _pending = new();

        public void Record(Guid familyId, string ipAddress)
        {
            if (familyId == Guid.Empty) return;
            if (string.IsNullOrWhiteSpace(ipAddress)) return;

            var entry = new FamilyViewBufferEntry(
                familyId,
                ipAddress,
                DateOnly.FromDateTime(DateTime.UtcNow));

            // TryAdd returns false if the key was already present — perfect
            // dedup short-circuit, no lock.
            if (_seen.TryAdd(entry, 0))
            {
                _pending.TryAdd(entry, 0);
            }
        }

        public IReadOnlyList<FamilyViewBufferEntry> DrainPending()
        {
            // Snapshot the current keys, then remove each. New writes during
            // the snapshot land safely in the next pass.
            var keys = _pending.Keys.ToArray();
            foreach (var k in keys)
            {
                _pending.TryRemove(k, out _);
            }
            return keys;
        }

        public void EvictOlderThan(DateOnly cutoff)
        {
            foreach (var key in _seen.Keys)
            {
                if (key.ViewDate < cutoff)
                {
                    _seen.TryRemove(key, out _);
                }
            }
        }
    }
}
