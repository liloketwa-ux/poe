using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/events/{eventId:int}/categories")]
public sealed class CategoriesController(RaceDayDbContext db) : ApiControllerBase(db)
{
    /// <summary>Lists categories and entry counts for an event.</summary>
    /// <remarks>Public endpoint. Requires the event id in the route.</remarks>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int eventId, CancellationToken token)
    {
        if (!await Db.Events.AnyAsync(x => x.EventId == eventId, token)) return NotFound();
        return Ok(await Db.Categories.Where(x => x.EventId == eventId).Select(x => new CategorySummary(
            x.CategoryId, x.EventId, x.Name, x.DistanceKm, x.EntryFee, x.MaxEntries,
            x.Enrolments.Count(e => e.Status == "Confirmed"), x.MinAge, x.MaxAge)).ToListAsync(token));
    }

    /// <summary>Creates a category for an event owned by the signed-in Organiser.</summary>
    /// <remarks>JSON body: name, distanceKm, entryFee, maxEntries, minAge and maxAge.</remarks>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request, CancellationToken token)
    {
        if (!await OwnsEventAsync(eventId, token)) return await Db.Events.AnyAsync(x => x.EventId == eventId, token) ? Forbid() : NotFound();
        var error = Validate(request); if (error is not null) return BadRequest(new { message = error });
        var item = new Category { EventId = eventId, Name = request.Name.Trim(), DistanceKm = request.DistanceKm,
            EntryFee = request.EntryFee, MaxEntries = request.MaxEntries, MinAge = request.MinAge, MaxAge = request.MaxAge };
        Db.Categories.Add(item); await Db.SaveChangesAsync(token);
        return Created($"/api/events/{eventId}/categories/{item.CategoryId}", ToSummary(item));
    }

    /// <summary>Updates a category on an event owned by the signed-in Organiser.</summary>
    /// <remarks>Requires the event id and category id in the route, plus the category JSON body.</remarks>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int eventId, int categoryId, CategoryRequest request, CancellationToken token)
    {
        var item = await Db.Categories.FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.EventId == eventId, token);
        if (item is null) return NotFound();
        if (!await OwnsEventAsync(eventId, token)) return Forbid();
        var error = Validate(request); if (error is not null) return BadRequest(new { message = error });
        item.Name = request.Name.Trim(); item.DistanceKm = request.DistanceKm; item.EntryFee = request.EntryFee;
        item.MaxEntries = request.MaxEntries; item.MinAge = request.MinAge; item.MaxAge = request.MaxAge;
        await Db.SaveChangesAsync(token); return Ok(ToSummary(item));
    }

    /// <summary>Deletes a category when it has no enrolments.</summary>
    /// <remarks>The signed-in Organiser must own the event. Returns 409 when entries exist.</remarks>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int eventId, int categoryId, CancellationToken token)
    {
        var item = await Db.Categories.Include(x => x.Enrolments).FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.EventId == eventId, token);
        if (item is null) return NotFound();
        if (!await OwnsEventAsync(eventId, token)) return Forbid();
        if (item.Enrolments.Any()) return Conflict(new { message = "A category with enrolments cannot be deleted." });
        Db.Categories.Remove(item); await Db.SaveChangesAsync(token); return NoContent();
    }

    private static string? Validate(CategoryRequest x)
    {
        if (x.DistanceKm <= 0 || x.EntryFee < 0 || x.MaxEntries <= 0) return "DistanceKm and MaxEntries must be positive, and EntryFee cannot be negative.";
        if (x.MinAge is < 0 || x.MaxAge is < 0 || (x.MinAge.HasValue && x.MaxAge.HasValue && x.MaxAge < x.MinAge))
            return "Age limits must be non-negative and the maximum age cannot be lower than the minimum age.";
        return null;
    }
    private static CategorySummary ToSummary(Category x) => new(x.CategoryId, x.EventId, x.Name, x.DistanceKm,
        x.EntryFee, x.MaxEntries, 0, x.MinAge, x.MaxAge);
}