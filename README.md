# Lateral CMS API

Lateral CMS is an ASP.NET Core Web API built with .NET 9 and Entity Framework Core. It uses a local SQLite database, applies pending migrations automatically, and seeds a development administrator account when the API starts for the first time.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git, if you are cloning the repository

Confirm that the SDK is available:

```text
dotnet --version
```

The output should start with `9.`. Platform-specific .NET installation guidance is available for [Windows](https://learn.microsoft.com/dotnet/core/install/windows), [Linux](https://learn.microsoft.com/dotnet/core/install/linux), and [macOS](https://learn.microsoft.com/dotnet/core/install/macos).

## Run on Windows

Open PowerShell in the repository root and run:

```powershell
dotnet restore .\src\LateralCms\LateralCms.sln
dotnet run --project .\src\LateralCms\LateralCms\LateralCms.Api.csproj --launch-profile http
```

## Run on Linux

Open a terminal in the repository root and run:

```bash
dotnet restore ./src/LateralCms/LateralCms.sln
dotnet run --project ./src/LateralCms/LateralCms/LateralCms.Api.csproj --launch-profile http
```

## Run on macOS

Open Terminal in the repository root and run:

```bash
dotnet restore ./src/LateralCms/LateralCms.sln
dotnet run --project ./src/LateralCms/LateralCms/LateralCms.Api.csproj --launch-profile http
```

On all platforms, the API starts at:

- Swagger UI: <http://localhost:5205/swagger>
- OpenAPI document: <http://localhost:5205/openapi/v1.json>
- API base URL: <http://localhost:5205/api>

Press `Ctrl+C` in the terminal to stop the API.

## First startup

No external database server is required. On first startup, the API automatically:

1. Creates `src/LateralCms/LateralCms/App_Data/lateralcms.db`.
2. Applies the included Entity Framework Core migrations.
3. Seeds the roles and the local administrator account.

The seeded development credentials are:

```text
Username: administrator
Password: fb29ce7c-b3a5-4841-94ef-650085d774a3
```

These credentials are stored in the source code for local assessment purposes and must be replaced before deploying the API to a shared or production environment.

## Verify the API

Most endpoints require HTTP Basic Authentication. With the API running, submit a sample event from a second terminal.

The webhook accepts batches containing these CMS event types:

- `publish`: stores the supplied snapshot and exposes that version to consumers.
- `unPublish`: stores the supplied snapshot, including a version that was never published, and disables the entity without deleting its history.
- `delete`: hard-deletes the entity and all of its stored versions.

The service also accepts the scenario's `add` and `update` lifecycle events. They store an unpublished version; data only becomes consumer-visible after a `publish` event. Event names are case-insensitive, while IDs, timestamps, versions, and payloads are validated before a batch is accepted.

### Windows PowerShell

```powershell
$credentials = [Convert]::ToBase64String(
    [Text.Encoding]::UTF8.GetBytes("administrator:fb29ce7c-b3a5-4841-94ef-650085d774a3")
)

$body = ConvertTo-Json -InputObject @(
    @{
        type = "publish"
        id = "example-1"
        payload = @{ title = "Hello" }
        version = 1
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
    }
)

Invoke-RestMethod `
    -Method Post `
    -Uri "http://localhost:5205/api/cms/events" `
    -Headers @{ Authorization = "Basic $credentials" } `
    -ContentType "application/json" `
    -Body $body
```

### Linux or macOS

```bash
curl --user 'administrator:fb29ce7c-b3a5-4841-94ef-650085d774a3' \
  --header 'Content-Type: application/json' \
  --data '[{"type":"publish","id":"example-1","payload":{"title":"Hello"},"version":1,"timestamp":"2026-08-04T12:00:00Z"}]' \
  http://localhost:5205/api/cms/events
```

A successful request returns HTTP `202 Accepted` and a batch ID.

## Build and test

Run these commands from the repository root:

### Windows

```powershell
dotnet build .\src\LateralCms\LateralCms.sln
dotnet test .\src\LateralCms\LateralCms.sln
```

### Linux or macOS

```bash
dotnet build ./src/LateralCms/LateralCms.sln
dotnet test ./src/LateralCms/LateralCms.sln
```

## Configuration

The default SQLite connection string is defined in `src/LateralCms/LateralCms/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "LateralCmsDatabase": "Data Source=App_Data/lateralcms.db"
  }
}
```

You can override it with the `ConnectionStrings__LateralCmsDatabase` environment variable. The target directory must be writable by the account running the API.

## Troubleshooting

- **`dotnet` is not recognized or not found:** install the .NET 9 SDK, restart the terminal, and run `dotnet --version` again.
- **Port 5205 is already in use:** stop the process using that port or replace `--launch-profile http` with `--urls http://localhost:5210`.
- **Package restore fails:** confirm that the machine can reach NuGet, then rerun `dotnet restore`.
- **The database cannot be created:** confirm that `src/LateralCms/LateralCms` is writable and that another process is not locking the SQLite file.
