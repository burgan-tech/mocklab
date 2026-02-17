using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mocklab.App.Data;
using Mocklab.App.Extensions;

namespace Mocklab.App.Controllers;

[ApiController]
public class CatchAllController : ControllerBase
{
    private readonly ILogger<CatchAllController> _logger;
    private readonly MocklabDbContext _dbContext;
    private readonly MocklabOptions _options;

    public CatchAllController(
        ILogger<CatchAllController> logger, 
        MocklabDbContext dbContext,
        IOptions<MocklabOptions> options)
    {
        _logger = logger;
        _dbContext = dbContext;
        _options = options.Value;
    }

    // Catches all routes except _admin - regardless of HTTP method
    [Route("{**catchAll}")]
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [HttpHead]
    [HttpOptions]
    public async Task<IActionResult> HandleAllRequests(string? catchAll)
    {
        var fullPath = Request.Path.Value ?? string.Empty;
        var requestMethod = Request.Method;

        // Strip route prefix to get the actual path for matching
        var prefix = string.IsNullOrEmpty(_options.RoutePrefix) 
            ? "" 
            : "/" + _options.RoutePrefix.Trim('/');
        var requestPath = !string.IsNullOrEmpty(prefix) && fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? fullPath[prefix.Length..]
            : fullPath;

        // Skip admin and framework routes - let them be handled by their own handlers
        if (requestPath.StartsWith("/_admin", StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var queryString = Request.QueryString.Value ?? string.Empty;

        // Read request body (if exists)
        string? requestBody = null;
        if (Request.ContentLength > 0 && (requestMethod == "POST" || requestMethod == "PUT" || requestMethod == "PATCH"))
        {
            using var reader = new StreamReader(Request.Body);
            requestBody = await reader.ReadToEndAsync();
        }

        _logger.LogInformation(
            "Request: {Method} {Path}{QueryString}",
            requestMethod,
            requestPath,
            queryString
        );

        return await FindPreparedMockResponse(requestMethod, requestPath, queryString, requestBody);
    }
    private async Task<IActionResult> FindPreparedMockResponse(string requestMethod, string requestPath, string queryString, string? requestBody)
    {
        // Find matching mock response from database
        var mockResponse = await FindMatchingMockResponse(requestMethod, requestPath, queryString, requestBody);

        if (mockResponse != null)
        {
            _logger.LogInformation(
                "Mock response found: Id={Id}, Description={Description}",
                mockResponse.Id,
                mockResponse.Description
            );

            // Set Content-Type header
            Response.ContentType = mockResponse.ContentType;

            object? response = null;
            try
            {
                if (mockResponse.ContentType == "application/json")
                {
                    response = JsonSerializer.Deserialize<object>(mockResponse.ResponseBody);
                }
                else
                {
                    response = mockResponse.ResponseBody;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing mock response: {ResponseBody}", mockResponse.ResponseBody);
            }
            return StatusCode(mockResponse.StatusCode, response);
        }

        // No match found
        _logger.LogWarning("Mock response not found: {Method} {Path}", requestMethod, requestPath);

        return NotFound(new
        {
            Error = "Mock response not found",
            Request = new
            {
                Method = requestMethod,
                Path = requestPath,
                QueryString = queryString,
                Body = requestBody
            },
            Message = "No mock response found for this request. Please add a mock response to the database.",
            Timestamp = DateTime.UtcNow
        });
    }

    private async Task<Models.MockResponse?> FindMatchingMockResponse(
        string method,
        string path,
        string queryString,
        string? requestBody)
    {
        var query = BuildMockQuery(method, queryString, requestBody);
        //TODO: Consider use firstordefault instead of tolistasync, or return all matches instead of just the first one
        // First, try exact route match
        var exactMatch = await query.Where(m => m.Route == path).ToListAsync();

        // If no exact match, fallback to contains
        var mockResponses = exactMatch.Count > 0
            ? exactMatch
            : await query.Where(m => path.Contains(m.Route)).ToListAsync();

        foreach (var mock in mockResponses)
        {
            // Check query string (if exists)
            return mock;
        }

        return null;
    }
    private IQueryable<Models.MockResponse> BuildMockQuery(string method, string? queryString, string? requestBody)
    {
        var query = _dbContext.MockResponses.AsQueryable().Where(m => m.IsActive && m.HttpMethod == method);

        if (!string.IsNullOrEmpty(queryString))
        {
            query = query.Where(m => m.QueryString == queryString);
        }

        if (!string.IsNullOrEmpty(requestBody))
        {
            query = query.Where(m => m.RequestBody == requestBody);
        }

        return query;
    }
}
