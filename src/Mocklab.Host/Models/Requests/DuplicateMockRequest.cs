namespace Mocklab.Host.Models.Requests;

/// <summary>
/// Optional request payload for duplicating a mock to a different collection/folder.
/// When null/empty, the mock is duplicated in its original location.
/// </summary>
public class DuplicateMockRequest
{
    public int? CollectionId { get; set; }
    public int? FolderId { get; set; }
}
