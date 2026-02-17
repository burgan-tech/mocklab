# Docker

Mocklab ships with a multi-stage Dockerfile that builds both the React frontend and the .NET backend in a single image.

## Building the Image

From the project root (where `Mocklab.slnx` is located):

```bash
docker build -t mocklab:latest .
```

The build stages:
1. **frontend-build** — Installs npm dependencies and runs `npm run build`
2. **build** — Restores NuGet packages, copies frontend output, compiles the .NET app
3. **publish** — Creates a release publish
4. **final** — Minimal ASP.NET runtime image with non-root user

## Running with Docker

### Basic

```bash
docker run -d --name mocklab -p 8080:5000 mocklab:latest
```

The application is available at `http://localhost:8080`.

### With Custom Configuration

Override settings via environment variables using the `__` (double underscore) notation:

```bash
docker run -d \
  --name mocklab \
  -p 8080:5000 \
  -e Mocklab__SeedSampleData=true \
  -e Mocklab__EnableUI=true \
  mocklab:latest
```

### With Persistent Data

Mount a volume to preserve the SQLite database across container restarts:

```bash
docker run -d \
  --name mocklab \
  -p 8080:5000 \
  -v mocklab-data:/app \
  mocklab:latest
```

### Container Management

```bash
# View logs
docker logs -f mocklab

# Stop
docker stop mocklab

# Remove
docker rm mocklab
```

## Docker Compose with PostgreSQL

A ready-to-use `docker-compose.yml` is provided in the `docs/` directory. It starts Mocklab with a PostgreSQL 17 database.

```bash
cd docs
docker compose up -d
```

This starts:

| Service | URL |
|---|---|
| Mocklab | `http://localhost:8080` |
| PostgreSQL | `localhost:5432` |

### Configuration

All Mocklab settings are passed as environment variables:

```yaml
services:
  mocklab:
    build:
      context: ..
      dockerfile: Dockerfile
    ports:
      - "8080:5000"
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=mocklab;Username=postgres;Password=postgres"
      Mocklab__UseHostDatabase: "true"
      Mocklab__DatabaseProvider: "postgresql"
      Mocklab__SchemaName: "mocklab"
      Mocklab__AutoMigrate: "true"
      Mocklab__SeedSampleData: "true"
      Mocklab__EnableUI: "true"
    depends_on:
      postgres:
        condition: service_healthy
```

### Environment Variable Mapping

The `__` notation maps to `:` in `appsettings.json`:

| Environment Variable | appsettings.json Equivalent |
|---|---|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `Mocklab__UseHostDatabase` | `Mocklab:UseHostDatabase` |
| `Mocklab__DatabaseProvider` | `Mocklab:DatabaseProvider` |
| `Mocklab__SchemaName` | `Mocklab:SchemaName` |
| `Mocklab__RoutePrefix` | `Mocklab:RoutePrefix` |

### Useful Commands

```bash
# Start services
docker compose up -d

# View Mocklab logs
docker compose logs -f mocklab

# Stop services
docker compose down

# Stop and reset database
docker compose down -v
```

### Using a Pre-built Image

If you already have a built image or are pulling from a registry, replace the `build` section with `image`:

```yaml
services:
  mocklab:
    image: mocklab:latest
    ports:
      - "8080:5000"
    environment:
      Mocklab__SeedSampleData: "true"
    restart: unless-stopped
```
