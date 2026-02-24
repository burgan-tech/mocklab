using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Mocklab.Host.Data;

namespace Mocklab.Host;

/// <summary>
/// Design-time factory used by "dotnet ef migrations" commands.
/// Set MOCKLAB_DB_PROVIDER environment variable to select the target provider:
///   sqlite (default), postgresql
/// </summary>
public class MocklabDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MocklabDbContext>
{
    public MocklabDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("MOCKLAB_DB_PROVIDER") ?? "sqlite";
        var optionsBuilder = new DbContextOptionsBuilder<MocklabDbContext>();

        switch (provider.ToLowerInvariant())
        {
            case "postgresql":
            case "postgres":
                optionsBuilder.UseNpgsql("Host=localhost;Database=mocklab_design",
                    x => x.MigrationsAssembly("Mocklab.Migrations.PostgreSql"));
                break;
            default:
                optionsBuilder.UseSqlite("Data Source=mocklab_design.db",
                    x => x.MigrationsAssembly("Mocklab.Migrations.Sqlite"));
                break;
        }

        return new MocklabDbContext(optionsBuilder.Options);
    }
}
