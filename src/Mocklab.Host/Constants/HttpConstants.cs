namespace Mocklab.Host.Constants;

/// <summary>
/// Shared HTTP-related constant values used across the application.
/// </summary>
public static class HttpConstants
{
    public const string DefaultContentType = "application/json";
    public const int DefaultStatusCode = 200;
    public const string DefaultResponseBody = "{}";

    public const string MethodGet = "GET";
    public const string MethodPost = "POST";
    public const string MethodPut = "PUT";
    public const string MethodDelete = "DELETE";
    public const string MethodPatch = "PATCH";

    /// <summary>
    /// Standard HTTP methods recognised when parsing OpenAPI specifications.
    /// </summary>
    public static readonly string[] SupportedHttpMethods =
        ["get", "post", "put", "delete", "patch", "head", "options"];
}
