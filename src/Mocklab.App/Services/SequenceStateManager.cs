using System.Collections.Concurrent;

namespace Mocklab.App.Services;

/// <summary>
/// In-memory implementation of sequence state management.
/// Uses ConcurrentDictionary for thread-safe counter tracking.
/// State is lost on application restart by design — sequences reset on deploy.
/// </summary>
public class SequenceStateManager : ISequenceStateManager
{
    private readonly ConcurrentDictionary<int, int> _counters = new();

    public int GetNextIndex(int mockId, int totalItems)
    {
        if (totalItems <= 0) return 0;

        // Atomically get the current value and increment
        var currentIndex = _counters.AddOrUpdate(
            mockId,
            0,                          // If key doesn't exist, start at 0
            (_, current) => current     // If key exists, return current value (we'll increment after)
        );

        // Advance the counter for next call (with wrap-around)
        _counters[mockId] = (currentIndex + 1) % totalItems;

        return currentIndex;
    }

    public void Reset(int mockId)
    {
        _counters.TryRemove(mockId, out _);
    }

    public void ResetAll()
    {
        _counters.Clear();
    }
}
