using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.Host.Data;
using Mocklab.Host.Models;
using Mocklab.Host.Models.Requests;
using Mocklab.Host.Services;

namespace Mocklab.Host.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("_admin/mocks")]
public class MockAdminController(
    MocklabDbContext dbContext,
    ILogger<MockAdminController> logger,
    IMockImportService importService,
    ISequenceStateManager sequenceStateManager) : ControllerBase
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<MockAdminController> _logger = logger;
    private readonly IMockImportService _importService = importService;
    private readonly ISequenceStateManager _sequenceStateManager = sequenceStateManager;

    /// <summary>
    /// List all mock responses (includes rule count and sequence item count)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllMocks([FromQuery] bool? isActive = null, [FromQuery] int? collectionId = null, [FromQuery] int? folderId = null)
    {
        var query = _dbContext.MockResponses
            .Include(m => m.Rules)
            .Include(m => m.SequenceItems)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        if (collectionId.HasValue)
        {
            query = query.Where(m => m.CollectionId == collectionId.Value);
        }

        if (folderId.HasValue)
        {
            query = query.Where(m => m.FolderId == folderId.Value);
        }

        var mocks = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return Ok(mocks);
    }

    private const string KeyValueOwnerTypeMockResponseRule = "MockResponseRule";

    private static string NormalizeMethod(string? method) => (method ?? "").Trim().ToUpperInvariant();
    private static string NormalizeRoute(string? route) => (route ?? "").Trim();

    private async Task<bool> ExistsMockWithSameMethodAndRouteAsync(int? collectionId, string method, string route, int? excludeId = null)
    {
        var m = NormalizeMethod(method);
        var r = NormalizeRoute(route);
        var query = _dbContext.MockResponses
            .Where(x => x.CollectionId == collectionId);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        var candidates = await query.Select(x => new { x.HttpMethod, x.Route }).ToListAsync();
        return candidates.Any(x => NormalizeMethod(x.HttpMethod) == m && NormalizeRoute(x.Route) == r);
    }

    private static bool HasDuplicateRules(IEnumerable<MockResponseRule>? rules)
    {
        if (rules == null) return false;
        var list = rules.ToList();
        if (list.Count == 0) return false;
        var set = new HashSet<(int StatusCode, string Field, string Op, string? Value)>();
        foreach (var r in list)
        {
            var key = (r.StatusCode, r.ConditionField ?? "", r.ConditionOperator ?? "", r.ConditionValue ?? "");
            if (!set.Add(key)) return true;
        }
        return false;
    }

    /// <summary>
    /// Get a specific mock response with rules and sequence items (rules include responseHeaders from KeyValueEntry).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMock(int id)
    {
        var mock = await _dbContext.MockResponses
            .Include(m => m.Rules.OrderBy(r => r.Priority))
            .Include(m => m.SequenceItems.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mock == null)
        {
            return NotFound(new { Error = "Mock response not found" });
        }

        var ruleIds = mock.Rules.Select(r => r.Id).ToList();
        if (ruleIds.Count > 0)
        {
            var headerEntries = await _dbContext.KeyValueEntries
                .Where(k => k.OwnerType == KeyValueOwnerTypeMockResponseRule && ruleIds.Contains(k.OwnerId))
                .ToListAsync();
            var headersByRuleId = headerEntries
                .GroupBy(k => k.OwnerId)
                .ToDictionary(g => g.Key, g => g.Select(e => new ResponseHeaderItem { Key = e.Key, Value = e.Value }).ToList());
            foreach (var rule in mock.Rules)
            {
                rule.ResponseHeaders = headersByRuleId.TryGetValue(rule.Id, out var list) ? list : new List<ResponseHeaderItem>();
            }
        }

        return Ok(mock);
    }

    /// <summary>
    /// Add a new mock response (with optional rules and sequence items)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMock([FromBody] MockResponse mockResponse)
    {
        if (mockResponse.FolderId.HasValue && mockResponse.CollectionId.HasValue)
        {
            var folder = await _dbContext.MockFolders
                .FirstOrDefaultAsync(f => f.Id == mockResponse.FolderId.Value && f.CollectionId == mockResponse.CollectionId.Value);
            if (folder == null)
                return BadRequest(new { Error = "Folder not found or does not belong to the selected collection" });
        }
        else if (mockResponse.FolderId.HasValue)
        {
            return BadRequest(new { Error = "Folder can only be set when a collection is selected" });
        }

        if (await ExistsMockWithSameMethodAndRouteAsync(mockResponse.CollectionId, mockResponse.HttpMethod, mockResponse.Route))
            return BadRequest(new { Error = "A mock with the same HTTP method and route already exists in this collection." });

        if (HasDuplicateRules(mockResponse.Rules))
            return BadRequest(new { Error = "Duplicate rule: the same status code and condition (field, operator, value) can only be used once per mock." });

        mockResponse.CreatedAt = DateTime.UtcNow;
        mockResponse.UpdatedAt = null;

        _dbContext.MockResponses.Add(mockResponse);
        await _dbContext.SaveChangesAsync();

        if (mockResponse.Rules != null)
        {
            foreach (var rule in mockResponse.Rules)
            {
                var headers = rule.ResponseHeaders;
                if (headers != null && headers.Count > 0)
                {
                    foreach (var h in headers)
                    {
                        if (string.IsNullOrWhiteSpace(h.Key)) continue;
                        _dbContext.KeyValueEntries.Add(new KeyValueEntry
                        {
                            OwnerType = KeyValueOwnerTypeMockResponseRule,
                            OwnerId = rule.Id,
                            Key = h.Key.Trim(),
                            Value = h.Value ?? string.Empty
                        });
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("New mock response added: Id={Id}, Route={Route}",
            mockResponse.Id, mockResponse.Route);

        return CreatedAtAction(nameof(GetMock), new { id = mockResponse.Id }, mockResponse);
    }

    /// <summary>
    /// Update mock response (with rules and sequence items — delete+recreate strategy)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMock(int id, [FromBody] MockResponse mockResponse)
    {
        var existingMock = await _dbContext.MockResponses
            .Include(m => m.Rules)
            .Include(m => m.SequenceItems)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existingMock == null)
        {
            return NotFound(new { Error = "Mock response not found" });
        }

        if (await ExistsMockWithSameMethodAndRouteAsync(mockResponse.CollectionId, mockResponse.HttpMethod, mockResponse.Route, excludeId: id))
            return BadRequest(new { Error = "A mock with the same HTTP method and route already exists in this collection." });

        if (HasDuplicateRules(mockResponse.Rules))
            return BadRequest(new { Error = "Duplicate rule: the same status code and condition (field, operator, value) can only be used once per mock." });

        // Update basic fields
        existingMock.HttpMethod = mockResponse.HttpMethod;
        existingMock.Route = mockResponse.Route;
        existingMock.QueryString = mockResponse.QueryString;
        existingMock.RequestBody = mockResponse.RequestBody;
        existingMock.StatusCode = mockResponse.StatusCode;
        existingMock.ResponseBody = mockResponse.ResponseBody;
        existingMock.ContentType = mockResponse.ContentType;
        existingMock.Description = mockResponse.Description;
        existingMock.DelayMs = mockResponse.DelayMs;
        existingMock.CollectionId = mockResponse.CollectionId;
        if (mockResponse.FolderId.HasValue)
        {
            if (!mockResponse.CollectionId.HasValue)
                return BadRequest(new { Error = "Folder can only be set when a collection is selected" });
            var folder = await _dbContext.MockFolders
                .FirstOrDefaultAsync(f => f.Id == mockResponse.FolderId.Value && f.CollectionId == mockResponse.CollectionId.Value);
            if (folder == null)
                return BadRequest(new { Error = "Folder not found or does not belong to the selected collection" });
            existingMock.FolderId = mockResponse.FolderId;
        }
        else
        {
            existingMock.FolderId = null;
        }

        existingMock.IsSequential = mockResponse.IsSequential;
        existingMock.IsActive = mockResponse.IsActive;
        existingMock.UpdatedAt = DateTime.UtcNow;

        // Update rules: delete existing KeyValueEntries for those rules, then delete rules and recreate
        if (mockResponse.Rules != null)
        {
            var existingRuleIds = existingMock.Rules.Select(r => r.Id).ToList();
            if (existingRuleIds.Count > 0)
            {
                var toRemove = await _dbContext.KeyValueEntries
                    .Where(k => k.OwnerType == KeyValueOwnerTypeMockResponseRule && existingRuleIds.Contains(k.OwnerId))
                    .ToListAsync();
                _dbContext.KeyValueEntries.RemoveRange(toRemove);
            }

            _dbContext.MockResponseRules.RemoveRange(existingMock.Rules);
            foreach (var rule in mockResponse.Rules)
            {
                rule.Id = 0; // Reset ID for new insertion
                rule.MockResponseId = id;
                existingMock.Rules.Add(rule);
            }
        }

        // Update sequence items: delete existing and recreate
        if (mockResponse.SequenceItems != null)
        {
            _dbContext.MockResponseSequenceItems.RemoveRange(existingMock.SequenceItems);
            foreach (var item in mockResponse.SequenceItems)
            {
                item.Id = 0; // Reset ID for new insertion
                item.MockResponseId = id;
                existingMock.SequenceItems.Add(item);
            }
        }

        await _dbContext.SaveChangesAsync();

        // Persist response headers for each rule (KeyValueEntry)
        if (mockResponse.Rules != null)
        {
            for (var i = 0; i < existingMock.Rules.Count; i++)
            {
                var rule = existingMock.Rules.ElementAt(i);
                var headers = i < mockResponse.Rules.Count ? mockResponse.Rules.ElementAt(i).ResponseHeaders : null;
                if (headers != null && headers.Count > 0)
                {
                    foreach (var h in headers)
                    {
                        if (string.IsNullOrWhiteSpace(h.Key)) continue;
                        _dbContext.KeyValueEntries.Add(new KeyValueEntry
                        {
                            OwnerType = KeyValueOwnerTypeMockResponseRule,
                            OwnerId = rule.Id,
                            Key = h.Key.Trim(),
                            Value = h.Value ?? string.Empty
                        });
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        // Populate ResponseHeaders on returned rules (same as GetMock)
        var ruleIds = existingMock.Rules.Select(r => r.Id).ToList();
        if (ruleIds.Count > 0)
        {
            var headerEntries = await _dbContext.KeyValueEntries
                .Where(k => k.OwnerType == KeyValueOwnerTypeMockResponseRule && ruleIds.Contains(k.OwnerId))
                .ToListAsync();
            var headersByRuleId = headerEntries
                .GroupBy(k => k.OwnerId)
                .ToDictionary(g => g.Key, g => g.Select(e => new ResponseHeaderItem { Key = e.Key, Value = e.Value }).ToList());
            foreach (var rule in existingMock.Rules)
            {
                rule.ResponseHeaders = headersByRuleId.TryGetValue(rule.Id, out var list) ? list : new List<ResponseHeaderItem>();
            }
        }

        _logger.LogInformation("Mock response updated: Id={Id}, Rules={RuleCount}, SequenceItems={SeqCount}",
            id, existingMock.Rules.Count, existingMock.SequenceItems.Count);

        return Ok(existingMock);
    }

    /// <summary>
    /// Duplicate a mock response (with rules and sequence items) into the same or a different collection/folder
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> DuplicateMock(int id, [FromBody] DuplicateMockRequest? request = null)
    {
        var source = await _dbContext.MockResponses
            .Include(m => m.Rules)
            .Include(m => m.SequenceItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (source == null)
            return NotFound(new { Error = "Mock response not found" });

        var targetCollectionId = request?.CollectionId ?? source.CollectionId;
        var targetFolderId = request?.FolderId;
        if (request?.CollectionId == null && request?.FolderId == null)
            targetFolderId = source.FolderId;

        if (targetFolderId.HasValue && targetCollectionId.HasValue)
        {
            var folder = await _dbContext.MockFolders
                .FirstOrDefaultAsync(f => f.Id == targetFolderId.Value && f.CollectionId == targetCollectionId.Value);
            if (folder == null)
                return BadRequest(new { Error = "Target folder not found or does not belong to the selected collection" });
        }

        var suffix = " (Copy)";
        var baseRoute = source.Route;
        var newRoute = baseRoute + suffix;

        var counter = 2;
        while (await ExistsMockWithSameMethodAndRouteAsync(targetCollectionId, source.HttpMethod, newRoute))
        {
            newRoute = $"{baseRoute} (Copy {counter})";
            counter++;
        }

        var clone = new MockResponse
        {
            HttpMethod = source.HttpMethod,
            Route = newRoute,
            QueryString = source.QueryString,
            RequestBody = source.RequestBody,
            StatusCode = source.StatusCode,
            ResponseBody = source.ResponseBody,
            ContentType = source.ContentType,
            Description = string.IsNullOrEmpty(source.Description) ? null : source.Description + suffix,
            DelayMs = source.DelayMs,
            CollectionId = targetCollectionId,
            FolderId = targetFolderId,
            IsSequential = source.IsSequential,
            IsActive = source.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var rule in source.Rules)
        {
            clone.Rules.Add(new MockResponseRule
            {
                ConditionField = rule.ConditionField,
                ConditionOperator = rule.ConditionOperator,
                ConditionValue = rule.ConditionValue,
                StatusCode = rule.StatusCode,
                ResponseBody = rule.ResponseBody,
                ContentType = rule.ContentType,
                Priority = rule.Priority
            });
        }

        foreach (var seq in source.SequenceItems)
        {
            clone.SequenceItems.Add(new MockResponseSequenceItem
            {
                Order = seq.Order,
                StatusCode = seq.StatusCode,
                ResponseBody = seq.ResponseBody,
                ContentType = seq.ContentType,
                DelayMs = seq.DelayMs
            });
        }

        _dbContext.MockResponses.Add(clone);
        await _dbContext.SaveChangesAsync();

        // Copy rule response headers (KeyValueEntry)
        if (source.Rules.Count > 0)
        {
            var sourceRuleIds = source.Rules.Select(r => r.Id).ToList();
            var headerEntries = await _dbContext.KeyValueEntries
                .Where(k => k.OwnerType == KeyValueOwnerTypeMockResponseRule && sourceRuleIds.Contains(k.OwnerId))
                .ToListAsync();
            var headersByRuleId = headerEntries
                .GroupBy(k => k.OwnerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var sourceRulesList = source.Rules.OrderBy(r => r.Priority).ToList();
            var cloneRulesList = clone.Rules.OrderBy(r => r.Priority).ToList();
            for (var i = 0; i < sourceRulesList.Count && i < cloneRulesList.Count; i++)
            {
                if (headersByRuleId.TryGetValue(sourceRulesList[i].Id, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        _dbContext.KeyValueEntries.Add(new KeyValueEntry
                        {
                            OwnerType = KeyValueOwnerTypeMockResponseRule,
                            OwnerId = cloneRulesList[i].Id,
                            Key = entry.Key,
                            Value = entry.Value
                        });
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Mock duplicated: SourceId={SourceId}, NewId={NewId}", id, clone.Id);

        return CreatedAtAction(nameof(GetMock), new { id = clone.Id }, clone);
    }

    /// <summary>
    /// Delete mock response
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMock(int id)
    {
        var mock = await _dbContext.MockResponses.FindAsync(id);

        if (mock == null)
        {
            return NotFound(new { Error = "Mock response not found" });
        }

        _dbContext.MockResponses.Remove(mock);
        await _dbContext.SaveChangesAsync();

        // Clean up sequence state
        _sequenceStateManager.Reset(id);

        _logger.LogInformation("Mock response deleted: Id={Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Toggle mock response active/inactive
    /// </summary>
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleMock(int id)
    {
        var mock = await _dbContext.MockResponses.FindAsync(id);

        if (mock == null)
        {
            return NotFound(new { Error = "Mock response not found" });
        }

        mock.IsActive = !mock.IsActive;
        mock.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Mock response status changed: Id={Id}, IsActive={IsActive}",
            id, mock.IsActive);

        return Ok(new
        {
            Id = mock.Id,
            IsActive = mock.IsActive,
            Message = mock.IsActive ? "Mock activated" : "Mock deactivated"
        });
    }

    /// <summary>
    /// Reset sequence counter for a specific mock
    /// </summary>
    [HttpPost("{id}/sequence/reset")]
    public IActionResult ResetSequence(int id)
    {
        _sequenceStateManager.Reset(id);
        _logger.LogInformation("Sequence reset for Mock Id={Id}", id);
        return Ok(new { Message = $"Sequence reset for mock {id}" });
    }

    /// <summary>
    /// Reset all sequence counters
    /// </summary>
    [HttpPost("sequence/reset-all")]
    public IActionResult ResetAllSequences()
    {
        _sequenceStateManager.ResetAll();
        _logger.LogInformation("All sequences reset");
        return Ok(new { Message = "All sequences have been reset" });
    }

    /// <summary>
    /// Bulk update collection and/or folder for multiple mocks
    /// </summary>
    [HttpPost("bulk-update")]
    public async Task<IActionResult> BulkUpdateMocks([FromBody] BulkUpdateMocksRequest request)
    {
        if (request.MockIds == null || request.MockIds.Count == 0)
            return BadRequest(new { Error = "MockIds is required and cannot be empty" });

        if (request.FolderId.HasValue && !request.CollectionId.HasValue)
            return BadRequest(new { Error = "CollectionId is required when FolderId is set" });

        if (request.FolderId.HasValue && request.CollectionId.HasValue)
        {
            var folder = await _dbContext.MockFolders
                .FirstOrDefaultAsync(f => f.Id == request.FolderId.Value && f.CollectionId == request.CollectionId.Value);
            if (folder == null)
                return BadRequest(new { Error = "Folder not found or does not belong to the selected collection" });
        }

        var mocks = await _dbContext.MockResponses
            .Where(m => request.MockIds.Contains(m.Id))
            .ToListAsync();

        foreach (var mock in mocks)
        {
            if (request.CollectionId.HasValue)
            {
                mock.CollectionId = request.CollectionId.Value;
                mock.FolderId = request.FolderId;
            }
            else
            {
                mock.CollectionId = null;
                mock.FolderId = null;
            }
            mock.UpdatedAt = DateTime.UtcNow;
        }

        var targetCollectionId = request.CollectionId;
        var mocksInTargetAfterUpdate = await _dbContext.MockResponses
            .Where(m => m.CollectionId == targetCollectionId && !request.MockIds.Contains(m.Id))
            .Select(m => new { m.HttpMethod, m.Route })
            .ToListAsync();
        var keysInTarget = mocksInTargetAfterUpdate
            .Select(m => (NormalizeMethod(m.HttpMethod), NormalizeRoute(m.Route)))
            .ToHashSet();
        foreach (var mock in mocks)
        {
            var key = (NormalizeMethod(mock.HttpMethod), NormalizeRoute(mock.Route));
            if (!keysInTarget.Add(key))
            {
                return BadRequest(new { Error = "Bulk update would create a duplicate: same HTTP method and route already exist in the target collection." });
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Bulk updated {Count} mock(s)", mocks.Count);

        return Ok(new { UpdatedCount = mocks.Count });
    }

    /// <summary>
    /// Clear all mocks
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearAllMocks()
    {
        var count = await _dbContext.MockResponses.CountAsync();
        _dbContext.MockResponses.RemoveRange(_dbContext.MockResponses);
        await _dbContext.SaveChangesAsync();

        // Reset all sequence state
        _sequenceStateManager.ResetAll();

        _logger.LogWarning("All mock responses deleted. Deleted count: {Count}", count);

        return Ok(new
        {
            Message = "All mock responses deleted",
            DeletedCount = count
        });
    }

    /// <summary>
    /// Import a mock from a cURL command.
    /// </summary>
    [HttpPost("import/curl")]
    public async Task<IActionResult> ImportFromCurl([FromBody] ImportCurlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Curl))
            return BadRequest(new { Error = "cURL command cannot be empty." });

        var result = await _importService.ImportFromCurlAsync(request.Curl);

        if (!result.Success)
            return BadRequest(new { Error = result.ErrorMessage });

        return CreatedAtAction(nameof(GetMock), new { id = result.Mock!.Id }, result.Mock);
    }

    /// <summary>
    /// Import mocks from an OpenAPI (Swagger) JSON specification.
    /// </summary>
    [HttpPost("import/openapi")]
    public async Task<IActionResult> ImportFromOpenApi([FromBody] ImportOpenApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OpenApiJson))
            return BadRequest(new { Error = "OpenAPI JSON cannot be empty." });

        var result = await _importService.ImportFromOpenApiAsync(request.OpenApiJson);

        if (!result.Success)
            return BadRequest(new { Error = result.ErrorMessage });

        return Ok(new
        {
            Message = $"Successfully imported {result.ImportedCount} mock response(s) from OpenAPI specification.",
            result.ImportedCount,
            result.Mocks
        });
    }
}
