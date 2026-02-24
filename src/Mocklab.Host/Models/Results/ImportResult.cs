namespace Mocklab.Host.Models.Results;

/// <summary>
/// Unified result returned by import service operations.
/// </summary>
public class ImportResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The single mock created (cURL import).
    /// </summary>
    public MockResponse? Mock { get; init; }

    /// <summary>
    /// The collection of mocks created (OpenAPI import).
    /// </summary>
    public IReadOnlyList<MockResponse>? Mocks { get; init; }

    public int ImportedCount { get; init; }

    // ── Factory helpers ──────────────────────────────────────────────

    public static ImportResult Fail(string error) =>
        new() { Success = false, ErrorMessage = error };

    public static ImportResult SingleMock(MockResponse mock) =>
        new() { Success = true, Mock = mock, ImportedCount = 1 };

    public static ImportResult MultipleMocks(IReadOnlyList<MockResponse> mocks) =>
        new() { Success = true, Mocks = mocks, ImportedCount = mocks.Count };
}
