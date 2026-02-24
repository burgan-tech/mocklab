#!/bin/bash
# Usage: ./add-migration.sh <MigrationName>
# Generates EF Core migrations for all supported database providers.

set -e

MIGRATION_NAME=$1

if [ -z "$MIGRATION_NAME" ]; then
  echo "Usage: ./add-migration.sh <MigrationName>"
  echo "Example: ./add-migration.sh AddNewFeature"
  exit 1
fi

echo "=== Generating SQLite migration: $MIGRATION_NAME ==="
dotnet ef migrations add "$MIGRATION_NAME" \
  --project src/Mocklab.Migrations.Sqlite \
  --startup-project src/Mocklab.App

echo ""
echo "=== Generating PostgreSQL migration: $MIGRATION_NAME ==="
MOCKLAB_DB_PROVIDER=postgresql dotnet ef migrations add "$MIGRATION_NAME" \
  --project src/Mocklab.Migrations.PostgreSql \
  --startup-project src/Mocklab.App

echo ""
echo "Done! Migrations generated for SQLite and PostgreSQL."
