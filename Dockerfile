# Frontend Build Stage
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY src/Mocklab.Host/frontend/package*.json ./frontend/
RUN cd frontend && npm ci
COPY src/Mocklab.Host/frontend/ ./frontend/
RUN cd frontend && npm run build

# .NET Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Mocklab.slnx .
COPY src/Mocklab.Host/Mocklab.Host.csproj src/Mocklab.Host/
COPY src/Mocklab.Data/Mocklab.Data.csproj src/Mocklab.Data/
COPY src/Mocklab.Migrations.Sqlite/Mocklab.Migrations.Sqlite.csproj src/Mocklab.Migrations.Sqlite/
COPY src/Mocklab.Migrations.PostgreSql/Mocklab.Migrations.PostgreSql.csproj src/Mocklab.Migrations.PostgreSql/

# Restore dependencies
RUN dotnet restore Mocklab.slnx

# Copy all source code
COPY src/ src/

# Copy frontend build output into wwwroot
COPY --from=frontend-build /app/wwwroot/_mocklab src/Mocklab.Host/wwwroot/_mocklab/

# Build the application
RUN dotnet build src/Mocklab.Host/Mocklab.Host.csproj -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish src/Mocklab.Host/Mocklab.Host.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user for security
RUN useradd --no-create-home --shell /bin/false appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Mocklab.Host.dll"]
