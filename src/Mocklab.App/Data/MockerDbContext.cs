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
    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<MockCollection> MockCollections { get; set; }
    public DbSet<MockFolder> MockFolders { get; set; }
    public DbSet<MockResponseRule> MockResponseRules { get; set; }
    public DbSet<MockResponseSequenceItem> MockResponseSequenceItems { get; set; }
    public DbSet<KeyValueEntry> KeyValueEntries { get; set; }

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

        // Configure MockCollection entity
        modelBuilder.Entity<MockCollection>(entity =>
        {
            entity.ToTable("MockCollections", _schemaName);

            entity.HasIndex(c => c.Name)
                .HasDatabaseName("IX_MockCollections_Name");

            entity.HasMany(c => c.MockResponses)
                .WithOne()
                .HasForeignKey(m => m.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure MockFolder entity (folders within a collection)
        modelBuilder.Entity<MockFolder>(entity =>
        {
            entity.ToTable("MockFolders", _schemaName);

            entity.HasIndex(f => f.CollectionId)
                .HasDatabaseName("IX_MockFolders_CollectionId");

            entity.HasOne(f => f.Collection)
                .WithMany(c => c.Folders)
                .HasForeignKey(f => f.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.ParentFolder)
                .WithMany(f => f.Children)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MockResponse -> MockFolder (optional; set null when folder is deleted)
        modelBuilder.Entity<MockResponse>()
            .HasOne<MockFolder>()
            .WithMany(f => f.MockResponses)
            .HasForeignKey(m => m.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure RequestLog entity
        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.ToTable("RequestLogs", _schemaName);

            entity.HasIndex(r => r.Timestamp)
                .HasDatabaseName("IX_RequestLogs_Timestamp");

            entity.HasIndex(r => new { r.HttpMethod, r.IsMatched })
                .HasDatabaseName("IX_RequestLogs_HttpMethod_IsMatched");
        });

        // Configure MockResponseRule entity (conditional response rules)
        modelBuilder.Entity<MockResponseRule>(entity =>
        {
            entity.ToTable("MockResponseRules", _schemaName);

            entity.HasOne(r => r.MockResponse)
                .WithMany(m => m.Rules)
                .HasForeignKey(r => r.MockResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => new { r.MockResponseId, r.Priority })
                .HasDatabaseName("IX_MockResponseRules_MockResponseId_Priority");
        });

        // Configure MockResponseSequenceItem entity (sequential mock responses)
        modelBuilder.Entity<MockResponseSequenceItem>(entity =>
        {
            entity.ToTable("MockResponseSequenceItems", _schemaName);

            entity.HasOne(s => s.MockResponse)
                .WithMany(m => m.SequenceItems)
                .HasForeignKey(s => s.MockResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => new { s.MockResponseId, s.Order })
                .HasDatabaseName("IX_MockResponseSequenceItems_MockResponseId_Order");
        });

        // Configure KeyValueEntry (generic key-value for MockResponseRule headers, etc.)
        modelBuilder.Entity<KeyValueEntry>(entity =>
        {
            entity.ToTable("KeyValueEntries", _schemaName);

            entity.HasIndex(k => new { k.OwnerType, k.OwnerId })
                .HasDatabaseName("IX_KeyValueEntries_OwnerType_OwnerId");
        });

        // Note: Seed data removed - will be handled by MocklabApplicationExtensions
        // This allows for more flexible seeding based on runtime configuration
    }
}
