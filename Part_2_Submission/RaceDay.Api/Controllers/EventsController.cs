using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/events")]
public sealed class EventsController(RaceDayDbContext db) : ApiControllerBase(db)
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? eventType, [FromQuery] string? province,
        [FromQuery] DateOnly? fromDate, CancellationToken token)
    {
        var query = Db.Events.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(x => x.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(province)) query = query.Where(x => x.Province == province);
        query = query.Where(x => x.EventDate >= (fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow)));
        return Ok(await query.OrderBy(x => x.EventDate).Select(x => new EventSummary(x.EventId, x.Name,
            x.Description, x.EventDate, x.Venue, x.City, x.Province, x.DistanceKm, x.EventType, x.Status, x.BannerImageUrl)).ToListAsync(token));
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Mine(CancellationToken token)
    {
        var items = await Db.Events.AsNoTracking().Where(x => x.OrganiserId == CurrentUserId)
            .OrderBy(x => x.EventDate).Select(x => new EventSummary(x.EventId, x.Name, x.Description,
                x.EventDate, x.Venue, x.City, x.Province, x.DistanceKm, x.EventType, x.Status, x.BannerImageUrl)).ToListAsync(token);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken token)
    {
        var item = await Db.Events.AsNoTracking().Include(x => x.Organiser).Include(x => x.Categories).ThenInclude(x => x.Enrolments)
            .Include(x => x.Routes).FirstOrDefaultAsync(x => x.EventId == id, token);
        if (item is null) return NotFound();
        var categories = item.Categories.Select(c => new CategorySummary(c.CategoryId, c.EventId, c.Name,
            c.DistanceKm, c.EntryFee, c.MaxEntries, c.Enrolments.Count(e => e.Status == "Confirmed"), c.MinAge, c.MaxAge)).ToList();
        var route = item.Routes.FirstOrDefault();
        var details = new EventDetails(item.EventId, item.Name, item.Description, item.EventDate,
            item.RegistrationOpen, item.RegistrationClose, item.Venue, item.City, item.Province,
            item.DistanceKm, item.EventType, item.Status, $"{item.Organiser!.FirstName} {item.Organiser.LastName}",
            item.BannerImageUrl, categories, route is null ? null : new RouteResponse(route.RouteId, route.EventId,
            route.StartLocation, route.FinishLocation, route.RouteUrl, route.ElevationGainM, route.Notes));
        return Ok(details);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(EventRequest request, CancellationToken token)
    {
        var validation = ValidateEvent(request);
        if (validation is not null) return BadRequest(new { message = validation });
        var item = new Event
        {
            OrganiserId = CurrentUserId, Name = request.Name.Trim(), Description = request.Description?.Trim(),
            EventDate = request.EventDate, RegistrationOpen = request.RegistrationOpen,
            RegistrationClose = request.RegistrationClose, Venue = request.Venue.Trim(), City = request.City.Trim(),
            Province = request.Province.Trim(), DistanceKm = request.DistanceKm,
            EventType = NormaliseEventType(request.EventType), Status = request.Status
        };
        Db.Events.Add(item); await Db.SaveChangesAsync(token);
        return CreatedAtAction(nameof(GetById), new { id = item.EventId }, ToSummary(item));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, EventRequest request, CancellationToken token)
    {
        var item = await Db.Events.FirstOrDefaultAsync(x => x.EventId == id, token);
        if (item is null) return NotFound();
        if (item.OrganiserId != CurrentUserId) return Forbid();
        var validation = ValidateEvent(request);
        if (validation is not null) return BadRequest(new { message = validation });
        item.Name = request.Name.Trim(); item.Description = request.Description?.Trim();
        item.EventDate = request.EventDate; item.RegistrationOpen = request.RegistrationOpen;
        item.RegistrationClose = request.RegistrationClose; item.Venue = request.Venue.Trim();
        item.City = request.City.Trim(); item.Province = request.Province.Trim(); item.DistanceKm = request.DistanceKm;
        item.EventType = NormaliseEventType(request.EventType); item.Status = request.Status;
        await Db.SaveChangesAsync(token); return Ok(ToSummary(item));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        var item = await Db.Events.Include(x => x.Enrolments).FirstOrDefaultAsync(x => x.EventId == id, token);
        if (item is null) return NotFound();
        if (item.OrganiserId != CurrentUserId) return Forbid();
        if (item.Enrolments.Any()) return Conflict(new { message = "An event with enrolments cannot be deleted. Cancel or retain the event instead." });
        Db.Events.Remove(item); await Db.SaveChangesAsync(token); return NoContent();
    }

    private static string? ValidateEvent(EventRequest x)
    {
        if (x.DistanceKm <= 0) return "DistanceKm must be greater than zero.";
        if (x.RegistrationClose < x.RegistrationOpen || x.EventDate < x.RegistrationOpen)
            return "Registration dates and event date are inconsistent.";
        if (!new[] { "Running", "Walking", "Cycling" }.Contains(NormaliseEventType(x.EventType)))
            return "EventType must be Running, Walking or Cycling.";
        if (!new[] { "Draft", "Open", "Closed", "Completed", "Cancelled" }.Contains(x.Status))
            return "Status is not a supported event status.";
        return null;
    }
    private static string NormaliseEventType(string value) => value.Trim().ToLowerInvariant() switch
    { "run" or "running" => "Running", "walk" or "walking" => "Walking", "cycle" or "cycling" => "Cycling", _ => value.Trim() };
    private static EventSummary ToSummary(Event x) => new(x.EventId, x.Name, x.Description, x.EventDate, x.Venue,
        x.City, x.Province, x.DistanceKm, x.EventType, x.Status, x.BannerImageUrl);
}