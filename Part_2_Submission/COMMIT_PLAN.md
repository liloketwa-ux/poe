# Part 2 — 20 meaningful commit plan

The assessment requires at least 20 meaningful commits made from your own GitHub account. Do not create empty commits or copy timestamps. Make each commit after completing and testing the corresponding change; use the actual work and dates you performed.

1. `chore(api): create ASP.NET Core Web API solution` — create the solution and API project.
2. `feat(data): add RaceDay entity models` — add User, Event, Route, Category, Enrolment and Result entities.
3. `feat(data): configure EF Core SQL Server context` — add DbSets, keys, constraints and relationships.
4. `feat(auth): implement participant and organiser registration` — add validated registration DTO and endpoint.
5. `feat(auth): hash passwords with Identity PasswordHasher` — store only password hashes and test verification.
6. `feat(auth): add cookie session login and logout` — add login claims, cookie and logout.
7. `feat(profile): implement authenticated profile endpoints` — add own-profile retrieval and update.
8. `feat(events): add public event browsing and filtering` — add catalogue and event details.
9. `feat(events): restrict event creation to organisers` — add ownership-scoped create endpoint.
10. `feat(events): add owner-only event update and delete` — add edit and safe deletion rules.
11. `feat(categories): implement event category CRUD` — add public reads and organiser-only management.
12. `feat(enrolments): add participant category enrolment` — record event/category/participant links and capacity checks.
13. `feat(enrolments): add organiser enrolment reporting` — list entries only for owned events.
14. `feat(results): implement organiser result capture` — create and correct finish times and places.
15. `feat(results): add participant race history` — restrict personal history to its owner.
16. `feat(api): add route and weather resources` — persist routes and integrate event-location weather lookup.
17. `feat(api): document endpoints in Swagger` — add XML summaries and response/request descriptions.
18. `test(api): cover authentication and role enforcement` — test login, hashing, anonymous and wrong-role responses.
19. `test(api): cover event ownership and participant enrolment` — test persistence and access boundaries.
20. `ci(api): build projects and run tests in GitHub Actions` — add workflow and verify a green run.

Suggested workflow after each real milestone:

```powershell
git add Part_2_Submission .github/workflows/raceday-ci.yml
git commit -m "feat(api): describe the completed change"
git push origin raceday-part2
```
