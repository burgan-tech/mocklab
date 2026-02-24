namespace Mocklab.Host.Models;

/// <summary>
/// Represents a logged incoming request to the mock server
/// </summary>
public class RequestLog
{
    public int Id { get; set; }

    /// <summary>
    /// HTTP Method of the incoming request
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Request route/path
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Query string from the request
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// Request body content
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Request headers serialized as JSON
    /// </summary>
    public string? RequestHeaders { get; set; }

    /// <summary>
    /// ID of the matched MockResponse, null if no match found
    /// </summary>
    public int? MatchedMockId { get; set; }

    /// <summary>
    /// Description of the matched mock (for quick reference)
    /// </summary>
    public string? MatchedMockDescription { get; set; }

    /// <summary>
    /// HTTP status code returned in the response
    /// </summary>
    public int ResponseStatusCode { get; set; }

    /// <summary>
    /// Whether the request was matched to a mock
    /// </summary>
    public bool IsMatched { get; set; }

    /// <summary>
    /// Timestamp of the request
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Response time in milliseconds (including any configured delay)
    /// </summary>
    public long? ResponseTimeMs { get; set; }
}
