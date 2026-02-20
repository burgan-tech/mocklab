using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mocklab.App.Data;
using Mocklab.App.Models;
using Mocklab.App.Services;

namespace Mocklab.App.Controllers;

/// <summary>
/// Admin controller for managing mock folders (update and delete; create is under collections)
/// </summary>
[ApiController]
[Route("_admin/folders")]
public class FolderAdminController(
    MocklabDbContext dbContext,
    ILogger<FolderAdminController> logger,
    ISequenceStateManager sequenceStateManager) : ControllerBase
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<FolderAdminController> _logger = logger;
    private readonly ISequenceStateManager _sequenceStateManager = sequenceStateManager;

    /// <summary>
    /// Update a folder
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFolder(int id, [FromBody] MockFolder folder)
    {
        var existing = await _dbContext.MockFolders.FindAsync(id);
        if (existing == null)
            return NotFound(new { Error = "Folder not found" });

        existing.Name = folder.Name;
        existing.Color = folder.Color;
        existing.UpdatedAt = DateTime.UtcNow;

        if (folder.ParentFolderId.HasValue)
        {
            if (folder.ParentFolderId == id)
                return BadRequest(new { Error = "Folder cannot be its own parent" });
            var parent = await _dbContext.MockFolders
                .FirstOrDefaultAsync(f => f.Id == folder.ParentFolderId.Value && f.CollectionId == existing.CollectionId);
            if (parent == null)
                return BadRequest(new { Error = "Parent folder not found or does not belong to this collection" });
            existing.ParentFolderId = folder.ParentFolderId;
        }
        else
        {
            existing.ParentFolderId = null;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Folder updated: Id={Id}", id);

        return Ok(new
        {
            existing.Id,
            existing.CollectionId,
            existing.ParentFolderId,
            existing.Name,
            existing.Color,
            existing.CreatedAt,
            existing.UpdatedAt
        });
    }

    /// <summary>
    /// Delete a folder. When deleteMocks=true, mocks in the folder are deleted; otherwise their FolderId is set to null.
    /// Child folders always get ParentFolderId = null.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFolder(int id, [FromQuery] bool deleteMocks = false)
    {
        var folder = await _dbContext.MockFolders
            .Include(f => f.MockResponses)
            .Include(f => f.Children)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (folder == null)
            return NotFound(new { Error = "Folder not found" });

        if (deleteMocks)
        {
            var mockIds = folder.MockResponses.Select(m => m.Id).ToList();
            _dbContext.MockResponses.RemoveRange(folder.MockResponses);
            foreach (var mockId in mockIds)
                _sequenceStateManager.Reset(mockId);
        }
        else
        {
            foreach (var mock in folder.MockResponses)
                mock.FolderId = null;
        }

        foreach (var child in folder.Children)
            child.ParentFolderId = null;

        _dbContext.MockFolders.Remove(folder);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Folder deleted: Id={Id}, DeleteMocks={DeleteMocks}", id, deleteMocks);

        return NoContent();
    }
}
