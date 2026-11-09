# RaceDay — Part 3: Automation of Web Application

The RaceDay ASP.NET Core MVC front end consumes the Part 2 API using the server-side `RaceDayApiClient`. The MVC application has no Entity Framework reference and no database connection. API authentication cookies are held in the server-side MVC session and forwarded on API calls.

## Features

- Register/login/logout as Organiser or Participant.
- Session-aware navigation and protected role-specific actions.
- Public event catalogue with type and province filters, event details and categories.
- Organiser dashboard, event create/edit/delete, category creation, enrolment list and result capture.
- Participant dashboard, personal enrolments and results, profile edit and profile photo.
- Event banner and profile images are uploaded by MVC to the API. Only the API connects to Azure Blob Storage.
- Responsive Bootstrap UI with RaceDay dark-green, orange and neutral palette.

## Run locally

1. Start SQL Server and configure `ConnectionStrings__RaceDayDb` for the API.
2. Start the API as documented in [Part 2](../Part_2_Submission/README.md), then confirm Swagger at `http://localhost:5100/swagger`.
3. Run the MVC project:

```powershell
dotnet run --project Part_3_Submission/RaceDay.Mvc/RaceDay.Mvc.csproj
```

Set `Api__BaseUrl` to the API URL if necessary. Browse the local URL printed by dotnet.

## Demo accounts for the Docker demonstration

When the Docker Compose configuration is used, it creates two local demonstration accounts and sample events/categories/routes/enrolment/results. This seed is only enabled by `Database__SeedDemoData=true` in the Compose file.

| Role | Email | Password |
|---|---|---|
| Organiser | `organiser@raceday.demo` | `RaceDayDemo!2026` |
| Participant | `participant@raceday.demo` | `RaceDayDemo!2026` |

These are demonstration credentials, not production accounts. Do not deploy the seeded environment as a production system.

## Docker Compose

From this directory, set a strong local SQL Server password and (optionally) an Azure Storage connection string:

```powershell
$env:SA_PASSWORD = "Replace-With-A-Strong-Local-Password!"
$env:AZURE_BLOB_CONNECTION_STRING = "your-storage-connection-string"
docker compose up --build
```

Open MVC at http://localhost:5101 and Swagger at http://localhost:5100/swagger. SQL Server data is stored in a named Docker volume. Azure upload requires a real connection string; without it, upload endpoints report a service-unavailable response.

## Evidence to add after your own run

- UI design previews are in the accompanying POE evidence pack. Capture your actual running application before submitting.
- Screenshot of event details, categories, enrolments and result entry.
- CI verification summary: `../docs/ci-cd-green-build.svg`; successful workflow: [RaceDay .NET CI #42599642557](https://github.com/liloketwa-ux/poe/actions/runs/42599642557).
- Unlisted YouTube video link: `PASTE-UNLISTED-YOUTUBE-LINK-HERE`.

Do not insert mock CI evidence. Demonstrate the Azure upload with a configured storage account during the recording if that integration is being assessed.