namespace Mocklab.Host.Models;

/// <summary>
/// A named, structured data set attached to a collection for use in Scriban templates.
/// </summary>
public class DataBucket
{
    public int Id { get; set; }

    /// <summary>
    /// Collection this bucket belongs to.
    /// </summary>
    public int CollectionId { get; set; }

    /// <summary>
    /// Bucket name (e.g. "persons", "products"). Used in templates as the variable name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON data: array of objects or a single object. Stored as string.
    /// </summary>
    public string Data { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public MockCollection? Collection { get; set; }
}
