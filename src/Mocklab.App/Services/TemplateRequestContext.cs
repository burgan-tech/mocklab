namespace Mocklab.App.Services;

/// <summary>
/// Context passed to the template processor for request-specific and collection-specific data.
/// </summary>
public class TemplateRequestContext
{
    /// <summary>
    /// Route parameters extracted from the matched route template (e.g. id from /api/users/{id}).
    /// </summary>
    public IReadOnlyDictionary<string, string>? RouteParams { get; set; }

    /// <summary>
    /// Pre-read request body when already consumed (e.g. by CatchAllController) to avoid double read.
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Named data bucket objects (name -> object or list) for the mock's collection. Populated when Data Buckets are loaded.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Buckets { get; set; }
}
