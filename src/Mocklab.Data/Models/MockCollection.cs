namespace Mocklab.Host.Models;

/// <summary>
/// Represents a group/collection of related mock responses
/// </summary>
public class MockCollection
{
    public int Id { get; set; }

    /// <summary>
    /// Collection name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Collection description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display color for UI (hex code, e.g., #6366f1)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property for related mock responses
    /// </summary>
    public ICollection<MockResponse> MockResponses { get; set; } = new List<MockResponse>();

    /// <summary>
    /// Navigation property for folders in this collection
    /// </summary>
    public ICollection<MockFolder> Folders { get; set; } = new List<MockFolder>();

    /// <summary>
    /// Navigation property for data buckets in this collection
    /// </summary>
    public ICollection<DataBucket> DataBuckets { get; set; } = new List<DataBucket>();
}
