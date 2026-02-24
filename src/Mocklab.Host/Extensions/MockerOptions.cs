namespace Mocklab.Host.Extensions;

/// <summary>
/// Configuration options for Mocklab API
/// </summary>
public class MocklabOptions
{
    /// <summary>
    /// Use the host application's database connection.
    /// When true, Mocklab will use the same database as the host application.
    /// When false, it will use its own SQLite database (standalone mode).
    /// Default: false
    /// </summary>
    public bool UseHostDatabase { get; set; } = false;

    /// <summary>
    /// Database schema name for Mocklab tables.
    /// Used when UseHostDatabase is true to isolate Mocklab tables in a separate schema.
    /// Default: "mocklab"
    /// </summary>
    public string SchemaName { get; set; } = "mocklab";

    /// <summary>
    /// Connection string for standalone mode (when UseHostDatabase is false).
    /// Default: "Data Source=mocklab.db"
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=mocklab.db";

    /// <summary>
    /// Automatically run database migrations on startup.
    /// When true, database schema will be created/updated automatically.
    /// When false, you must run migrations manually using 'dotnet ef database update'.
    /// Default: true
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Seed sample mock data on first run.
    /// Useful for testing and demonstration purposes.
    /// Default: false
    /// </summary>
    public bool SeedSampleData { get; set; } = false;

    /// <summary>
    /// Route prefix for mock endpoints.
    /// Consumers can set this to serve mock responses under a custom path prefix.
    /// Example: "mock" means requests to /mock/api/users will match mock route /api/users.
    /// Default: "" (no prefix, catches all non-admin routes)
    /// </summary>
    public string RoutePrefix { get; set; } = "";

    /// <summary>
    /// Route prefix for admin endpoints.
    /// All admin API endpoints will be available under this prefix.
    /// Default: "_admin"
    /// </summary>
    public string AdminRoutePrefix { get; set; } = "_admin";

    /// <summary>
    /// Enable or disable the frontend UI.
    /// When true, the React admin UI will be served at /_admin route.
    /// Default: true
    /// </summary>
    public bool EnableUI { get; set; } = true;

    /// <summary>
    /// Database provider type for host database mode.
    /// Options: "sqlite", "sqlserver", "postgresql"
    /// Only used when UseHostDatabase is true.
    /// Default: "sqlserver"
    /// </summary>
    public string DatabaseProvider { get; set; } = "sqlserver";
}
