# PowerShell deployment script for local testing
Write-Host "Starting deployment preparation..." -ForegroundColor Green

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Run tests (if any)
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test

# Publish the project
Write-Host "Publishing project..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Deployment preparation completed successfully!" -ForegroundColor Green
Write-Host "You can now deploy the contents of the 'publish' folder to Render." -ForegroundColor Cyan
