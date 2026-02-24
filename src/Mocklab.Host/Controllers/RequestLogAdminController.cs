using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.Host.Data;

namespace Mocklab.Host.Controllers;

/// <summary>
/// Admin controller for managing request logs
/// </summary>
[ApiController]
[Route("_admin/logs")]
public class RequestLogAdminController(
    MocklabDbContext dbContext,
    ILogger<RequestLogAdminController> logger) : ControllerBase
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<RequestLogAdminController> _logger = logger;

    /// <summary>
    /// List request logs with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? method = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] bool? isMatched = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _dbContext.RequestLogs.AsQueryable();

        if (!string.IsNullOrEmpty(method))
            query = query.Where(l => l.HttpMethod == method);

        if (statusCode.HasValue)
            query = query.Where(l => l.ResponseStatusCode == statusCode.Value);

        if (isMatched.HasValue)
            query = query.Where(l => l.IsMatched == isMatched.Value);

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Data = logs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get a specific request log
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLog(int id)
    {
        var log = await _dbContext.RequestLogs.FindAsync(id);

        if (log == null)
            return NotFound(new { Error = "Request log not found" });

        return Ok(log);
    }

    /// <summary>
    /// Get count of recent request logs (for sidebar badge)
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetRecentCount([FromQuery] int minutes = 5)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutes);
        var count = await _dbContext.RequestLogs
            .Where(l => l.Timestamp >= since)
            .CountAsync();

        return Ok(new { Count = count, Minutes = minutes });
    }

    /// <summary>
    /// Clear all request logs
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearLogs()
    {
        var count = await _dbContext.RequestLogs.CountAsync();
        _dbContext.RequestLogs.RemoveRange(_dbContext.RequestLogs);
        await _dbContext.SaveChangesAsync();

        _logger.LogWarning("All request logs cleared. Deleted count: {Count}", count);

        return Ok(new
        {
            Message = "All request logs cleared",
            DeletedCount = count
        });
    }
}
