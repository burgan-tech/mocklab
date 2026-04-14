using System.Text.Json;

namespace Mocklab.Host.Services;

/// <summary>
/// Imports a single collection JSON document into the database during seed.
/// </summary>
public interface IJsonSeedImporter
{
    /// <summary>
    /// Imports a collection from a parsed JSON element.
    /// If a collection with the same name already exists the import is skipped (idempotent).
    /// </summary>
    /// <param name="root">Parsed JSON root element (collection export format).</param>
    /// <param name="sourceFile">Source file path used for logging only.</param>
    Task<SeedImportResult> ImportAsync(JsonElement root, string sourceFile);
}

/// <summary>
/// Result of a single seed file import attempt.
/// </summary>
public record SeedImportResult(bool Skipped, string? SkipReason, int MocksImported);
