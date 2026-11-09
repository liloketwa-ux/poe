# RaceDay live UI evidence

The PNG files in this folder are captured from the **running ASP.NET Core MVC application and live Swagger UI** by the GitHub Actions workflow named **RaceDay live UI evidence**.

They are not design mock-ups or generated previews. The workflow builds and starts the repository Docker Compose application with its seeded demonstration database, opens the pages in Chromium using Playwright, captures full-page screenshots, and commits the screenshots back to this folder.

To refresh the evidence, open the repository **Actions** tab, select **RaceDay live UI evidence**, choose the main branch, and click **Run workflow**. When the run completes successfully, the screenshots appear in this folder and are also available as a downloadable workflow artifact.

Demo login accounts are for this local evidence environment only:
- Organiser: organiser@raceday.demo
- Participant: participant@raceday.demo
- Password: RaceDayDemo!2026

Azure uploads are not represented by these screenshots; the evidence environment does not contain a real Azure Storage connection string.
