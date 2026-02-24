using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mocklab.Host.Constants;
using Mocklab.Host.Data;
using Mocklab.Host.Models;
using Mocklab.Host.Models.Results;

namespace Mocklab.Host.Services;

public class MockImportService(
    MocklabDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    ILogger<MockImportService> logger) : IMockImportService
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<MockImportService> _logger = logger;

    // ═══════════════════════════════════════════════════════════════════
    //  cURL Import
    // ═══════════════════════════════════════════════════════════════════

    public async Task<ImportResult> ImportFromCurlAsync(string curlCommand)
    {
        // 1. Parse
        CurlParseResult parsed;
        try
        {
            parsed = CurlParser.Parse(curlCommand);
        }
        catch (ArgumentException ex)
        {
            return ImportResult.Fail($"Failed to parse cURL: {ex.Message}");
        }

        // 2. Execute the real HTTP request
        HttpResponseMessage response;
        try
        {
            var request = BuildHttpRequest(parsed);
            var client = _httpClientFactory.CreateClient();
            response = await client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            return ImportResult.Fail($"Failed to execute request: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ImportResult.Fail("Request timed out.");
        }

        // 3. Capture response and persist
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseContentType = response.Content.Headers.ContentType?.MediaType
                                  ?? HttpConstants.DefaultContentType;

        var uri = new Uri(parsed.Url);

        var method = (parsed.Method ?? "GET").Trim().ToUpperInvariant();
        var route = (uri.AbsolutePath ?? "/").Trim();
        var exists = await _dbContext.MockResponses.AnyAsync(m =>
            m.CollectionId == null &&
            (m.HttpMethod ?? "").Trim().ToUpperInvariant() == method &&
            (m.Route ?? "").Trim() == route);
        if (exists)
            return ImportResult.Fail("A mock with the same HTTP method and route already exists (no collection).");

        var mock = new MockResponse
        {
            HttpMethod = method,
            Route = route,
            QueryString = string.IsNullOrEmpty(uri.Query) ? null : uri.Query,
            RequestBody = parsed.Body,
            StatusCode = (int)response.StatusCode,
            ResponseBody = responseBody,
            ContentType = responseContentType,
            Description = $"Imported from cURL: {parsed.Method} {uri.Host}{uri.AbsolutePath}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MockResponses.Add(mock);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Mock imported from cURL: Id={Id}, Method={Method}, Route={Route}, StatusCode={StatusCode}",
            mock.Id, mock.HttpMethod, mock.Route, mock.StatusCode);

        return ImportResult.SingleMock(mock);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  OpenAPI Import
    // ═══════════════════════════════════════════════════════════════════

    public async Task<ImportResult> ImportFromOpenApiAsync(string openApiJson)
    {
        // 1. Parse JSON
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(openApiJson);
        }
        catch (JsonException ex)
        {
            return ImportResult.Fail($"Invalid JSON: {ex.Message}");
        }

        if (!doc.RootElement.TryGetProperty("paths", out var paths))
        {
            return ImportResult.Fail("No 'paths' found in OpenAPI specification.");
        }

        // 2. Build mock list
        var mocks = new List<MockResponse>();

        foreach (var pathItem in paths.EnumerateObject())
        {
            foreach (var methodItem in pathItem.Value.EnumerateObject())
            {
                var method = methodItem.Name.ToLowerInvariant();
                if (!HttpConstants.SupportedHttpMethods.Contains(method))
                    continue;

                var mock = BuildMockFromOperation(pathItem.Name, method, methodItem.Value);
                mocks.Add(mock);
            }
        }

        if (mocks.Count == 0)
        {
            return ImportResult.Fail("No API endpoints found in the OpenAPI specification.");
        }

        var existingRows = await _dbContext.MockResponses
            .Where(m => m.CollectionId == null)
            .Select(m => new { m.HttpMethod, m.Route })
            .ToListAsync();
        var existingKeys = existingRows
            .Select(x => ((x.HttpMethod ?? "").Trim().ToUpperInvariant(), (x.Route ?? "").Trim()))
            .ToHashSet();
        var seen = new HashSet<(string Method, string Route)>();
        var toAdd = new List<MockResponse>();
        foreach (var mock in mocks)
        {
            var method = (mock.HttpMethod ?? "").Trim().ToUpperInvariant();
            var route = (mock.Route ?? "").Trim();
            if (!seen.Add((method, route)) || existingKeys.Contains((method, route)))
                continue;
            mock.HttpMethod = method;
            mock.Route = route;
            toAdd.Add(mock);
        }

        _dbContext.MockResponses.AddRange(toAdd);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Imported {Count} mocks from OpenAPI specification ({Skipped} duplicates skipped).", toAdd.Count, mocks.Count - toAdd.Count);

        return ImportResult.MultipleMocks(toAdd);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Private helpers
    // ═══════════════════════════════════════════════════════════════════

    private static HttpRequestMessage BuildHttpRequest(CurlParseResult parsed)
    {
        var request = new HttpRequestMessage(new HttpMethod(parsed.Method), parsed.Url);

        foreach (var header in parsed.Headers)
        {
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;

            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (!string.IsNullOrEmpty(parsed.Body))
        {
            var contentType = parsed.Headers
                .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                .Value ?? HttpConstants.DefaultContentType;

            request.Content = new StringContent(parsed.Body, System.Text.Encoding.UTF8, contentType);
        }

        return request;
    }

    private static MockResponse BuildMockFromOperation(string route, string method, JsonElement operation)
    {
        var description = GetDescription(operation);
        var (statusCode, responseBody, contentType) = ExtractResponseDetails(operation);

        return new MockResponse
        {
            HttpMethod = method.ToUpperInvariant(),
            Route = route,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ContentType = contentType,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string? GetDescription(JsonElement operation)
    {
        if (operation.TryGetProperty("summary", out var summary))
            return summary.GetString();

        if (operation.TryGetProperty("operationId", out var operationId))
            return operationId.GetString();

        return null;
    }

    private static (int statusCode, string responseBody, string contentType) ExtractResponseDetails(
        JsonElement operation)
    {
        var statusCode = HttpConstants.DefaultStatusCode;
        var responseBody = HttpConstants.DefaultResponseBody;
        var contentType = HttpConstants.DefaultContentType;

        if (!operation.TryGetProperty("responses", out var responses))
            return (statusCode, responseBody, contentType);

        foreach (var resp in responses.EnumerateObject())
        {
            if (!int.TryParse(resp.Name, out var code) || code < 200 || code >= 300)
                continue;

            statusCode = code;

            if (!resp.Value.TryGetProperty("content", out var content))
                break;

            foreach (var mediaType in content.EnumerateObject())
            {
                contentType = mediaType.Name;
                responseBody = ExtractExampleBody(mediaType.Value) ?? responseBody;
                break; // use first media type
            }

            break; // use first success response
        }

        return (statusCode, responseBody, contentType);
    }

    private static string? ExtractExampleBody(JsonElement mediaType)
    {
        if (mediaType.TryGetProperty("example", out var example))
            return example.ToString();

        if (!mediaType.TryGetProperty("examples", out var examples))
            return null;

        foreach (var ex in examples.EnumerateObject())
        {
            if (ex.Value.TryGetProperty("value", out var val))
                return val.ToString();

            break; // take first
        }

        return null;
    }
}
