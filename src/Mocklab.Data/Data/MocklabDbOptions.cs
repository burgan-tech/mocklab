namespace Mocklab.App.Data;

/// <summary>
/// Database-level options for MocklabDbContext (schema name, etc.)
/// </summary>
public class MocklabDbOptions
{
    public string SchemaName { get; set; } = "mocklab";
}
