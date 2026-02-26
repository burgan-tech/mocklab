using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mocklab.Host.Constants;
using Mocklab.Host.Data;
using Mocklab.Host.Extensions;
using Mocklab.Host.Models;
using Mocklab.Host.Services;

namespace Mocklab.Host.Controllers;

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
        if (Request.ContentLength > 0 && (requestMethod == HttpConstants.MethodPost || requestMethod == HttpConstants.MethodPut || requestMethod == HttpConstants.MethodPatch))
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

        // Load response headers for rules (KeyValueEntry) so matched rule can apply them
        if (mockResponse?.Rules.Count > 0)
        {
            var ruleIds = mockResponse.Rules.Select(r => r.Id).ToList();
            var headerEntries = await _dbContext.KeyValueEntries
                .Where(k => k.OwnerType == "MockResponseRule" && ruleIds.Contains(k.OwnerId))
                .ToListAsync();
            var headersByRuleId = headerEntries
                .GroupBy(k => k.OwnerId)
                .ToDictionary(g => g.Key, g => g.Select(e => new ResponseHeaderItem { Key = e.Key, Value = e.Value }).ToList());
            foreach (var rule in mockResponse.Rules)
            {
                rule.ResponseHeaders = headersByRuleId.TryGetValue(rule.Id, out var list) ? list : new List<ResponseHeaderItem>();
            }
        }

        IActionResult result;
        int responseStatusCode;

        if (mockResponse != null)
        {
            _logger.LogInformation(
                "Mock response found: Id={Id}, Description={Description}",
                mockResponse.Id,
                mockResponse.Description
            );

            // Build template context once (route params, request body, data buckets) for body and header value processing
            var routeParams = RuleEvaluator.GetRouteParameters(mockResponse.Route, requestPath);
            var templateContext = new TemplateRequestContext
            {
                RouteParams = routeParams,
                RequestBody = requestBody
            };
            if (mockResponse.CollectionId.HasValue)
            {
                var bucketEntities = await _dbContext.DataBuckets
                    .Where(b => b.CollectionId == mockResponse.CollectionId.Value)
                    .ToListAsync();
                var buckets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var b in bucketEntities)
                {
                    var obj = JsonToScribanHelper.FromJson(b.Data);
                    if (obj != null)
                        buckets[b.Name] = obj;
                }
                templateContext.Buckets = buckets;
            }

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
                var matchedRule = _ruleEvaluator.Evaluate(mockResponse.Rules, Request, requestBody, mockResponse.Route, requestPath);
                if (matchedRule != null)
                {
                    _logger.LogInformation(
                        "Rule matched: Id={RuleId} for Mock Id={MockId}",
                        matchedRule.Id, mockResponse.Id);

                    statusCode = matchedRule.StatusCode;
                    responseBody = matchedRule.ResponseBody;
                    contentType = matchedRule.ContentType;
                    if (matchedRule.ResponseHeaders != null && matchedRule.ResponseHeaders.Count > 0)
                    {
                        foreach (var h in matchedRule.ResponseHeaders)
                        {
                            if (!string.IsNullOrWhiteSpace(h.Key))
                            {
                                var processedValue = _templateProcessor.ProcessTemplate(h.Value ?? string.Empty, Request, templateContext);
                                Response.Headers.Append(h.Key, processedValue);
                            }
                        }
                    }
                }
            }

            // Step 3: Apply response delay
            if (delayMs.HasValue && delayMs.Value > 0)
            {
                _logger.LogInformation("Applying delay: {DelayMs}ms for Mock Id={Id}", delayMs.Value, mockResponse.Id);
                await Task.Delay(delayMs.Value);
            }

            // Step 4: Process template variables in response body
            var processedBody = _templateProcessor.ProcessTemplate(responseBody, Request, templateContext);

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
        var query = BuildMockQuery(method, queryString);
        var candidates = await query.ToListAsync();

        // Helper: prefer candidates with matching body over those without
        Models.MockResponse? SelectBestMatch(IEnumerable<Models.MockResponse> matches)
        {
            var list = matches.ToList();
            if (list.Count <= 1) return list.FirstOrDefault();

            var withBody = list.FirstOrDefault(m =>
                !string.IsNullOrEmpty(m.RequestBody) && !string.IsNullOrEmpty(requestBody) &&
                requestBody.Contains(m.RequestBody, StringComparison.OrdinalIgnoreCase));
            return withBody ?? list.FirstOrDefault(m => string.IsNullOrEmpty(m.RequestBody));
        }

        // 1) Exact route match (prefer body-matching mock)
        var exactMatches = candidates.Where(m => m.Route == path).ToList();
        if (exactMatches.Count > 0)
            return SelectBestMatch(exactMatches);

        // 2) Parametric match: /api/users/{id} matches /api/users/123
        var parametricMatches = candidates
            .Where(m => m.Route.Contains('{', StringComparison.Ordinal) &&
                        RuleEvaluator.GetRouteParameters(m.Route, path) != null)
            .ToList();
        if (parametricMatches.Count > 0)
            return SelectBestMatch(parametricMatches);

        // 3) Fallback: path contains route
        var fallbackMatches = candidates
            .Where(m => !string.IsNullOrEmpty(m.Route) && path.Contains(m.Route, StringComparison.Ordinal))
            .ToList();
        return SelectBestMatch(fallbackMatches);
    }

    private IQueryable<Models.MockResponse> BuildMockQuery(string method, string? queryString)
    {
        var query = _dbContext.MockResponses
            .AsQueryable()
            .Include(m => m.Rules)
            .Include(m => m.SequenceItems)
            .Where(m => m.IsActive && m.HttpMethod == method);

        if (!string.IsNullOrEmpty(queryString))
        {
            query = query.Where(m => string.IsNullOrEmpty(m.QueryString) || m.QueryString == queryString);
        }

        return query;
    }
}
