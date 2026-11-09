# RaceDay — Part 3: Automation of Web Application

The RaceDay ASP.NET Core MVC front end consumes the Part 2 API using the server-side `RaceDayApiClient`. The MVC application has no Entity Framework reference and no database connection. API authentication cookies are held in the server-side MVC session and forwarded on API calls.

## Features

- Register, login and logout as Organiser or Participant.
- Session-aware navigation and protected role-specific actions.
- Public event catalogue with type and province filters, event details and categories.
- Organiser dashboard, event create/edit/delete, category creation, enrolment list and result capture.
- Participant dashboard, personal enrolments and results, profile edit and profile photo.
- Event banner and profile images are forwarded by MVC to the API. Only the API connects to Azure Blob Storage.
- Responsive Bootstrap UI with RaceDay dark-green, orange and neutral styling.

## Run locally without Docker

1. Use the .NET 8 SDK and make sure SQL Server LocalDB is installed and running.
2. Start the API using the instructions in [Part 2](../Part_2_Submission/README.md). Confirm Swagger at [http://localhost:5100/swagger](http://localhost:5100/swagger).
3. In a second PowerShell window from the repository root, run:

```powershell
dotnet run --no-launch-profile --project .\Part_3_Submission\RaceDay.Mvc\RaceDay.Mvc.csproj --urls http://localhost:5101
```

Open [http://localhost:5101](http://localhost:5101). The API base URL defaults to `http://localhost:5100/`; set `Api__BaseUrl` only when the API uses a different address.

## Demo accounts

The demo seed creates these local accounts when `Database__SeedDemoData=true` is enabled on a clean database:

| Role | Email | Password |
|---|---|---|
| Organiser | `organiser@raceday.demo` | `RaceDayDemo!2026` |
| Participant | `participant@raceday.demo` | `RaceDayDemo!2026` |

These accounts are for local demonstration only, not production deployment.

## Docker Compose

From the repository root, set a strong local SQL Server password and start all services:

```powershell
$env:SA_PASSWORD = "Replace-With-A-Strong-Local-Password!"
docker compose -f .\Part_3_Submission\docker-compose.yml up --build
```

Optional Azure configuration:

```powershell
$env:AZURE_BLOB_CONNECTION_STRING = "your-storage-connection-string"
```

Open MVC at [http://localhost:5101](http://localhost:5101) and Swagger at [http://localhost:5100/swagger](http://localhost:5100/swagger). SQL Server data is stored in a named Docker volume. Without a real Azure connection string, upload endpoints return HTTP 503 by design.

Stop the services with `Ctrl+C`, then run:

```powershell
docker compose -f .\Part_3_Submission\docker-compose.yml down
```

Do not add `-v` unless you intend to delete the persistent database volume.

## Evidence still requiring capture

- Genuine screenshots of the running application: participant event catalogue, event details/enrolment, participant dashboard/profile, organiser dashboard, event/category management, enrolments and result entry.
- Actual Swagger UI and GitHub Actions success screenshots if requested by the rubric.
- The generated SVG CI summary in `../docs/ci-cd-green-build.svg` is not a screenshot of the GitHub website.
- A live Azure upload needs a real Azure Storage account and credentials. Automated fake-storage tests are not evidence of a live cloud upload.

Unlisted YouTube demonstration: [https://youtu.be/ODFcxN-v5O8](https://youtu.be/ODFcxN-v5O8)
