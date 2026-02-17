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
}
