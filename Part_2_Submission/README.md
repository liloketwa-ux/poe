# RaceDay — Part 2: RESTful API Development

This is the Part 2 implementation: an **ASP.NET Core Web API written in C#**, using Entity Framework Core Code-First and SQL Server.

## Projects

- `RaceDay.Api/` — API controllers, RaceDay entities, EF Core database context, cookie/session authentication, role authorization, Swagger and Azure Blob Storage upload endpoints.
- `RaceDay.Api.Tests/` — xUnit HTTP integration tests using WebApplicationFactory and an isolated SQLite in-memory database.

## Roles and security

**Organiser:** creates and maintains owned events and categories, views enrolments for owned events, and captures or corrects event results.

**Participant:** browses events and categories, enrols in a category, cancels eligible own enrolments, updates own profile and views own results.

Public registration accepts either `Organiser` or `Participant`. Passwords are stored as ASP.NET Core Identity password hashes. Login issues an HTTP-only authentication cookie containing the authenticated identity and role; session state also records the user ID and role. Logout clears both. Organiser operations are scoped to events owned by that organiser.

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

Swagger is available at `/swagger`. XML operation summaries and response metadata are included in the API controllers.

## Database

The EF Core Code-First model contains Users, Events, Routes, Categories, Enrolments and Results, with foreign keys, indexes, unique constraints and check constraints aligned with the Part 1 SQL schema. The Part 3 image columns are added by `docs/RaceDay-Part3-ImageColumns.sql` when using the Part 1 SQL script.

The default development connection string uses `(localdb)\\MSSQLLocalDB`. To create a clean local development database and insert demonstration records, run from the repository root:

```powershell
$env:Database__EnsureCreated = "true"
$env:Database__SeedDemoData = "true"
dotnet run --no-launch-profile --project .\Part_2_Submission\RaceDay.Api\RaceDay.Api.csproj --urls http://localhost:5100
```

`EnsureCreated` creates a new database schema; it does not apply migrations or update a pre-existing database. If you already created the database using the Part 1 SQL script, apply `docs/RaceDay-Part3-ImageColumns.sql` after the Part 1 SQL script.

## Tests

Run:

```powershell
dotnet test Part_2_Submission/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj --configuration Release
```

The xUnit suite uses HTTP integration tests and an isolated SQLite database. It covers registration validation, duplicate accounts, password hashing, login/session persistence, anonymous and wrong-role access, organiser event ownership, participant enrolment, result publishing/history, Swagger availability, profile updates, fake-storage upload persistence, missing Azure credentials and demo-data seeding. The demo-seed regression test checks that the sample enrolment references the generated category identity.

## Azure Blob Storage

Event banner and profile-picture uploads are implemented in the API. The storage service requires a real connection string in `AzureBlobStorage__ConnectionString`. Without credentials, upload endpoints intentionally return HTTP 503. Automated tests use a fake storage adapter and separately test the missing-credential guard; these tests do **not** prove that an upload to a real Azure account succeeded.

## CI/CD and submission evidence

The workflow `.github/workflows/raceday-ci.yml` uses .NET 8. The verified [RaceDay .NET 8 CI run #42637090817](https://github.com/liloketwa-ux/poe/actions/runs/42637090817) completed successfully: solution build passed, 12 integration tests passed (0 failed, 0 skipped), Docker Compose validated, and API/MVC Docker images built. See the [workflow history](https://github.com/liloketwa-ux/poe/actions/workflows/raceday-ci.yml).

## Video presentations

- **Interface demonstration — running RaceDay application:** [Watch the interface demonstration](https://youtu.be/ODFcxN-v5O8)
- **Code walkthrough — API structure, controllers, database logic, authentication, validation, Blob Storage service and tests:** [Watch the coding walkthrough](https://youtu.be/InnJ0NeXg9w)

Review [the Part 2 commit plan](COMMIT_PLAN.md). A commit plan is not itself proof that each development task was completed; use the commit history and ensure the implementation is work you can explain.
