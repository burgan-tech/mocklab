using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mocklab.App.Data;
using Mocklab.App.Extensions;
using Mocklab.App.Models;
using Mocklab.App.Services;

namespace Mocklab.App.Controllers;

[ApiController]
public class CatchAllController(
    ILogger<CatchAllController> logger,
    MocklabDbContext dbContext,
    IOptions<MocklabOptions> options,
    ITemplateProcessor templateProcessor,
    IRuleEvaluator ruleEvaluator,
    ISequenceStateManager sequenceStateManager) : ControllerBase
{
    private readonly ILogger<CatchAllController> _logger = logger;
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly MocklabOptions _options = options.Value;
    private readonly ITemplateProcessor _templateProcessor = templateProcessor;
    private readonly IRuleEvaluator _ruleEvaluator = ruleEvaluator;
    private readonly ISequenceStateManager _sequenceStateManager = sequenceStateManager;

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

        // Start timing the request
        var stopwatch = Stopwatch.StartNew();

        // Find matching mock response from database (with Rules and SequenceItems)
        var mockResponse = await FindMatchingMockResponse(requestMethod, requestPath, queryString, requestBody);

        IActionResult result;
        int responseStatusCode;

        if (mockResponse != null)
        {
            _logger.LogInformation(
                "Mock response found: Id={Id}, Description={Description}",
                mockResponse.Id,
                mockResponse.Description
            );

            // Determine response values — defaults from mock
            var responseBody = mockResponse.ResponseBody;
            var contentType = mockResponse.ContentType;
            var statusCode = mockResponse.StatusCode;
            var delayMs = mockResponse.DelayMs;

            // Step 1: Check for sequential mode
            if (mockResponse.IsSequential && mockResponse.SequenceItems.Count > 0)
            {
                var orderedItems = mockResponse.SequenceItems.OrderBy(s => s.Order).ToList();
                var index = _sequenceStateManager.GetNextIndex(mockResponse.Id, orderedItems.Count);
                var sequenceItem = orderedItems[index];

                _logger.LogInformation(
                    "Sequential response: Mock Id={MockId}, Step {Index}/{Total}",
                    mockResponse.Id, index + 1, orderedItems.Count);

                statusCode = sequenceItem.StatusCode;
                responseBody = sequenceItem.ResponseBody;
                contentType = sequenceItem.ContentType;
                // Sequence item delay overrides mock-level delay
                if (sequenceItem.DelayMs.HasValue)
                    delayMs = sequenceItem.DelayMs;
            }
            // Step 2: Check for matching rules (only if NOT sequential)
            else if (mockResponse.Rules.Count > 0)
            {
                var matchedRule = _ruleEvaluator.Evaluate(mockResponse.Rules, Request, requestBody);
                if (matchedRule != null)
                {
                    _logger.LogInformation(
                        "Rule matched: Id={RuleId} for Mock Id={MockId}",
                        matchedRule.Id, mockResponse.Id);

                    statusCode = matchedRule.StatusCode;
                    responseBody = matchedRule.ResponseBody;
                    contentType = matchedRule.ContentType;
                }
            }

            // Step 3: Apply response delay
            if (delayMs.HasValue && delayMs.Value > 0)
            {
                _logger.LogInformation("Applying delay: {DelayMs}ms for Mock Id={Id}", delayMs.Value, mockResponse.Id);
                await Task.Delay(delayMs.Value);
            }

            // Step 4: Process template variables in response body
            var processedBody = _templateProcessor.ProcessTemplate(responseBody, Request);

            // Step 5: Return response using ContentResult to avoid deserialize/re-serialize issues
            // Previously JsonSerializer.Deserialize<object> caused 204 No Content bug
            responseStatusCode = statusCode;
            result = new ContentResult
            {
                StatusCode = statusCode,
                Content = processedBody,
                ContentType = contentType
            };
        }
        else
        {
            // No match found
            _logger.LogWarning("Mock response not found: {Method} {Path}", requestMethod, requestPath);
            responseStatusCode = 404;
            result = NotFound(new
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

        stopwatch.Stop();

        // Log the request
        await LogRequestAsync(requestMethod, requestPath, queryString, requestBody, mockResponse, responseStatusCode, stopwatch.ElapsedMilliseconds);

        return result;
    }

    private async Task LogRequestAsync(
        string method, string path, string? queryString, string? requestBody,
        Models.MockResponse? matchedMock, int responseStatusCode, long responseTimeMs)
    {
        try
        {
            var requestLog = new RequestLog
            {
                HttpMethod = method,
                Route = path,
                QueryString = string.IsNullOrEmpty(queryString) ? null : queryString,
                RequestBody = requestBody,
                RequestHeaders = SerializeHeaders(Request.Headers),
                MatchedMockId = matchedMock?.Id,
                MatchedMockDescription = matchedMock?.Description,
                ResponseStatusCode = responseStatusCode,
                IsMatched = matchedMock != null,
                Timestamp = DateTime.UtcNow,
                ResponseTimeMs = responseTimeMs
            };

            _dbContext.RequestLogs.Add(requestLog);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log request: {Method} {Path}", method, path);
        }
    }

    private static string SerializeHeaders(IHeaderDictionary headers)
    {
        var dict = headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        return JsonSerializer.Serialize(dict);
    }

    private async Task<Models.MockResponse?> FindMatchingMockResponse(
        string method,
        string path,
        string queryString,
        string? requestBody)
    {
        var query = BuildMockQuery(method, queryString, requestBody)
            .Include(m => m.Rules)
            .Include(m => m.SequenceItems);

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
            // Match mocks that either have no queryString filter (null/empty = match any query)
            // or have a specific queryString that matches the incoming request
            query = query.Where(m => string.IsNullOrEmpty(m.QueryString) || m.QueryString == queryString);
        }

        if (!string.IsNullOrEmpty(requestBody))
        {
            // Match mocks that either have no requestBody filter (null/empty = match any body)
            // or have a specific requestBody that matches the incoming request
            query = query.Where(m => string.IsNullOrEmpty(m.RequestBody) || m.RequestBody == requestBody);
        }

        return query;
    }
}
