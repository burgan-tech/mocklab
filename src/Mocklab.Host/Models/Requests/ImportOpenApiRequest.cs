namespace Mocklab.Host.Models.Requests;

/// <summary>
/// Request payload for the OpenAPI import endpoint.
/// </summary>
public class ImportOpenApiRequest
{
    public string OpenApiJson { get; set; } = string.Empty;
}
