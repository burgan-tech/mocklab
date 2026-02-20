using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.App.Data;
using Mocklab.App.Models;
using Mocklab.App.Services;

namespace Mocklab.App.Controllers;

/// <summary>
/// Admin controller for managing mock collections
/// </summary>
[ApiController]
[Route("_admin/collections")]
public class CollectionAdminController(
    MocklabDbContext dbContext,
    ILogger<CollectionAdminController> logger,
    ISequenceStateManager sequenceStateManager) : ControllerBase
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<CollectionAdminController> _logger = logger;
    private readonly ISequenceStateManager _sequenceStateManager = sequenceStateManager;

    /// <summary>
    /// List all collections with mock count. Optionally include folders for each collection (for tree UI).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCollections([FromQuery] bool includeFolders = false)
    {
        if (!includeFolders)
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

        var collectionsWithFolders = await _dbContext.MockCollections
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Color,
                c.CreatedAt,
                c.UpdatedAt,
                MockCount = c.MockResponses.Count,
                Folders = c.Folders.Select(f => new
                {
                    f.Id,
                    f.CollectionId,
                    f.ParentFolderId,
                    f.Name,
                    f.Color,
                    f.CreatedAt,
                    f.UpdatedAt,
                    MockCount = f.MockResponses.Count
                }).OrderBy(f => f.Name).ToList()
            })
            .ToListAsync();

        return Ok(collectionsWithFolders);
    }

    /// <summary>
    /// List folders for a collection (flat list with ParentFolderId for building tree)
    /// </summary>
    [HttpGet("{id}/folders")]
    public async Task<IActionResult> GetFolders(int id)
    {
        var collectionExists = await _dbContext.MockCollections.AnyAsync(c => c.Id == id);
        if (!collectionExists)
            return NotFound(new { Error = "Collection not found" });

        var folders = await _dbContext.MockFolders
            .Where(f => f.CollectionId == id)
            .Select(f => new
            {
                f.Id,
                f.CollectionId,
                f.ParentFolderId,
                f.Name,
                f.Color,
                f.CreatedAt,
                f.UpdatedAt,
                MockCount = f.MockResponses.Count
            })
            .OrderBy(f => f.Name)
            .ToListAsync();

        return Ok(folders);
    }

    /// <summary>
    /// Create a folder in a collection
    /// </summary>
    [HttpPost("{id}/folders")]
    public async Task<IActionResult> CreateFolder(int id, [FromBody] MockFolder folder)
    {
        var collection = await _dbContext.MockCollections.FindAsync(id);
        if (collection == null)
            return NotFound(new { Error = "Collection not found" });

        folder.CollectionId = id;
        folder.Id = 0;
        folder.CreatedAt = DateTime.UtcNow;
        folder.UpdatedAt = null;

        if (folder.ParentFolderId.HasValue)
        {
            var parent = await _dbContext.MockFolders
                .FirstOrDefaultAsync(f => f.Id == folder.ParentFolderId.Value && f.CollectionId == id);
            if (parent == null)
                return BadRequest(new { Error = "Parent folder not found or does not belong to this collection" });
        }

        _dbContext.MockFolders.Add(folder);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Folder created: Id={Id}, Name={Name}, CollectionId={CollectionId}",
            folder.Id, folder.Name, id);

        var created = new
        {
            folder.Id,
            folder.CollectionId,
            folder.ParentFolderId,
            folder.Name,
            folder.Color,
            folder.CreatedAt,
            folder.UpdatedAt
        };
        return CreatedAtAction(nameof(GetFolders), new { id }, created);
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
    /// Delete a collection. When deleteMocks=true, all mocks in the collection are deleted; otherwise their CollectionId is set to null.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollection(int id, [FromQuery] bool deleteMocks = false)
    {
        var collection = await _dbContext.MockCollections
            .Include(c => c.MockResponses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null)
            return NotFound(new { Error = "Collection not found" });

        if (deleteMocks)
        {
            var mockIds = collection.MockResponses.Select(m => m.Id).ToList();
            _dbContext.MockResponses.RemoveRange(collection.MockResponses);
            foreach (var mockId in mockIds)
                _sequenceStateManager.Reset(mockId);
        }
        else
        {
            foreach (var mock in collection.MockResponses)
            {
                mock.CollectionId = null;
                mock.FolderId = null;
            }
        }

        _dbContext.MockCollections.Remove(collection);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Collection deleted: Id={Id}, DeleteMocks={DeleteMocks}", id, deleteMocks);

        return NoContent();
    }

    /// <summary>
    /// Export a collection as JSON (includes folders and mock folderIndex for import).
    /// </summary>
    [HttpPost("{id}/export")]
    public async Task<IActionResult> ExportCollection(int id)
    {
        var collection = await _dbContext.MockCollections
            .Include(c => c.Folders)
            .Include(c => c.MockResponses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null)
            return NotFound(new { Error = "Collection not found" });

        // Order folders so parent always comes before children (topological order for import).
        var allFolders = collection.Folders.ToList();
        var orderedFolders = new List<MockFolder>();
        var added = new HashSet<int>();
        while (added.Count < allFolders.Count)
        {
            var next = allFolders
                .Where(f => !added.Contains(f.Id) && (!f.ParentFolderId.HasValue || added.Contains(f.ParentFolderId!.Value)))
                .OrderBy(f => f.Id)
                .ToList();
            if (next.Count == 0) break;
            foreach (var f in next)
            {
                orderedFolders.Add(f);
                added.Add(f.Id);
            }
        }
        var folderIdToIndex = orderedFolders.Select((f, i) => (f.Id, i)).ToDictionary(x => x.Id, x => x.i);

        var foldersExport = orderedFolders.Select(f => new
        {
            f.Name,
            f.Color,
            ParentFolderIndex = f.ParentFolderId.HasValue && folderIdToIndex.TryGetValue(f.ParentFolderId.Value, out var pi) ? pi : -1
        }).ToList();

        var mocksExport = collection.MockResponses.Select(m =>
        {
            int? folderIndex = null;
            if (m.FolderId.HasValue && folderIdToIndex.TryGetValue(m.FolderId.Value, out var fi))
                folderIndex = fi;
            return new
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
                m.IsActive,
                FolderIndex = folderIndex
            };
        }).ToList();

        var exportData = new
        {
            Collection = new
            {
                collection.Name,
                collection.Description,
                collection.Color
            },
            Folders = foldersExport,
            Mocks = mocksExport
        };

        return Ok(exportData);
    }

    /// <summary>
    /// Import a collection from JSON (supports optional folders and mock folderIndex).
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
                Color = collectionData.TryGetProperty("color", out var colors) ? colors.GetString() : null,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.MockCollections.Add(collection);
            await _dbContext.SaveChangesAsync();

            var newFolderIds = new List<int>();
            if (importData.TryGetProperty("folders", out var foldersData))
            {
                foreach (var folderEl in foldersData.EnumerateArray())
                {
                    var name = folderEl.GetProperty("name").GetString() ?? "Folder";
                    var color = folderEl.TryGetProperty("color", out var c) ? c.GetString() : null;
                    var parentIndex = folderEl.TryGetProperty("parentFolderIndex", out var p) && p.ValueKind == JsonValueKind.Number ? p.GetInt32() : -1;
                    int? parentFolderId = parentIndex >= 0 && parentIndex < newFolderIds.Count ? newFolderIds[parentIndex] : null;

                    var folder = new MockFolder
                    {
                        CollectionId = collection.Id,
                        ParentFolderId = parentFolderId,
                        Name = name,
                        Color = color,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.MockFolders.Add(folder);
                    await _dbContext.SaveChangesAsync();
                    newFolderIds.Add(folder.Id);
                }
            }

            var importedCount = 0;
            foreach (var mockData in mocksData.EnumerateArray())
            {
                int? folderId = null;
                if (mockData.TryGetProperty("folderIndex", out var folderIndexEl) && folderIndexEl.ValueKind == JsonValueKind.Number)
                {
                    var folderIndex = folderIndexEl.GetInt32();
                    if (folderIndex >= 0 && folderIndex < newFolderIds.Count)
                        folderId = newFolderIds[folderIndex];
                }

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
                    FolderId = folderId,
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
