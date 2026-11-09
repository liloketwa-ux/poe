using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api")]
public sealed class EnrolmentsController(RaceDayDbContext db) : ApiControllerBase(db)
{
    /// <summary>Lists enrolments belonging to the signed-in Participant.</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet("enrolments/me")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Mine(CancellationToken token) => Ok(await QueryEnrolments()
        .Where(x => x.ParticipantId == CurrentUserId).OrderByDescending(x => x.EnrolmentDate).Select(ToResponse()).ToListAsync(token));

    /// <summary>Enrols the signed-in Participant in an event category.</summary>
    /// <remarks>JSON body: categoryId. Returns 409 when registration is closed, the category is full or the Participant is already enrolled.</remarks>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("events/{eventId:int}/enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int eventId, EnrolRequest request, CancellationToken token)
    {
        var item = await Db.Events.Include(x => x.Categories).FirstOrDefaultAsync(x => x.EventId == eventId, token);
        if (item is null) return NotFound(new { message = "Event not found." });
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today < item.RegistrationOpen || today > item.RegistrationClose || item.Status != "Open")
            return Conflict(new { message = "Registration for this event is not open." });
        var category = item.Categories.FirstOrDefault(x => x.CategoryId == request.CategoryId);
        if (category is null) return BadRequest(new { message = "The selected category does not belong to this event." });
        if (await Db.Enrolments.AnyAsync(x => x.ParticipantId == CurrentUserId && x.EventId == eventId, token))
            return Conflict(new { message = "You are already enrolled in this event." });
        var taken = await Db.Enrolments.CountAsync(x => x.CategoryId == category.CategoryId && x.Status == "Confirmed", token);
        if (taken >= category.MaxEntries) return Conflict(new { message = "This category is full." });
        var enrolment = new Enrolment { ParticipantId = CurrentUserId, EventId = eventId,
            CategoryId = category.CategoryId, EnrolmentDate = DateTime.UtcNow, Status = "Confirmed" };
        Db.Enrolments.Add(enrolment);
        try { await Db.SaveChangesAsync(token); }
        catch (DbUpdateException) { return Conflict(new { message = "The enrolment conflicts with an existing entry." }); }
        var result = await QueryEnrolments().Where(x => x.EnrolmentId == enrolment.EnrolmentId).Select(ToResponse()).SingleAsync(token);
        return Created($"/api/enrolments/{enrolment.EnrolmentId}", result);
    }

    /// <summary>Cancels an eligible enrolment belonging to the signed-in Participant.</summary>
    /// <remarks>An enrolment with an existing result cannot be cancelled.</remarks>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpDelete("enrolments/{id:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Cancel(int id, CancellationToken token)
    {
        var item = await Db.Enrolments.Include(x => x.Result).FirstOrDefaultAsync(x => x.EnrolmentId == id, token);
        if (item is null) return NotFound();
        if (item.ParticipantId != CurrentUserId) return Forbid();
        if (item.Result is not null) return Conflict(new { message = "An enrolment with a recorded result cannot be cancelled." });
        item.Status = "Cancelled"; await Db.SaveChangesAsync(token); return NoContent();
    }

    /// <summary>Lists enrolments for an event owned by the signed-in Organiser.</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("events/{eventId:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> ForEvent(int eventId, CancellationToken token)
    {
        if (!await Db.Events.AnyAsync(x => x.EventId == eventId, token)) return NotFound();
        if (!await OwnsEventAsync(eventId, token)) return Forbid();
        return Ok(await QueryEnrolments().Where(x => x.EventId == eventId)
            .OrderBy(x => x.Participant!.LastName).Select(ToResponse()).ToListAsync(token));
    }

    private IQueryable<Enrolment> QueryEnrolments() => Db.Enrolments.AsNoTracking()
        .Include(x => x.Participant).Include(x => x.Event).Include(x => x.Category);
    private static System.Linq.Expressions.Expression<Func<Enrolment, EnrolmentResponse>> ToResponse() =>
        x => new EnrolmentResponse(x.EnrolmentId, x.EventId, x.Event!.Name, x.CategoryId, x.Category!.Name,
            x.EnrolmentDate, x.Status, x.Participant!.FirstName + " " + x.Participant.LastName, x.Participant.Email);
}