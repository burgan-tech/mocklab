using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mocklab.Host.Models;

/// <summary>
/// Defines a conditional response rule for a mock response.
/// When the condition matches, this rule's response overrides the default mock response.
/// </summary>
public class MockResponseRule
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the parent MockResponse
    /// </summary>
    public int MockResponseId { get; set; }

    /// <summary>
    /// The field to evaluate: "header.Authorization", "query.page", "body.amount", "method", "path"
    /// </summary>
    public string ConditionField { get; set; } = string.Empty;

    /// <summary>
    /// The comparison operator: equals, contains, startsWith, endsWith, regex, exists, notExists, greaterThan, lessThan
    /// </summary>
    public string ConditionOperator { get; set; } = "equals";

    /// <summary>
    /// The value to compare against (not needed for exists/notExists)
    /// </summary>
    public string? ConditionValue { get; set; }

    /// <summary>
    /// HTTP status code to return when this rule matches
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Response body to return when this rule matches
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Content type for the response when this rule matches
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Priority order (lower number = higher priority)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Response headers when this rule matches. Not stored on this entity; populated from KeyValueEntry (OwnerType = "MockResponseRule") in API/runtime.
    /// </summary>
    [NotMapped]
    public List<ResponseHeaderItem>? ResponseHeaders { get; set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    [JsonIgnore]
    public MockResponse? MockResponse { get; set; }
}
