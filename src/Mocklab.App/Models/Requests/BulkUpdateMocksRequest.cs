namespace Mocklab.App.Models.Requests;

/// <summary>
/// Request payload for bulk updating mock collection/folder
/// </summary>
public class BulkUpdateMocksRequest
{
    public List<int> MockIds { get; set; } = new();
    public int? CollectionId { get; set; }
    public int? FolderId { get; set; }
}
