namespace Mocklab.Host.Models;

/// <summary>
/// Represents a folder (area) within a collection for grouping mock responses
/// </summary>
public class MockFolder
{
    public int Id { get; set; }

    /// <summary>
    /// Parent collection
    /// </summary>
    public int CollectionId { get; set; }

    /// <summary>
    /// Optional parent folder for nesting (e.g. Reports/Summary)
    /// </summary>
    public int? ParentFolderId { get; set; }

    /// <summary>
    /// Folder name
    /// </summary>
    public string Name { get; set; } = string.Empty;

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
    /// Navigation property for parent collection
    /// </summary>
    public MockCollection? Collection { get; set; }

    /// <summary>
    /// Navigation property for parent folder (when nested)
    /// </summary>
    public MockFolder? ParentFolder { get; set; }

    /// <summary>
    /// Child folders (when nested)
    /// </summary>
    public ICollection<MockFolder> Children { get; set; } = new List<MockFolder>();

    /// <summary>
    /// Mock responses in this folder
    /// </summary>
    public ICollection<MockResponse> MockResponses { get; set; } = new List<MockResponse>();
}
