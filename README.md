# Mocklab

Lightweight, embeddable API mocking for .NET. Run it standalone or drop it into any ASP.NET Core project with two lines of code.

```csharp
builder.Services.AddMocklab();
app.UseMocklab();
```

## Features

- All HTTP methods (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
- Multi-database: SQLite (standalone), PostgreSQL, SQL Server
- Embedded React admin UI at `/_admin/`
- Import mocks from cURL commands or OpenAPI/Swagger specs
- Configurable route prefix to scope mock endpoints
- Schema isolation when sharing the host database
- **Collections** — Group and organize mocks with color-coded categories, import/export
- **Conditional Rules** — Return different responses based on headers, query params, body, method (regex supported)
- **Sequential Responses** — Stateful mock sequences for retry, rate-limit, and progress testing
- **Dynamic Templates** — `{{$randomUUID}}`, `{{$randomName}}`, `{{$request.header.X}}` and more
- **Response Delays** — Simulate latency at mock or sequence-step level
- **Request Logging** — Monitor all incoming requests with match status and response times

## Quick Start

### Standalone

```bash
git clone https://github.com/user/mocklab.git
cd mocklab
dotnet run --project src/Mocklab.App
```

Open `http://localhost:5000/_admin/` for the admin UI.

### Integrate into Your Project

```bash
dotnet add reference path/to/Mocklab.App.csproj
```

```csharp
builder.Services.AddMocklab(options =>
{
    options.RoutePrefix = "mock";        // Serve mocks under /mock/...
    options.SeedSampleData = true;
});
app.UseMocklab();
```

Now `GET /mock/api/users` returns mock responses, and `/_admin/` opens the management UI.

> **RoutePrefix** is important when integrating: without it, the catch-all handler intercepts all non-admin routes in your application. Set a prefix like `"mock"` to keep your own controllers unaffected.

### Docker

```bash
docker build -t mocklab:latest .
docker run -d --name mocklab -p 8080:5000 mocklab:latest
```

## Documentation

| Guide | Description |
|---|---|
| [Getting Started](docs/getting-started.md) | Local development, debugging, frontend build, migrations |
| [Docker](docs/docker.md) | Building images, running containers, Docker Compose with PostgreSQL |
| [Integration](docs/integration.md) | Adding Mocklab to an existing project, configuration options, examples |
| [API Reference](docs/api-reference.md) | Admin API endpoints, mock response structure, route matching logic |

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core, Entity Framework Core 10 |
| Database | SQLite, PostgreSQL, SQL Server |
| Frontend | React 19, PrimeReact 10, Vite 7 |

## License

MIT
