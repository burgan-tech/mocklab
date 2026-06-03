# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What Mocklab is

Lightweight, embeddable API mocking for .NET 10 / ASP.NET Core. It ships two ways from the **same** `Mocklab.Host` project:

- **Standalone app / Docker** — built as an `Exe`, run directly.
- **NuGet library** — consumers call `builder.Services.AddMocklab(config)` + `app.UseMocklab()`.

The `IsPackaging` MSBuild property toggles between these: when `true`, `OutputType` becomes `Library` and `Program.cs` is excluded from compilation (see `Mocklab.Host.csproj`). Keep `Program.cs` free of logic that library consumers need — all reusable wiring lives in the `Extensions/` folder.

## Commands

```bash
# Run standalone (SQLite, serves admin UI at http://localhost:5000/_admin/)
dotnet run --project src/Mocklab.Host

# Build / restore the whole solution
dotnet build Mocklab.slnx
dotnet restore Mocklab.slnx

# Frontend (React/Vite) — run from src/Mocklab.Host/frontend
npm install
npm run dev      # Vite dev server (proxies to backend; CORS allows :3000 and :5173)
npm run build    # outputs to wwwroot/_mocklab — embedded into the assembly on dotnet build
npm run lint     # eslint

# Docker
docker build -t mocklab:latest .
docker run -d -p 8080:5000 mocklab:latest
```

There is **no test project** in this repo. `MocklabApi.http` (root) and per-feature `.http` files are the manual API exercise harness.

## Database migrations (critical — multi-provider)

Each provider has its **own** migrations assembly so column types stay native (`Mocklab.Migrations.Sqlite`, `Mocklab.Migrations.PostgreSql`). A migration generated for one provider must **never** be the only one updated — always regenerate for both:

```bash
./add-migration.sh <MigrationName>   # generates SQLite AND PostgreSQL migrations
```

The script selects the provider via the `MOCKLAB_DB_PROVIDER` env var, read by `MocklabDesignTimeDbContextFactory` (`sqlite` default, `postgresql`). SQL Server has no migrations assembly — it relies on runtime model creation. Migrations apply automatically on startup when `AutoMigrate` is true (default); failures are logged as warnings and do **not** crash the app.

## Architecture

### Request flow (the heart of the system)
`CatchAllController` (`Controllers/CatchAllController.cs`) has a single `{**catchAll}` route bound to every HTTP method. For each request it:
1. Strips `RoutePrefix`, then ignores `/_admin`, `/openapi`, `/swagger`.
2. `FindMatchingMockResponse` resolves a `MockResponse` by precedence: **exact route → parametric (`/api/users/{id}`) → substring fallback**, preferring body-matching candidates within each tier.
3. Chooses the response in priority order: **sequential (`SequenceStateManager`) → conditional rule (`RuleEvaluator`) → default mock body**. Sequential and rules are mutually exclusive (sequential wins).
4. Applies optional delay, runs the body and rule-header values through `ITemplateProcessor` (Scriban), returns a raw `ContentResult` (never re-serialized — avoids a prior 204 bug), and logs to `RequestLog`.

### Routing model (how the catch-all coexists with the host app)
- `MocklabControllerFeatureProvider` registers Mocklab's controllers **explicitly by type** rather than assembly-scanning, so dropping the library into a consumer project never triggers `ReflectionTypeLoadException`.
- `MocklabRoutePrefixConvention` rewrites the catch-all route template to `{prefix}/{**catchAll}` when `RoutePrefix` is set. Without a prefix, the catch-all intercepts **all** non-admin routes — this is why integration docs stress setting `RoutePrefix`.

### Project layout
| Project | Role |
|---|---|
| `Mocklab.Host` | Everything: controllers, services, middleware, DI/middleware extensions, embedded React UI. Built as Exe or Library. |
| `Mocklab.Data` | `MocklabDbContext` + entity models. Note: its `RootNamespace` is `Mocklab.Host`, so models live in the `Mocklab.Host.Models` namespace despite being in this project. |
| `Mocklab.Migrations.Sqlite` / `Mocklab.Migrations.PostgreSql` | Provider-specific EF Core migrations only. |

`Mocklab.App/` is **not** in the solution (`Mocklab.slnx`) — treat it as legacy; do not edit it.

### Configuration
`AddMocklab` binds the `Mocklab` config section to `MocklabOptions` (`Extensions/MockerOptions.cs`), then applies an optional override action. Key options: `UseHostDatabase`, `DatabaseProvider` (`sqlite`/`postgresql`/`sqlserver`), `SchemaName` (table isolation when sharing a host DB), `RoutePrefix`, `EnableUI`, `SeedSampleData`, `SeedDirectory`. Connection-string fallback chain: `Mocklab:ConnectionString` → `ConnectionStrings:DefaultConnection` → `Data Source=mocklab.db`. Env-var overrides use the `Mocklab__` prefix (e.g. `Mocklab__SeedDirectory`).

### Seeding
Two independent mechanisms in `UseMocklab` (`MocklabApplicationExtensions.cs`):
- `SeedSampleData` — inserts hardcoded sample mocks, only when the table is empty.
- `SeedDirectory` — recursively imports `*.json` collection-export files via `IJsonSeedImporter` on startup; already-imported collections are skipped (idempotent). This is the Docker volume seed path (`/app/seed`).

### Frontend UI
React 19 + PrimeReact + Vite, built into `wwwroot/_mocklab/` and embedded as assembly resources. `MocklabStaticFilesMiddleware` serves them under `/_admin/*` with SPA fallback to `index.html`, while passing `/_admin/{mocks,logs,collections,folders}` API calls through to the admin controllers. `dotnet build` auto-runs the frontend build via the `BuildFrontend` MSBuild target (best-effort, `ContinueOnError`).

### Admin API
`*AdminController`s under `/_admin/...` are the CRUD + import surface for the data model: `MockResponse` (with `MockResponseRule`, `MockResponseSequenceItem`), `MockCollection`, `MockFolder`, `DataBucket`, `RequestLog`. `MockImportService` + `CurlParser` import from cURL and OpenAPI specs. Response headers for rules are stored generically in `KeyValueEntry` keyed by `(OwnerType, OwnerId)`.

## Conventions
- Target framework is `net10.0`; SDK pinned via `global.json` (10.0.101, `latestFeature` roll-forward).
- Health endpoint is `/health` (used by the Docker `HEALTHCHECK` and k8s probes).
- CI publishes Docker image (GHCR) + NuGet package on push to `release-v*` branches (`.github/workflows/build-and-publish.yml`).
