# RaceDay — Event Management Platform

RaceDay is a South African road-running, walking and cycling event platform. Organisers publish events, manage categories, review enrolments and capture race results. Participants discover events, enrol and view their personal results.

**Technology:** C#, ASP.NET Core Web API, ASP.NET Core MVC, Entity Framework Core, SQL Server, Swagger, xUnit, Azure Blob Storage and Docker.

## Repository map

| Part | Location | Deliverables |
|---|---|---|
| Part 1 — Planning/database | `docs/` | ERD, endpoint plan and SQL Server schema |
| Part 2 — REST API | `Part_2_Submission/` | C# API, EF Core, authentication, role access, Swagger and tests |
| Part 3 — MVC | `Part_3_Submission/` | MVC front end, API client, role-specific UI, image-upload forwarding and Docker |
| CI/CD | `.github/workflows/raceday-ci.yml` | Builds API/MVC, runs API tests and builds both Docker images |

## Part 1

The Part 1 planning and database artefacts are in `docs/`: ERD, API endpoint plan, SQL Server schema and the Part 3 image-column extension script.

## Part 2 — RESTful API development

The C# API uses EF Core Code-First with SQL Server. It provides Organiser and Participant roles, ASP.NET Core Identity password hashing, an HTTP-only authentication cookie, session state, role-protected endpoints and Swagger documentation.

See [Part 2 README](Part_2_Submission/README.md).

## Part 3 — MVC application

The MVC application consumes Part 2 through a server-side typed HTTP client. It has no Entity Framework reference or database connection. It provides public event discovery, role-specific dashboards, event and category workflows, enrolments, results and profile pages. Event banners and profile pictures are forwarded through the API to Azure Blob Storage.

See [Part 3 README](Part_3_Submission/README.md).

## Build and test

The solution projects target **.NET 8.0**. From the repository root:

```powershell
dotnet --list-sdks
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test Part_2_Submission/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

## Run locally without Docker

A local SQL Server instance is required. The API's default development connection string uses SQL Server LocalDB. Start the API in one PowerShell window, from the repository root:

```powershell
$env:Database__EnsureCreated = "true"
$env:Database__SeedDemoData = "true"
dotnet run --no-launch-profile --project .\Part_2_Submission\RaceDay.Api\RaceDay.Api.csproj --urls http://localhost:5100
```

In a second PowerShell window:

```powershell
dotnet run --no-launch-profile --project .\Part_3_Submission\RaceDay.Mvc\RaceDay.Mvc.csproj --urls http://localhost:5101
```

Open the MVC site at [http://localhost:5101](http://localhost:5101) and Swagger at [http://localhost:5100/swagger](http://localhost:5100/swagger). Demo accounts are listed in the Part 3 README. For the first run, use a clean LocalDB database. EF Core `EnsureCreated` does not update an existing database schema.

## Docker

From the repository root:

```powershell
$env:SA_PASSWORD = "Replace-With-A-Strong-Local-Password!"
docker compose -f .\Part_3_Submission\docker-compose.yml up --build
```

See the Part 3 README for optional Azure configuration and stop instructions.

## CI/CD and evidence

The [successful RaceDay .NET 8 CI run #42637090817](https://github.com/liloketwa-ux/poe/actions/runs/42637090817) completed on 9 November 2026: API, MVC and tests built; all 12 API integration tests passed; Docker Compose validated; and both Docker images built. The [workflow history](https://github.com/liloketwa-ux/poe/actions/workflows/raceday-ci.yml) contains subsequent runs.

The generated SVG evidence summary is based on workflow metadata, not a screenshot of the GitHub Actions page. Capture a genuine screenshot of the successful run and screenshots of the actual running MVC application where required by the rubric.

## Demonstration video

[Unlisted YouTube demonstration](https://youtu.be/ODFcxN-v5O8)
