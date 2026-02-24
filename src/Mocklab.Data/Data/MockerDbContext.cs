using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Mocklab.App.Models;

namespace Mocklab.App.Data;

public class MocklabDbContext : DbContext
{
    private readonly string _schemaName;

    public MocklabDbContext(DbContextOptions<MocklabDbContext> options) : base(options)
    {
        _schemaName = "mocklab";
    }

    public MocklabDbContext(
        DbContextOptions<MocklabDbContext> options, 
        IOptions<MocklabDbOptions> dbOptions) : base(options)
    {
        _schemaName = dbOptions.Value.SchemaName ?? "mocklab";
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
    public DbSet<DataBucket> DataBuckets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MockResponse>(entity =>
        {
            entity.ToTable("MockResponses", _schemaName);

            entity.HasIndex(m => new { m.HttpMethod, m.Route, m.IsActive })
                .HasDatabaseName("IX_MockResponses_HttpMethod_Route_IsActive");
        });

        modelBuilder.Entity<MockCollection>(entity =>
        {
            entity.ToTable("MockCollections", _schemaName);

            entity.HasIndex(c => c.Name)
                .HasDatabaseName("IX_MockCollections_Name");

            entity.HasMany(c => c.MockResponses)
                .WithOne()
                .HasForeignKey(m => m.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(c => c.DataBuckets)
                .WithOne(d => d.Collection)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DataBucket>(entity =>
        {
            entity.ToTable("DataBuckets", _schemaName);

            entity.HasIndex(d => new { d.CollectionId, d.Name })
                .HasDatabaseName("IX_DataBuckets_CollectionId_Name");
        });

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

        modelBuilder.Entity<MockResponse>()
            .HasOne<MockFolder>()
            .WithMany(f => f.MockResponses)
            .HasForeignKey(m => m.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.ToTable("RequestLogs", _schemaName);

            entity.HasIndex(r => r.Timestamp)
                .HasDatabaseName("IX_RequestLogs_Timestamp");

            entity.HasIndex(r => new { r.HttpMethod, r.IsMatched })
                .HasDatabaseName("IX_RequestLogs_HttpMethod_IsMatched");
        });

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

        modelBuilder.Entity<KeyValueEntry>(entity =>
        {
            entity.ToTable("KeyValueEntries", _schemaName);

            entity.HasIndex(k => new { k.OwnerType, k.OwnerId })
                .HasDatabaseName("IX_KeyValueEntries_OwnerType_OwnerId");
        });
    }
}
