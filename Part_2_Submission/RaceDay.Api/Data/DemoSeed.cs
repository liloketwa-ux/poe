using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDayRoute = RaceDay.Api.Models.Route;

namespace RaceDay.Api.Data;

/// <summary>Creates predictable demonstration records only when explicitly enabled by configuration.</summary>
public static class DemoSeed
{
    public const string DemoPassword = "RaceDayDemo!2026";

    public static async Task SeedAsync(RaceDayDbContext db, IPasswordHasher<User> hasher, CancellationToken token = default)
    {
        if (await db.Users.AnyAsync(token)) return;

        var organiser = new User
        {
            FirstName = "Thabo", LastName = "Mokoena", Email = "organiser@raceday.demo",
            Role = "Organiser", Phone = "0825551001"
        };
        organiser.PasswordHash = hasher.HashPassword(organiser, DemoPassword);

        var participant = new User
        {
            FirstName = "Lebo", LastName = "Dlamini", Email = "participant@raceday.demo",
            Role = "Participant", Phone = "0815552001"
        };
        participant.PasswordHash = hasher.HashPassword(participant, DemoPassword);
        db.Users.AddRange(organiser, participant);
        await db.SaveChangesAsync(token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var first = new Event
        {
            OrganiserId = organiser.UserId, Name = "Johannesburg City Challenge",
            Description = "A city road-running challenge with a half marathon and 10 km category.",
            EventDate = today.AddDays(14), RegistrationOpen = today.AddDays(-30), RegistrationClose = today.AddDays(10),
            Venue = "Constitution Hill", City = "Johannesburg", Province = "Gauteng",
            DistanceKm = 21.1m, EventType = "Running", Status = "Open"
        };
        var second = new Event
        {
            OrganiserId = organiser.UserId, Name = "Cape Peninsula Cycle",
            Description = "A scenic community cycling event.",
            EventDate = today.AddDays(28), RegistrationOpen = today.AddDays(-10), RegistrationClose = today.AddDays(24),
            Venue = "Green Point Urban Park", City = "Cape Town", Province = "Western Cape",
            DistanceKm = 42m, EventType = "Cycling", Status = "Open"
        };
        var third = new Event
        {
            OrganiserId = organiser.UserId, Name = "Durban Family Walk",
            Description = "A family-friendly walk along the Durban beachfront.",
            EventDate = today.AddDays(42), RegistrationOpen = today.AddDays(-5), RegistrationClose = today.AddDays(38),
            Venue = "Moses Mabhida Stadium", City = "Durban", Province = "KwaZulu-Natal",
            DistanceKm = 10m, EventType = "Walking", Status = "Open"
        };
        db.Events.AddRange(first, second, third);
        await db.SaveChangesAsync(token);

        var half = new Category { EventId = first.EventId, Name = "Half Marathon Open", DistanceKm = 21.1m, EntryFee = 220m, MaxEntries = 3000, MinAge = 18 };
        var ten = new Category { EventId = first.EventId, Name = "10 km Challenge", DistanceKm = 10m, EntryFee = 120m, MaxEntries = 2500, MinAge = 16 };
        var cycle = new Category { EventId = second.EventId, Name = "42 km Cycle", DistanceKm = 42m, EntryFee = 280m, MaxEntries = 1500, MinAge = 18 };
        var walk = new Category { EventId = third.EventId, Name = "10 km Family Walk", DistanceKm = 10m, EntryFee = 80m, MaxEntries = 2000, MinAge = 12 };
        db.Categories.AddRange(half, ten, cycle, walk);
        db.Routes.AddRange(
            new RaceDayRoute { EventId = first.EventId, StartLocation = "Constitution Hill", FinishLocation = "Constitution Hill", ElevationGainM = 210, Notes = "Urban road loop." },
            new Route { EventId = second.EventId, StartLocation = "Green Point Urban Park", FinishLocation = "Green Point Urban Park", ElevationGainM = 480, Notes = "Coastal and city sections." },
            new Route { EventId = third.EventId, StartLocation = "Moses Mabhida Stadium", FinishLocation = "Moses Mabhida Stadium", ElevationGainM = 65, Notes = "Flat beachfront family route." });
        var enrolment = new Enrolment
        {
            ParticipantId = participant.UserId, EventId = first.EventId, CategoryId = half.CategoryId,
            Status = "Confirmed", EnrolmentDate = DateTime.UtcNow
        };
        db.Enrolments.Add(enrolment);
        await db.SaveChangesAsync(token);
        db.Results.Add(new Result
        {
            EnrolmentId = enrolment.EnrolmentId, FinishTime = new TimeOnly(1, 42, 18),
            OverallPosition = 32, CategoryPosition = 18, AveragePace = 4.85m, FinishStatus = "Finished"
        });
        await db.SaveChangesAsync(token);
    }
}
