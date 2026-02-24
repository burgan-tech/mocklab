namespace Mocklab.App.Models;

/// <summary>
/// Generic key-value storage for entities. Used for response headers per rule (OwnerType = "MockResponseRule");
/// can be reused for other key-value needs (e.g. default response headers, sequence item metadata).
/// </summary>
public class KeyValueEntry
{
    public int Id { get; set; }

    /// <summary>
    /// Discriminator for the owning entity, e.g. "MockResponseRule", "MockResponse", "MockResponseSequenceItem".
    /// </summary>
    public string OwnerType { get; set; } = string.Empty;

    /// <summary>
    /// Primary key of the owning entity.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// Key name (e.g. "X-Request-Id", "Cache-Control").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
