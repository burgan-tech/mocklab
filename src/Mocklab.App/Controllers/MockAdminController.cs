using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.App.Data;
using Mocklab.App.Models;
using Mocklab.App.Models.Requests;
using Mocklab.App.Services;

namespace Mocklab.App.Controllers;

[ApiController]
[Route("_admin/mocks")]
public class MockAdminController : ControllerBase
{
    private readonly MocklabDbContext _dbContext;
    private readonly ILogger<MockAdminController> _logger;
    private readonly IMockImportService _importService;

    public MockAdminController(
        MocklabDbContext dbContext,
        ILogger<MockAdminController> logger,
        IMockImportService importService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _importService = importService;
    }

    /// <summary>
    /// List all mock responses
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllMocks([FromQuery] bool? isActive = null)
    {
        var query = _dbContext.MockResponses.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        var mocks = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return Ok(mocks);
    }

    /// <summary>
    /// Get a specific mock response
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMock(int id)
    {
        var mock = await _dbContext.MockResponses.FindAsync(id);

        if (mock == null)
        {
            return NotFound(new { Error = "Mock response not found" });
        }

        return Ok(mock);
    }

    /// <summary>
    /// Add a new mock response
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMock([FromBody] MockResponse mockResponse)
    {
        mockResponse.CreatedAt = DateTime.UtcNow;
        mockResponse.UpdatedAt = null;

        _dbContext.MockResponses.Add(mockResponse);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("New mock response added: Id={Id}, Route={Route}",
            mockResponse.Id, mockResponse.Route);

        return CreatedAtAction(nameof(GetMock), new { id = mockResponse.Id }, mockResponse);
    }

    /// <summary>
    /// Update mock response
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMock(int id, [FromBody] MockResponse mockResponse)
    {
        var existingMock = await _dbContext.MockResponses.FindAsync(id);

        if (existingMock == null)
        {
            return NotFound(new { Error = "Mock response not found" });
        }

        existingMock.HttpMethod = mockResponse.HttpMethod;
        existingMock.Route = mockResponse.Route;
        existingMock.QueryString = mockResponse.QueryString;
        existingMock.RequestBody = mockResponse.RequestBody;
        existingMock.StatusCode = mockResponse.StatusCode;
        existingMock.ResponseBody = mockResponse.ResponseBody;
        existingMock.ContentType = mockResponse.ContentType;
        existingMock.Description = mockResponse.Description;
        existingMock.IsActive = mockResponse.IsActive;
        existingMock.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Mock response updated: Id={Id}", id);

        return Ok(existingMock);
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
    /// Clear all mocks
    /// </summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearAllMocks()
    {
        var count = await _dbContext.MockResponses.CountAsync();
        _dbContext.MockResponses.RemoveRange(_dbContext.MockResponses);
        await _dbContext.SaveChangesAsync();

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
