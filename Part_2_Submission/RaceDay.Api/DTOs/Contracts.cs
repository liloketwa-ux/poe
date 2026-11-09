using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public sealed record RegisterRequest(
    [Required, StringLength(60, MinimumLength = 2)] string FirstName,
    [Required, StringLength(60, MinimumLength = 2)] string LastName,
    [Required, EmailAddress, StringLength(255)] string Email,
    [Required, MinLength(8), StringLength(100)] string Password,
    [Required] string Role,
    [Phone, StringLength(30)] string? Phone);

public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record UserResponse(int UserId, string FirstName, string LastName, string Email, string Role, string? Phone, string? ProfileImageUrl);
public sealed record UpdateProfileRequest([Required, StringLength(60, MinimumLength = 2)] string FirstName,
    [Required, StringLength(60, MinimumLength = 2)] string LastName, [Phone, StringLength(30)] string? Phone);

public sealed record EventRequest(
    [Required, StringLength(150, MinimumLength = 3)] string Name,
    [StringLength(500)] string? Description,
    DateOnly EventDate,
    DateOnly RegistrationOpen,
    DateOnly RegistrationClose,
    [Required, StringLength(150)] string Venue,
    [Required, StringLength(100)] string City,
    [Required, StringLength(100)] string Province,
    decimal DistanceKm,
    [Required] string EventType,
    string Status = "Open");
public sealed record EventSummary(int EventId, string Name, string? Description, DateOnly EventDate, string Venue,
    string City, string Province, decimal DistanceKm, string EventType, string Status, string? BannerImageUrl);
public sealed record EventDetails(int EventId, string Name, string? Description, DateOnly EventDate,
    DateOnly RegistrationOpen, DateOnly RegistrationClose, string Venue, string City, string Province,
    decimal DistanceKm, string EventType, string Status, string OrganiserName, string? BannerImageUrl,
    IReadOnlyList<CategorySummary> Categories, RouteResponse? Route);

public sealed record CategoryRequest([Required, StringLength(100)] string Name, decimal DistanceKm,
    decimal EntryFee, int MaxEntries, int? MinAge, int? MaxAge);
public sealed record CategorySummary(int CategoryId, int EventId, string Name, decimal DistanceKm,
    decimal EntryFee, int MaxEntries, int EntriesTaken, int? MinAge, int? MaxAge);
public sealed record EnrolRequest(int CategoryId);
public sealed record EnrolmentResponse(int EnrolmentId, int EventId, string EventName, int CategoryId,
    string CategoryName, DateTime EnrolmentDate, string Status, string ParticipantName, string ParticipantEmail);
public sealed record ResultRequest(int EnrolmentId, TimeOnly? FinishTime, int? OverallPosition,
    int? CategoryPosition, decimal? AveragePace, string FinishStatus = "Finished");
public sealed record ResultUpdateRequest(TimeOnly? FinishTime, int? OverallPosition,
    int? CategoryPosition, decimal? AveragePace, string FinishStatus = "Finished");
public sealed record ResultResponse(int ResultId, int EnrolmentId, string ParticipantName,
    string EventName, DateOnly EventDate, string CategoryName, TimeOnly? FinishTime,
    int? OverallPosition, int? CategoryPosition, decimal? AveragePace, string FinishStatus);
public sealed record RouteRequest([Required] string StartLocation, [Required] string FinishLocation,
    [Url, StringLength(500)] string? RouteUrl, int? ElevationGainM, [StringLength(500)] string? Notes);
public sealed record RouteResponse(int RouteId, int EventId, string StartLocation, string FinishLocation,
    string? RouteUrl, int? ElevationGainM, string? Notes);