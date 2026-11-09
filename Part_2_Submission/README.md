# RaceDay — Part 2: RESTful API Development

This is the corrected Part 2 implementation: an **ASP.NET Core Web API written in C#**, using Entity Framework Core Code-First and SQL Server. This replaces the previously submitted CampusConnect HTML/JavaScript project.

## Projects

- `RaceDay.Api/` — API controllers, RaceDay entities, EF Core database context, cookie/session authentication, role authorization, Swagger and Azure Blob Storage upload endpoints.
- `RaceDay.Api.Tests/` — xUnit HTTP integration tests using WebApplicationFactory and an isolated SQLite in-memory database.

## Roles and security

**Organiser:** creates and maintains owned events and categories, views enrolments for owned events, and captures or corrects event results.

**Participant:** browses events and categories, enrols in a category, cancels eligible own enrolments, updates own profile, and views own results.

Public registration accepts either `Organiser` or `Participant`. Passwords are stored as ASP.NET Core Identity password hashes. Login issues an HTTP-only authentication cookie that carries the authenticated identity and role; session state also records user ID and role. Logout clears both. Organiser operations are scoped to events owned by that organiser.

## Implemented API areas

| Feature | Routes | Access |
|---|---|---|
| Authentication | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout` | Public / Public / Authenticated |
| Profile | `GET /api/profile/me`, `PUT /api/profile/me` | Both roles |
| Event discovery | `GET /api/events`, `GET /api/events/{id}` | Public |
| Event management | `GET /api/events/mine`, `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` | Organiser, owner scoped |
| Categories | `GET /api/events/{id}/categories`, `POST/PUT/DELETE /api/events/{id}/categories/{categoryId}` | Public read; Organiser write |
| Enrolments | `GET /api/enrolments/me`, `POST /api/events/{id}/enrolments`, `DELETE /api/enrolments/{id}`, `GET /api/events/{id}/enrolments` | Participant / Organiser |
| Results | `GET /api/results/me`, `GET/POST /api/events/{id}/results`, `PUT /api/events/{id}/results/{resultId}` | Participant / Organiser |
| Route and weather | `GET/PUT /api/events/{id}/route`, `GET /api/events/{id}/weather` | Public read; Organiser write |
| Image upload | `POST /api/events/{id}/banner`, `POST /api/profile/me/picture` | Organiser owner / Authenticated user |

Swagger is available at `/swagger`. Request/response DTOs and endpoint summaries are included in the API project.

## Database

The EF Core model contains Users, Events, Routes, Categories, Enrolments and Results, with foreign keys, indexes, unique constraints and check constraints aligned to the Part 1 schema.

Set `ConnectionStrings__RaceDayDb` for the target SQL Server. Create the database with EF tooling:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialCreate --project Part_2_Submission/RaceDay.Api/RaceDay.Api.csproj --startup-project Part_2_Submission/RaceDay.Api/RaceDay.Api.csproj
dotnet ef database update --project Part_2_Submission/RaceDay.Api/RaceDay.Api.csproj --startup-project Part_2_Submission/RaceDay.Api/RaceDay.Api.csproj
```

Run the API:

```powershell
dotnet run --project Part_2_Submission/RaceDay.Api/RaceDay.Api.csproj
```

## Tests

```powershell
dotnet test Part_2_Submission/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

Eleven xUnit integration tests passed in GitHub Actions (0 failed, 0 skipped) on 9 November 2026. Coverage includes registration validation, duplicate accounts, password hashing, login/session persistence, anonymous access, role rejection, organiser event ownership, participant enrolment persistence, Swagger availability, profile updates, event banner upload through a test storage adapter, Azure's missing-credential guard, and organiser result capture / participant finish-history retrieval.

The Azure upload code can be exercised end to end only with a real Azure Blob Storage account and `AzureBlobStorage__ConnectionString`. Without credentials, upload endpoints intentionally return HTTP 503.

## CI/CD and submission evidence

`.github/workflows/raceday-ci.yml` restores and builds the solution, runs the API integration tests and uploads the TRX test result. The verified run is [RaceDay .NET CI #42599642557](https://github.com/liloketwa-ux/poe/actions/runs/42599642557). The repository contains a generated evidence summary at `docs/ci-cd-green-build.svg`; it is based on the actual run metadata and is not a browser screenshot of the GitHub page.

Unlisted YouTube demonstration: `PASTE-UNLISTED-YOUTUBE-LINK-HERE`.

Review [the Part 2 commit plan](COMMIT_PLAN.md). The rubric requires 20 meaningful commits made by the student. The GitHub history must reflect genuine implementation and testing work; a commit plan is not itself proof of completion.
