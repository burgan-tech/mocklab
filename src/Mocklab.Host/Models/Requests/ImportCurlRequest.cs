namespace Mocklab.Host.Models.Requests;

/// <summary>
/// Request payload for the cURL import endpoint.
/// </summary>
public class ImportCurlRequest
{
    public string Curl { get; set; } = string.Empty;
}
