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
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int eventId, CancellationToken token)
    {
        if (!await Db.Events.AnyAsync(x => x.EventId == eventId, token)) return NotFound();
        return Ok(await Db.Categories.Where(x => x.EventId == eventId).Select(x => new CategorySummary(
            x.CategoryId, x.EventId, x.Name, x.DistanceKm, x.EntryFee, x.MaxEntries,
            x.Enrolments.Count(e => e.Status == "Confirmed"), x.MinAge, x.MaxAge)).ToListAsync(token));
    }

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
        if (x.MinAge.HasValue != x.MaxAge.HasValue || (x.MinAge.HasValue && (x.MinAge < 0 || x.MaxAge < x.MinAge))) return "Supply a valid minimum and maximum age range, or leave both blank.";
        return null;
    }
    private static CategorySummary ToSummary(Category x) => new(x.CategoryId, x.EventId, x.Name, x.DistanceKm,
        x.EntryFee, x.MaxEntries, 0, x.MinAge, x.MaxAge);
}