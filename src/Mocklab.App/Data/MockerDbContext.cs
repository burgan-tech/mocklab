using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Mocklab.App.Extensions;
using Mocklab.App.Models;

namespace Mocklab.App.Data;

public class MocklabDbContext : DbContext
{
    private readonly string _schemaName;

    public MocklabDbContext(DbContextOptions<MocklabDbContext> options) : base(options)
    {
        _schemaName = "mocklab"; // Default schema name
    }

    public MocklabDbContext(
        DbContextOptions<MocklabDbContext> options, 
        IOptions<MocklabOptions> mocklabOptions) : base(options)
    {
        _schemaName = mocklabOptions.Value.SchemaName ?? "mocklab";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<MockResponse> MockResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MockResponse entity with schema support
        modelBuilder.Entity<MockResponse>(entity =>
        {
            // Use schema name (important for multi-tenant or host database scenarios)
            entity.ToTable("MockResponses", _schemaName);

            // Add indexes for fast searching
            entity.HasIndex(m => new { m.HttpMethod, m.Route, m.IsActive })
                .HasDatabaseName("IX_MockResponses_HttpMethod_Route_IsActive");
        });

        // Note: Seed data removed - will be handled by MocklabApplicationExtensions
        // This allows for more flexible seeding based on runtime configuration
    }
}
