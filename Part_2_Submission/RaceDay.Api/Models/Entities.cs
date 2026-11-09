namespace RaceDay.Api.Models;

public sealed class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public sealed class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public User? Organiser { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public DateOnly RegistrationOpen { get; set; }
    public DateOnly RegistrationClose { get; set; }
    public string Venue { get; set; } = "";
    public string City { get; set; } = "";
    public string Province { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "Running";
    public string Status { get; set; } = "Open";
    public string? BannerImageUrl { get; set; }
    public ICollection<Route> Routes { get; set; } = new List<Route>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public sealed class Route
{
    public int RouteId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string StartLocation { get; set; } = "";
    public string FinishLocation { get; set; } = "";
    public string? RouteUrl { get; set; }
    public int? ElevationGainM { get; set; }
    public string? Notes { get; set; }
}

public sealed class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string Name { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxEntries { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public sealed class Enrolment
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public User? Participant { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
    public Result? Result { get; set; }
}

public sealed class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }
    public TimeOnly? FinishTime { get; set; }
    public int? OverallPosition { get; set; }
    public int? CategoryPosition { get; set; }
    public decimal? AveragePace { get; set; }
    public string FinishStatus { get; set; } = "Finished";
}