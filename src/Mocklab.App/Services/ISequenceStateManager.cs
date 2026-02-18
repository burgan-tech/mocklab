namespace Mocklab.App.Services;

/// <summary>
/// Manages the state of sequential mock responses.
/// Tracks which sequence step should be returned next for each mock.
/// Uses an in-memory counter (ConcurrentDictionary) — state is lost on app restart.
/// </summary>
public interface ISequenceStateManager
{
    /// <summary>
    /// Gets the next sequence index for the given mock and advances the counter.
    /// Wraps around to 0 when reaching totalItems.
    /// </summary>
    /// <param name="mockId">The mock response ID</param>
    /// <param name="totalItems">Total number of sequence items</param>
    /// <returns>The current index (0-based) before advancing</returns>
    int GetNextIndex(int mockId, int totalItems);

    /// <summary>
    /// Resets the sequence counter for a specific mock back to 0.
    /// </summary>
    void Reset(int mockId);

    /// <summary>
    /// Resets all sequence counters.
    /// </summary>
    void ResetAll();
}
