using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mocklab.App.Data;
using Mocklab.App.Models;

namespace Mocklab.App.Extensions;

/// <summary>
/// Extension methods for configuring Mocklab middleware
/// </summary>
public static class MocklabApplicationExtensions
{
    /// <summary>
    /// Adds Mocklab middleware to the application pipeline.
    /// Handles database migration and optional frontend UI serving.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseMocklab(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MocklabDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<MocklabOptions>>().Value;

        // Handle database migration
        if (options.AutoMigrate)
        {
            try
            {
                // Ensure database and schema are created
                dbContext.Database.Migrate();
                
                // For SQL Server and PostgreSQL, ensure schema exists
                if (options.UseHostDatabase && 
                    (options.DatabaseProvider.ToLowerInvariant() == "sqlserver" || 
                     options.DatabaseProvider.ToLowerInvariant() == "postgresql"))
                {
                    EnsureSchemaExists(dbContext, options.SchemaName);
                }
            }
            catch (Exception ex)
            {
                // Log warning but don't crash the application
                Console.WriteLine($"Warning: Mocklab database migration failed: {ex.Message}");
                Console.WriteLine("You may need to run 'dotnet ef database update' manually.");
            }
        }

        // Seed sample data if requested and database is empty
        if (options.SeedSampleData && !dbContext.MockResponses.Any())
        {
            SeedSampleData(dbContext);
        }

        // Enable frontend UI if configured
        if (options.EnableUI)
        {
            app.UseMocklabUI();
        }

        return app;
    }

    /// <summary>
    /// Ensures the database schema exists for SQL Server and PostgreSQL
    /// </summary>
    private static void EnsureSchemaExists(MocklabDbContext dbContext, string schemaName)
    {
        var connection = dbContext.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        
        if (!wasOpen)
            connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            
            // Check database provider and create schema accordingly
            var provider = dbContext.Database.ProviderName;
            
            if (provider?.Contains("SqlServer") == true)
            {
                // SQL Server
                command.CommandText = $@"
                    IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{schemaName}')
                    BEGIN
                        EXEC('CREATE SCHEMA [{schemaName}]')
                    END";
            }
            else if (provider?.Contains("Npgsql") == true || provider?.Contains("PostgreSQL") == true)
            {
                // PostgreSQL
                command.CommandText = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"";
            }
            
            if (!string.IsNullOrEmpty(command.CommandText))
            {
                command.ExecuteNonQuery();
            }
        }
        finally
        {
            if (!wasOpen)
                connection.Close();
        }
    }

    /// <summary>
    /// Seeds sample mock data for testing and demonstration
    /// </summary>
    private static void SeedSampleData(MocklabDbContext dbContext)
    {
        var sampleMocks = new[]
        {
            new MockResponse
            {
                HttpMethod = "GET",
                Route = "/api/users",
                StatusCode = 200,
                ResponseBody = @"{""users"": [{""id"": 1, ""name"": ""John Doe""}, {""id"": 2, ""name"": ""Jane Smith""}]}",
                ContentType = "application/json",
                Description = "User list",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new MockResponse
            {
                HttpMethod = "GET",
                Route = "/api/users/{id}",
                StatusCode = 200,
                ResponseBody = @"{""id"": 1, ""name"": ""John Doe"", ""email"": ""john@example.com""}",
                ContentType = "application/json",
                Description = "Single user details",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new MockResponse
            {
                HttpMethod = "POST",
                Route = "/api/users",
                StatusCode = 201,
                ResponseBody = @"{""id"": 3, ""name"": ""New User"", ""message"": ""User created successfully""}",
                ContentType = "application/json",
                Description = "Create new user",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new MockResponse
            {
                HttpMethod = "GET",
                Route = "/api/products",
                QueryString = "?category=electronics",
                StatusCode = 200,
                ResponseBody = @"{""products"": [{""id"": 1, ""name"": ""Laptop"", ""category"": ""electronics""}]}",
                ContentType = "application/json",
                Description = "Product list by category",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        dbContext.MockResponses.AddRange(sampleMocks);
        dbContext.SaveChanges();
        
        Console.WriteLine($"Mocklab: Seeded {sampleMocks.Length} sample mock responses.");
    }

    /// <summary>
    /// Enables the Mocklab frontend UI (embedded React application)
    /// </summary>
    private static IApplicationBuilder UseMocklabUI(this IApplicationBuilder app)
    {
        // Use custom middleware to serve embedded frontend
        app.UseMiddleware<MocklabStaticFilesMiddleware>();
        
        return app;
    }
}
