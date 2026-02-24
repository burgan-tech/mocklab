# Getting Started

## Prerequisites

- .NET 10.0 SDK
- Node.js 18+ and npm (for frontend development only)

## Project Structure

```
mocklab/
├── Mocklab.slnx                            # Solution file
├── Dockerfile                              # Multi-stage Docker build
├── add-migration.sh                        # Helper: generate migrations for all providers
├── docs/                                   # Documentation & Docker Compose
│   └── docker-compose.yml
└── src/
    ├── Mocklab.Data/                       # Shared data layer (DbContext + entities)
    │   ├── Data/
    │   │   ├── MocklabDbContext.cs
    │   │   └── MocklabDbOptions.cs
    │   └── Models/
    │       ├── MockResponse.cs             # Mock response entity
    │       ├── MockCollection.cs           # Collection entity
    │       ├── MockResponseRule.cs         # Conditional rule entity
    │       ├── MockResponseSequenceItem.cs # Sequence step entity
    │       ├── RequestLog.cs               # Request log entity
    │       ├── DataBucket.cs               # Data bucket entity
    │       ├── KeyValueEntry.cs            # Key-value storage entity
    │       └── MockFolder.cs               # Folder entity
    ├── Mocklab.Migrations.Sqlite/          # SQLite-specific migrations
    │   └── Migrations/
    ├── Mocklab.Migrations.PostgreSql/      # PostgreSQL-specific migrations
    │   └── Migrations/
    └── Mocklab.App/                        # Main application
        ├── Program.cs
        ├── Controllers/
        │   ├── CatchAllController.cs           # Catch-all mock handler
        │   ├── MockAdminController.cs          # Admin CRUD API
        │   ├── CollectionAdminController.cs    # Collection management API
        │   └── RequestLogAdminController.cs    # Request log API
        ├── Data/
        │   └── MocklabDesignTimeDbContextFactory.cs  # EF Core design-time factory
        ├── Extensions/
        │   ├── MocklabOptions.cs               # Configuration options
        │   ├── MocklabServiceExtensions.cs     # AddMocklab()
        │   ├── MocklabApplicationExtensions.cs # UseMocklab()
        │   ├── MocklabRoutePrefixConvention.cs # Dynamic route prefix
        │   └── MocklabStaticFilesMiddleware.cs # Embedded frontend serving
        ├── Models/
        │   ├── Requests/                       # API request DTOs
        │   └── Results/                        # API result DTOs
        ├── Services/
        │   ├── IMockImportService.cs
        │   ├── MockImportService.cs            # cURL & OpenAPI import
        │   ├── TemplateProcessor.cs            # Dynamic template variables
        │   ├── RuleEvaluator.cs                # Conditional rule engine
        │   └── SequenceStateManager.cs         # In-memory sequence state
        └── frontend/                           # React admin UI
            ├── package.json
            ├── vite.config.js
            └── src/
                ├── pages/
                │   ├── MockManagementPage.jsx  # Mock CRUD + Rules + Sequences
                │   ├── CollectionsPage.jsx     # Collection management
                │   └── RequestLogsPage.jsx     # Request log viewer
                └── services/
```

## Running Locally

### Backend

```bash
dotnet restore
dotnet run --project src/Mocklab.App
```

The application starts on `http://localhost:5000` by default.
The SQLite database (`mocklab.db`) is created automatically on first run.

### Frontend (Development)

For working on the admin UI with hot reload:

```bash
cd src/Mocklab.App/frontend
npm install
npm run dev
```

The dev server starts at `http://localhost:3000` and proxies API calls to the backend.

### Building Frontend for Embedding

The React app is embedded into the .NET DLL as static files:

```bash
cd src/Mocklab.App/frontend
npm run build
```

Output goes to `wwwroot/_mocklab/` and is served automatically by the middleware at `/_admin/`.

## VS Code Debugging

The project includes pre-configured launch configurations:

| Configuration | Environment | Database | Port |
|---|---|---|---|
| Launch Mocklab API | Test | SQLite | `http://localhost:7070` |
| Launch Mocklab API (PostgreSQL) | Postgres | PostgreSQL | `http://localhost:7070` |

The `build` task runs `npm run build` for the frontend before compiling the .NET project.

## Database Management

### Reset Database

Delete the SQLite file and restart — it will be recreated with seed data (if `SeedSampleData` is enabled):

```bash
rm src/Mocklab.App/mocklab.db
dotnet run --project src/Mocklab.App
```

### Multi-Provider Migrations

Mocklab uses **separate migration assemblies** per database provider. Each provider gets its own migration project with native column types (e.g. `boolean` for PostgreSQL, `INTEGER` for SQLite), ensuring full compatibility.

| Project | Provider | Column Types |
|---|---|---|
| `Mocklab.Migrations.Sqlite` | SQLite | `INTEGER`, `TEXT` |
| `Mocklab.Migrations.PostgreSql` | PostgreSQL | `boolean`, `integer`, `text`, `timestamp with time zone` |

### Creating Migrations

Use the helper script to generate migrations for **all providers** at once:

```bash
./add-migration.sh <MigrationName>
```

Or run them individually:

```bash
# SQLite
dotnet ef migrations add <MigrationName> \
  --project src/Mocklab.Migrations.Sqlite \
  --startup-project src/Mocklab.App

# PostgreSQL
MOCKLAB_DB_PROVIDER=postgresql dotnet ef migrations add <MigrationName> \
  --project src/Mocklab.Migrations.PostgreSql \
  --startup-project src/Mocklab.App
```

The `MOCKLAB_DB_PROVIDER` environment variable tells the design-time factory which provider to use. It defaults to `sqlite` when not set.

### Applying Migrations

Migrations are applied automatically on startup when `AutoMigrate` is enabled (default). The correct migration assembly is selected based on the configured `DatabaseProvider`.

## Next Steps

- [Docker deployment](docker.md)
- [Integrate into your project](integration.md)
- [API Reference](api-reference.md)
