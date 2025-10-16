#!/bin/bash

# Database migration script for Render deployment
echo "Starting database migration..."

# Wait for database to be ready
echo "Waiting for database connection..."
sleep 10

# Run migrations
echo "Running Entity Framework migrations..."
dotnet ef database update --verbose

echo "Database migration completed!"
