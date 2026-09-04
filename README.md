# RaceDay – Part 1: System Planning and Database

RaceDay is a full-stack event management platform designed for South African road running, walking and cycling events. The system allows organisers to manage events, categories, enrolments and participant results, while participants can discover events, enter categories and view their personal performance history.

## User roles

### Organiser
An Organiser can create, edit and delete events, manage event categories, view all enrolments for their events, and capture or update participant results.

### Participant
A Participant can register and log in, browse upcoming events, enrol in an available event category, view their own enrolments, and track their personal results.

Role-based access is part of the API design and will be enforced in Part 2, then reflected in the MVC interface in Part 3.

## Part 1 deliverables

The complete planning artefacts are in `/docs`:

1. ERD (`RaceDay_ERD.png` and `RaceDay_ERD.pdf`)
2. REST API endpoint plan (`raceday-endpoint-plan.md`)
3. SQL Server schema and seed script (`RaceDayDB.sql`)

## Database overview

The relational model contains six entities:

- Users
- Events
- Routes
- Categories
- Enrolments
- Results

The SQL script includes primary keys, foreign keys, NOT NULL constraints, UNIQUE constraints, CHECK constraints and DEFAULT values, followed by sample data containing two Organisers, two Participants, three Events, event categories and sample enrolments.

## Running the database script

1. Open SQL Server Management Studio (SSMS).
2. Connect to a SQL Server instance.
3. Open `docs/RaceDayDB.sql`.
4. Execute the entire script.
5. Confirm that the final six validation queries return seeded records.

## CI/CD

The GitHub Actions workflow is stored in `.github/workflows/validate-docs.yml`. It checks that the repository structure and required Part 1 documentation files exist.

### Successful CI build screenshot

> **Submission step:** After pushing the repository to GitHub, open the Actions tab, run or wait for `Validate RaceDay Part 1`, and take a screenshot showing the green successful check. Replace the placeholder image below with that real GitHub screenshot before submitting.

`docs/ci-cd-green-build.png`

## Video presentation

> **Submission step:** Replace the placeholder below with your own YouTube link. The assessment requires a real voiceover; do not use an AI-generated voice.

YouTube: `PASTE-YOUR-YOUTUBE-LINK-HERE`

## GitHub commit requirement

A minimum of 20 meaningful commits is required for Part 1. Use the suggested sequence in `docs/commit-plan-part1.md` as a working guide, but create the commits from your own GitHub account as you complete the work.

## Repository structure

```text
RaceDay/
├── .github/
│   └── workflows/
│       └── validate-docs.yml
├── docs/
│   ├── RaceDayDB.sql
│   ├── RaceDay_ERD.png
│   ├── RaceDay_ERD.pdf
│   ├── commit-plan-part1.md
│   ├── erd.dot
│   ├── raceday-endpoint-plan.md
│   └── README-docs.md
└── README.md
```
