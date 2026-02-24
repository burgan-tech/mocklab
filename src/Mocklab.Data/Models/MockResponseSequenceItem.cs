using System.Text.Json.Serialization;

namespace Mocklab.App.Models;

/// <summary>
/// Represents a single step in a sequential mock response.
/// When a mock is marked as sequential, responses cycle through these items in order.
/// </summary>
public class MockResponseSequenceItem
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the parent MockResponse
    /// </summary>
    public int MockResponseId { get; set; }

    /// <summary>
    /// Order in the sequence (0-based)
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// HTTP status code for this sequence step
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Response body for this sequence step
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Content type for this sequence step
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Optional delay in milliseconds for this specific step
    /// </summary>
    public int? DelayMs { get; set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    [JsonIgnore]
    public MockResponse? MockResponse { get; set; }
}
