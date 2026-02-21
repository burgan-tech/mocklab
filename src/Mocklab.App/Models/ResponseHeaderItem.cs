using System.Text.Json.Serialization;

namespace Mocklab.App.Models;

/// <summary>
/// Key-value pair for response headers in API contract (serializes as "key" / "value").
/// </summary>
public class ResponseHeaderItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
