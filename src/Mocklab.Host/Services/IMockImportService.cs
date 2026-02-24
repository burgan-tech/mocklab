using Mocklab.Host.Models.Results;

namespace Mocklab.Host.Services;

/// <summary>
/// Handles importing mock responses from external sources (cURL commands, OpenAPI specs).
/// </summary>
public interface IMockImportService
{
    /// <summary>
    /// Parses and executes a cURL command, then persists the captured response as a mock.
    /// </summary>
    Task<ImportResult> ImportFromCurlAsync(string curlCommand);

    /// <summary>
    /// Parses an OpenAPI JSON specification and creates a mock for each path + method pair.
    /// </summary>
    Task<ImportResult> ImportFromOpenApiAsync(string openApiJson);
}
