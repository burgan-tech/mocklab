using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.App.Data;
using Mocklab.App.Models;

namespace Mocklab.App.Controllers;

/// <summary>
/// Admin controller for managing mock collections
/// </summary>
[ApiController]
[Route("_admin/collections")]
public class CollectionAdminController(
    MocklabDbContext dbContext,
    ILogger<CollectionAdminController> logger) : ControllerBase
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<CollectionAdminController> _logger = logger;

    /// <summary>
    /// List all collections with mock count
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCollections()
    {
        var collections = await _dbContext.MockCollections
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Color,
                c.CreatedAt,
                c.UpdatedAt,
                MockCount = c.MockResponses.Count
            })
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(collections);
    }

    /// <summary>
    /// Get a specific collection with its mocks
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCollection(int id)
    {
        var collection = await _dbContext.MockCollections
            .Include(c => c.MockResponses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null)
            return NotFound(new { Error = "Collection not found" });

        return Ok(collection);
    }

    /// <summary>
    /// Create a new collection
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCollection([FromBody] MockCollection collection)
    {
        collection.CreatedAt = DateTime.UtcNow;
        collection.UpdatedAt = null;

        _dbContext.MockCollections.Add(collection);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("New collection created: Id={Id}, Name={Name}", collection.Id, collection.Name);

        return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, collection);
    }

    /// <summary>
    /// Update a collection
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCollection(int id, [FromBody] MockCollection collection)
    {
        var existing = await _dbContext.MockCollections.FindAsync(id);

        if (existing == null)
            return NotFound(new { Error = "Collection not found" });

        existing.Name = collection.Name;
        existing.Description = collection.Description;
        existing.Color = collection.Color;
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Collection updated: Id={Id}", id);

        return Ok(existing);
    }

    /// <summary>
    /// Delete a collection (mocks get CollectionId = null)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollection(int id)
    {
        var collection = await _dbContext.MockCollections.FindAsync(id);

        if (collection == null)
            return NotFound(new { Error = "Collection not found" });

        _dbContext.MockCollections.Remove(collection);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Collection deleted: Id={Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Export a collection as JSON
    /// </summary>
    [HttpPost("{id}/export")]
    public async Task<IActionResult> ExportCollection(int id)
    {
        var collection = await _dbContext.MockCollections
            .Include(c => c.MockResponses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null)
            return NotFound(new { Error = "Collection not found" });

        var exportData = new
        {
            Collection = new
            {
                collection.Name,
                collection.Description,
                collection.Color
            },
            Mocks = collection.MockResponses.Select(m => new
            {
                m.HttpMethod,
                m.Route,
                m.QueryString,
                m.RequestBody,
                m.StatusCode,
                m.ResponseBody,
                m.ContentType,
                m.Description,
                m.DelayMs,
                m.IsActive
            })
        };

        return Ok(exportData);
    }

    /// <summary>
    /// Import a collection from JSON
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportCollection([FromBody] JsonElement importData)
    {
        try
        {
            var collectionData = importData.GetProperty("collection");
            var mocksData = importData.GetProperty("mocks");

            var collection = new MockCollection
            {
                Name = collectionData.GetProperty("name").GetString() ?? "Imported Collection",
                Description = collectionData.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                Color = collectionData.TryGetProperty("color", out var color) ? color.GetString() : null,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.MockCollections.Add(collection);
            await _dbContext.SaveChangesAsync();

            var importedCount = 0;
            foreach (var mockData in mocksData.EnumerateArray())
            {
                var mock = new MockResponse
                {
                    HttpMethod = mockData.GetProperty("httpMethod").GetString() ?? "GET",
                    Route = mockData.GetProperty("route").GetString() ?? "/",
                    QueryString = mockData.TryGetProperty("queryString", out var qs) ? qs.GetString() : null,
                    RequestBody = mockData.TryGetProperty("requestBody", out var rb) ? rb.GetString() : null,
                    StatusCode = mockData.TryGetProperty("statusCode", out var sc) ? sc.GetInt32() : 200,
                    ResponseBody = mockData.TryGetProperty("responseBody", out var respBody) ? respBody.GetString() ?? "{}" : "{}",
                    ContentType = mockData.TryGetProperty("contentType", out var ct) ? ct.GetString() ?? "application/json" : "application/json",
                    Description = mockData.TryGetProperty("description", out var d) ? d.GetString() : null,
                    DelayMs = mockData.TryGetProperty("delayMs", out var delay) && delay.ValueKind == JsonValueKind.Number ? delay.GetInt32() : null,
                    IsActive = mockData.TryGetProperty("isActive", out var ia) ? ia.GetBoolean() : true,
                    CollectionId = collection.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.MockResponses.Add(mock);
                importedCount++;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Collection imported: Id={Id}, Name={Name}, Mocks={Count}",
                collection.Id, collection.Name, importedCount);

            return Ok(new
            {
                Message = $"Successfully imported collection '{collection.Name}' with {importedCount} mock(s)",
                CollectionId = collection.Id,
                ImportedCount = importedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import collection");
            return BadRequest(new { Error = "Invalid import data format: " + ex.Message });
        }
    }
}
