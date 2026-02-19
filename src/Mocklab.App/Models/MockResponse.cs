namespace Mocklab.App.Models;

public class MockResponse
{
    public int Id { get; set; }
    
    /// <summary>
    /// HTTP Method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Route pattern (e.g.: /api/users/{id})
    /// </summary>
    public string Route { get; set; } = string.Empty;
    
    /// <summary>
    /// Query string (optional, e.g.: ?page=1&size=10)
    /// </summary>
    public string? QueryString { get; set; }
    
    /// <summary>
    /// Request body (optional, for POST/PUT)
    /// </summary>
    public string? RequestBody { get; set; }
    
    /// <summary>
    /// HTTP Status Code (200, 404, 500, etc.)
    /// </summary>
    public int StatusCode { get; set; } = 200;
    
    /// <summary>
    /// Response body to return (JSON, XML, text, etc.)
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;
    
    /// <summary>
    /// Response Content-Type (e.g.: application/json)
    /// </summary>
    public string ContentType { get; set; } = "application/json";
    
    /// <summary>
    /// Mock record description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Optional delay in milliseconds before returning the response.
    /// When set and greater than 0, the server will wait this many milliseconds before responding.
    /// </summary>
    public int? DelayMs { get; set; }

    /// <summary>
    /// Optional collection this mock belongs to
    /// </summary>
    public int? CollectionId { get; set; }

    /// <summary>
    /// When true, this mock cycles through sequence items instead of returning a static response
    /// </summary>
    public bool IsSequential { get; set; } = false;

    /// <summary>
    /// Active/Inactive status
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Conditional response rules for this mock
    /// </summary>
    public ICollection<MockResponseRule> Rules { get; set; } = new List<MockResponseRule>();

    /// <summary>
    /// Sequence items for sequential mock responses
    /// </summary>
    public ICollection<MockResponseSequenceItem> SequenceItems { get; set; } = new List<MockResponseSequenceItem>();
}
