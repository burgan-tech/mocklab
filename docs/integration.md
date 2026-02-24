# Integration Guide

Add Mocklab to any existing ASP.NET Core application — just like Swagger.

## Setup

### 1. Add the project reference

```bash
dotnet add reference path/to/Mocklab.Host.csproj
```

### 2. Configure services in `Program.cs`

```csharp
using Mocklab.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Your existing services
builder.Services.AddControllers();

// Add Mocklab
builder.Services.AddMocklab(options =>
{
    builder.Configuration.GetSection("Mocklab").Bind(options);
});

var app = builder.Build();

// Enable Mocklab middleware
app.UseMocklab();

app.MapControllers();
app.Run();
```

### 3. Add configuration to `appsettings.json`

```json
{
  "Mocklab": {
    "UseHostDatabase": false,
    "AutoMigrate": true,
    "SeedSampleData": true,
    "EnableUI": true,
    "RoutePrefix": "mock"
  }
}
```

That's it. Your mock endpoints are now available under `/mock/...` and the admin UI at `/_admin/`.

## Route Prefix

The `RoutePrefix` option controls where mock endpoints are served:

| RoutePrefix | Mock URL | Matches Route |
|---|---|---|
| `""` (default) | `GET /api/users` | `/api/users` |
| `"mock"` | `GET /mock/api/users` | `/api/users` |
| `"test-api"` | `GET /test-api/api/users` | `/api/users` |

**Important:** When `RoutePrefix` is empty, the catch-all handler intercepts **all** non-admin routes in your application. This means your own controllers may not be reachable. Always set a prefix when integrating into a project that has its own routes.

## Configuration Reference

All options can be set in code or via `appsettings.json`:

| Option | Type | Default | Description |
|---|---|---|---|
| `UseHostDatabase` | `bool` | `false` | Use the host application's database instead of standalone SQLite |
| `DatabaseProvider` | `string` | `"sqlserver"` | Database provider: `"sqlite"`, `"postgresql"`, or `"sqlserver"` |
| `SchemaName` | `string` | `"mocklab"` | Schema name for PostgreSQL / SQL Server (isolates Mocklab tables) |
| `ConnectionString` | `string` | `"Data Source=mocklab.db"` | Connection string for standalone mode |
| `AutoMigrate` | `bool` | `true` | Automatically apply database migrations on startup |
| `SeedSampleData` | `bool` | `false` | Seed sample mock data if the database is empty |
| `RoutePrefix` | `string` | `""` | Route prefix for mock endpoints (e.g. `"mock"` serves at `/mock/...`) |
| `AdminRoutePrefix` | `string` | `"_admin"` | Route prefix for admin API endpoints |
| `EnableUI` | `bool` | `true` | Enable the embedded React admin UI at `/_admin/` |

## Database Modes

### Standalone SQLite (Default)

No external database required. Mocklab creates its own SQLite file:

```csharp
builder.Services.AddMocklab(options =>
{
    options.UseHostDatabase = false;
    options.ConnectionString = "Data Source=mocklab.db";
});
```

### Host Database — PostgreSQL

Uses your application's PostgreSQL database with a separate schema:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=myapp;Username=myuser;Password=mypassword"
  },
  "Mocklab": {
    "UseHostDatabase": true,
    "DatabaseProvider": "postgresql",
    "SchemaName": "mocklab"
  }
}
```

This creates:
```sql
CREATE SCHEMA mocklab;
CREATE TABLE mocklab."MockResponses" (...);
```

### Host Database — SQL Server

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Mocklab": {
    "UseHostDatabase": true,
    "DatabaseProvider": "sqlserver",
    "SchemaName": "mocklab"
  }
}
```

## Examples

### Minimal

```csharp
builder.Services.AddMocklab();
app.UseMocklab();
```

Standalone SQLite, no prefix, seed data disabled. Good for quick testing.

### Development Environment

```csharp
builder.Services.AddMocklab(options =>
{
    options.RoutePrefix = "mock";
    options.SeedSampleData = true;
    options.EnableUI = true;
});
```

### Production

```csharp
builder.Services.AddMocklab(options =>
{
    options.UseHostDatabase = true;
    options.DatabaseProvider = "postgresql";
    options.SchemaName = "mocklab";
    options.RoutePrefix = "mock";
    options.AutoMigrate = false;
    options.SeedSampleData = false;
    options.EnableUI = false;
});
```

### Per-Environment Configuration (Recommended)

Bind from `appsettings.json` and override per environment:

```csharp
builder.Services.AddMocklab(options =>
{
    builder.Configuration.GetSection("Mocklab").Bind(options);
});
```

**appsettings.Development.json:**
```json
{
  "Mocklab": {
    "SeedSampleData": true,
    "EnableUI": true
  }
}
```

**appsettings.Production.json:**
```json
{
  "Mocklab": {
    "AutoMigrate": false,
    "SeedSampleData": false,
    "EnableUI": false
  }
}
```

## Production Notes

- **Protect admin endpoints.** `/_admin/*` should be secured with authentication.
- **Disable the UI** in production if not needed (`EnableUI = false`).
- **Manual migrations.** Set `AutoMigrate = false` and run migrations in your deployment pipeline.
- **Use a real database.** SQLite is fine for development but use PostgreSQL or SQL Server in production.
- **Always set `RoutePrefix`** when integrating to avoid route conflicts with your application.
