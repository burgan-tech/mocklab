# Frontend Build Stage
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY src/Mocklab.App/frontend/package*.json ./frontend/
RUN cd frontend && npm ci
COPY src/Mocklab.App/frontend/ ./frontend/
RUN cd frontend && npm run build

# .NET Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Mocklab.slnx .
COPY src/Mocklab.App/Mocklab.App.csproj src/Mocklab.App/

# Restore dependencies
RUN dotnet restore Mocklab.slnx

# Copy all source code
COPY src/ src/

# Copy frontend build output into wwwroot
COPY --from=frontend-build /app/wwwroot/_mocklab src/Mocklab.App/wwwroot/_mocklab/

# Build the application
RUN dotnet build src/Mocklab.App/Mocklab.App.csproj -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish src/Mocklab.App/Mocklab.App.csproj -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "Mocklab.App.dll"]
