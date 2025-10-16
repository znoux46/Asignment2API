# Products Management API

## Getting Started

### Prerequisites
1. .NET 8 SDK
2. PostgreSQL database

### Configuration (Never commit secrets)

Do NOT store secrets in `appsettings.json`. Use environment variables or a secrets manager.

Recommended environment variables (Windows PowerShell examples):

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require"
$env:CloudinarySettings__CloudName = "your_cloud_name"
$env:CloudinarySettings__ApiKey = "your_api_key"
$env:CloudinarySettings__ApiSecret = "your_api_secret"
$env:Jwt__Issuer = "ProductsManagement"
$env:Jwt__Audience = "ProductsManagementClient"
$env:Jwt__Secret = "kX9wLz&Sg4bA2@jTf7YpC5vH1rN3qM0E!uI6oD8zW_eG"
```

ASP.NET Core automatically binds environment variables using `__` as a section separator.

For local development, you can use the Secret Manager instead of env vars:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;..."
dotnet user-secrets set "CloudinarySettings:CloudName" "your_cloud_name"
dotnet user-secrets set "CloudinarySettings:ApiKey" "your_api_key"
dotnet user-secrets set "CloudinarySettings:ApiSecret" "your_api_secret"
dotnet user-secrets set "Jwt:Issuer" "ProductsManagement"
dotnet user-secrets set "Jwt:Audience" "ProductsManagementClient"
dotnet user-secrets set "Jwt:Secret" "kX9wLz&Sg4bA2@jTf7YpC5vH1rN3qM0E!uI6oD8zW_eG"
```

In production, use a managed secrets store (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault).

### Run

```bash
dotnet run --project Products_Management/Products_Management
```

Swagger will be available at `/`.
# Asignment2API
# Asignment2API
