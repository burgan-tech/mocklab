using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mocklab.App.Data;

namespace Mocklab.App.Extensions;

/// <summary>
/// Extension methods for configuring Mocklab services
/// </summary>
public static class MocklabServiceExtensions
{
    /// <summary>
    /// Adds Mocklab API services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMocklab(
        this IServiceCollection services,
        Action<MocklabOptions>? configure = null)
    {
        // Register and configure options
        var options = new MocklabOptions();
        configure?.Invoke(options);
        
        services.Configure<MocklabOptions>(opts =>
        {
            opts.UseHostDatabase = options.UseHostDatabase;
            opts.SchemaName = options.SchemaName;
            opts.ConnectionString = options.ConnectionString;
            opts.AutoMigrate = options.AutoMigrate;
            opts.SeedSampleData = options.SeedSampleData;
            opts.RoutePrefix = options.RoutePrefix;
            opts.AdminRoutePrefix = options.AdminRoutePrefix;
            opts.EnableUI = options.EnableUI;
            opts.DatabaseProvider = options.DatabaseProvider;
        });

        // Register DbContext
        if (options.UseHostDatabase)
        {
            // Use host application's database connection
            // The host app should have already registered a DbContext
            // We'll create our own DbContext that uses the same connection
            services.AddDbContext<MocklabDbContext>((serviceProvider, dbOptions) =>
            {
                var configuration = serviceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                switch (options.DatabaseProvider.ToLowerInvariant())
                {
                    case "sqlite":
                        dbOptions.UseSqlite(connectionString);
                        break;
                    case "postgresql":
                    case "postgres":
                        dbOptions.UseNpgsql(connectionString);
                        break;
                    case "sqlserver":
                    default:
                        dbOptions.UseSqlServer(connectionString);
                        break;
                }
            });
        }
        else
        {
            // Standalone mode - use SQLite with own connection string
            services.AddDbContext<MocklabDbContext>(dbOptions =>
            {
                dbOptions.UseSqlite(options.ConnectionString);
            });
        }

        // Register controllers from this assembly
        services.AddControllers(mvcOptions =>
            {
                if (!string.IsNullOrEmpty(options.RoutePrefix))
                {
                    mvcOptions.Conventions.Add(new MocklabRoutePrefixConvention(options.RoutePrefix));
                }
            })
            .AddApplicationPart(typeof(MocklabServiceExtensions).Assembly);

        return services;
    }
}
