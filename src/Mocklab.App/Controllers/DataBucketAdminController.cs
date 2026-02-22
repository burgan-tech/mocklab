using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.App.Data;
using Mocklab.App.Models;

namespace Mocklab.App.Controllers;

/// <summary>
/// Admin API for collection-level data buckets (used in Scriban templates).
/// </summary>
[ApiController]
[Route("_admin/collections/{collectionId:int}/data-buckets")]
public class DataBucketAdminController(MocklabDbContext dbContext, ILogger<DataBucketAdminController> logger) : ControllerBase
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<DataBucketAdminController> _logger = logger;

    /// <summary>
    /// List all data buckets for a collection.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int collectionId)
    {
        var exists = await _dbContext.MockCollections.AnyAsync(c => c.Id == collectionId);
        if (!exists)
            return NotFound(new { Error = "Collection not found" });

        var buckets = await _dbContext.DataBuckets
            .Where(b => b.CollectionId == collectionId)
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.CollectionId, b.Name, b.Description, b.CreatedAt, b.UpdatedAt })
            .ToListAsync();
        return Ok(buckets);
    }

    /// <summary>
    /// Get a single data bucket by id (must belong to the collection).
    /// </summary>
    [HttpGet("{bucketId:int}")]
    public async Task<IActionResult> GetOne(int collectionId, int bucketId)
    {
        var bucket = await _dbContext.DataBuckets
            .Where(b => b.CollectionId == collectionId && b.Id == bucketId)
            .Select(b => new { b.Id, b.CollectionId, b.Name, b.Description, b.Data, b.CreatedAt, b.UpdatedAt })
            .FirstOrDefaultAsync();
        if (bucket == null)
            return NotFound(new { Error = "Data bucket not found" });
        return Ok(bucket);
    }

    /// <summary>
    /// Create a new data bucket for the collection.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(int collectionId, [FromBody] DataBucketCreateUpdateRequest request)
    {
        var exists = await _dbContext.MockCollections.AnyAsync(c => c.Id == collectionId);
        if (!exists)
            return NotFound(new { Error = "Collection not found" });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { Error = "Name is required" });

        var nameExists = await _dbContext.DataBuckets
            .AnyAsync(b => b.CollectionId == collectionId && b.Name == request.Name.Trim());
        if (nameExists)
            return Conflict(new { Error = "A data bucket with this name already exists in the collection" });

        var bucket = new DataBucket
        {
            CollectionId = collectionId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Data = request.Data ?? "[]"
        };
        _dbContext.DataBuckets.Add(bucket);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created data bucket Id={Id}, Name={Name}, CollectionId={CollectionId}", bucket.Id, bucket.Name, collectionId);
        return CreatedAtAction(nameof(GetOne), new { collectionId, bucketId = bucket.Id }, new { bucket.Id, bucket.CollectionId, bucket.Name, bucket.Description, bucket.CreatedAt, bucket.UpdatedAt });
    }

    /// <summary>
    /// Update an existing data bucket.
    /// </summary>
    [HttpPut("{bucketId:int}")]
    public async Task<IActionResult> Update(int collectionId, int bucketId, [FromBody] DataBucketCreateUpdateRequest request)
    {
        var bucket = await _dbContext.DataBuckets
            .FirstOrDefaultAsync(b => b.CollectionId == collectionId && b.Id == bucketId);
        if (bucket == null)
            return NotFound(new { Error = "Data bucket not found" });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { Error = "Name is required" });

        var nameExists = await _dbContext.DataBuckets
            .AnyAsync(b => b.CollectionId == collectionId && b.Name == request.Name.Trim() && b.Id != bucketId);
        if (nameExists)
            return Conflict(new { Error = "A data bucket with this name already exists in the collection" });

        bucket.Name = request.Name.Trim();
        bucket.Description = request.Description?.Trim();
        bucket.Data = request.Data ?? "[]";
        bucket.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated data bucket Id={Id}, CollectionId={CollectionId}", bucketId, collectionId);
        return Ok(new { bucket.Id, bucket.CollectionId, bucket.Name, bucket.Description, bucket.UpdatedAt });
    }

    /// <summary>
    /// Delete a data bucket.
    /// </summary>
    [HttpDelete("{bucketId:int}")]
    public async Task<IActionResult> Delete(int collectionId, int bucketId)
    {
        var bucket = await _dbContext.DataBuckets
            .FirstOrDefaultAsync(b => b.CollectionId == collectionId && b.Id == bucketId);
        if (bucket == null)
            return NotFound(new { Error = "Data bucket not found" });

        _dbContext.DataBuckets.Remove(bucket);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted data bucket Id={Id}, CollectionId={CollectionId}", bucketId, collectionId);
        return NoContent();
    }
}

/// <summary>
/// Request body for creating or updating a data bucket.
/// </summary>
public class DataBucketCreateUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>
    /// JSON string: array of objects or single object. Default "[]".
    /// </summary>
    public string? Data { get; set; }
}
