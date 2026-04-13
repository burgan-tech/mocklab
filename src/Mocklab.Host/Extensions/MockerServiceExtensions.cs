using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mocklab.Host.Data;
using Mocklab.Host.Services;

namespace Mocklab.Host.Extensions;

/// <summary>
/// Extension methods for configuring Mocklab services
/// </summary>
public static class MocklabServiceExtensions
{
    /// <summary>
    /// Adds Mocklab API services to the dependency injection container.
    /// Reads options from the "Mocklab" section of <paramref name="configuration"/>.
    /// An optional <paramref name="configure"/> action can override individual values after binding.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration (used to bind the "Mocklab" section)</param>
    /// <param name="configure">Optional action to override individual option values after config binding</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMocklab(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MocklabOptions>? configure = null)
    {
        // Bind options from "Mocklab" config section, then apply any caller overrides
        var options = new MocklabOptions();
        configuration.GetSection("Mocklab").Bind(options);

        // Fallback: if ConnectionString not set in "Mocklab" section, try ConnectionStrings
        if (string.IsNullOrEmpty(options.ConnectionString))
            options.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=mocklab.db";

        configure?.Invoke(options);

        // Register options so the rest of the app can resolve IOptions<MocklabOptions>
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
            opts.SeedDirectory = options.SeedDirectory;
        });

        services.Configure<MocklabDbOptions>(opts =>
        {
            opts.SchemaName = options.SchemaName;
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
                        dbOptions.UseSqlite(connectionString,
                            x => x.MigrationsAssembly("Mocklab.Migrations.Sqlite"));
                        break;
                    case "postgresql":
                    case "postgres":
                        dbOptions.UseNpgsql(connectionString,
                            x => x.MigrationsAssembly("Mocklab.Migrations.PostgreSql"));
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
                dbOptions.UseSqlite(options.ConnectionString,
                    x => x.MigrationsAssembly("Mocklab.Migrations.Sqlite"));
            });
        }

        // Register business services
        services.AddHttpClient();
        services.AddScoped<IMockImportService, MockImportService>();
        services.AddScoped<IJsonSeedImporter, JsonSeedImporter>();
        services.AddSingleton<ITemplateProcessor, ScribanTemplateProcessor>();
        services.AddSingleton<IRuleEvaluator, RuleEvaluator>();
        services.AddSingleton<ISequenceStateManager, SequenceStateManager>();

        // Register Mocklab controllers explicitly (avoids full assembly scanning)
        services.AddControllers(mvcOptions =>
            {
                if (!string.IsNullOrEmpty(options.RoutePrefix))
                {
                    mvcOptions.Conventions.Add(new MocklabRoutePrefixConvention(options.RoutePrefix));
                }
            })
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new MocklabControllerFeatureProvider());
            });

        return services;
    }
}
