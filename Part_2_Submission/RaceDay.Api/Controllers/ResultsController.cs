using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api")]
public sealed class ResultsController(RaceDayDbContext db) : ApiControllerBase(db)
{
    [HttpGet("results/me")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Mine(CancellationToken token) => Ok(await ResultQuery()
        .Where(x => x.Enrolment!.ParticipantId == CurrentUserId).OrderByDescending(x => x.Enrolment!.Event!.EventDate)
        .Select(ToResponse()).ToListAsync(token));

    [HttpGet("events/{eventId:int}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> ForEvent(int eventId, CancellationToken token)
    {
        if (!await Db.Events.AnyAsync(x => x.EventId == eventId, token)) return NotFound();
        if (!await OwnsEventAsync(eventId, token)) return Forbid();
        return Ok(await ResultQuery().Where(x => x.Enrolment!.EventId == eventId).Select(ToResponse()).ToListAsync(token));
    }

    [HttpPost("events/{eventId:int}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create(int eventId, ResultRequest request, CancellationToken token)
    {
        if (!await OwnsEventAsync(eventId, token)) return await Db.Events.AnyAsync(x => x.EventId == eventId, token) ? Forbid() : NotFound();
        var enrolment = await Db.Enrolments.Include(x => x.Result).FirstOrDefaultAsync(x => x.EnrolmentId == request.EnrolmentId && x.EventId == eventId, token);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found for this event." });
        if (enrolment.Status != "Confirmed") return Conflict(new { message = "Results can only be added for confirmed enrolments." });
        if (enrolment.Result is not null) return Conflict(new { message = "A result already exists for this enrolment. Use PUT to correct it." });
        var error = Validate(request.FinishTime, request.OverallPosition, request.CategoryPosition, request.AveragePace, request.FinishStatus);
        if (error is not null) return BadRequest(new { message = error });
        var result = new Result { EnrolmentId = enrolment.EnrolmentId, FinishTime = request.FinishTime,
            OverallPosition = request.OverallPosition, CategoryPosition = request.CategoryPosition,
            AveragePace = request.AveragePace, FinishStatus = request.FinishStatus };
        Db.Results.Add(result); await Db.SaveChangesAsync(token);
        var response = await ResultQuery().Where(x => x.ResultId == result.ResultId).Select(ToResponse()).SingleAsync(token);
        return Created($"/api/events/{eventId}/results/{result.ResultId}", response);
    }

    [HttpPut("events/{eventId:int}/results/{resultId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int eventId, int resultId, ResultUpdateRequest request, CancellationToken token)
    {
        var item = await Db.Results.Include(x => x.Enrolment).FirstOrDefaultAsync(x => x.ResultId == resultId && x.Enrolment!.EventId == eventId, token);
        if (item is null) return NotFound();
        if (!await OwnsEventAsync(eventId, token)) return Forbid();
        var error = Validate(request.FinishTime, request.OverallPosition, request.CategoryPosition, request.AveragePace, request.FinishStatus);
        if (error is not null) return BadRequest(new { message = error });
        item.FinishTime = request.FinishTime; item.OverallPosition = request.OverallPosition;
        item.CategoryPosition = request.CategoryPosition; item.AveragePace = request.AveragePace;
        item.FinishStatus = request.FinishStatus; await Db.SaveChangesAsync(token);
        return Ok(await ResultQuery().Where(x => x.ResultId == resultId).Select(ToResponse()).SingleAsync(token));
    }

    private IQueryable<Result> ResultQuery() => Db.Results.AsNoTracking()
        .Include(x => x.Enrolment).ThenInclude(e => e!.Participant)
        .Include(x => x.Enrolment).ThenInclude(e => e!.Event)
        .Include(x => x.Enrolment).ThenInclude(e => e!.Category);
    private static System.Linq.Expressions.Expression<Func<Result, ResultResponse>> ToResponse() => x =>
        new ResultResponse(x.ResultId, x.EnrolmentId,
            x.Enrolment!.Participant!.FirstName + " " + x.Enrolment.Participant.LastName,
            x.Enrolment.Event!.Name, x.Enrolment.Event.EventDate, x.Enrolment.Category!.Name,
            x.FinishTime, x.OverallPosition, x.CategoryPosition, x.AveragePace, x.FinishStatus);
    private static string? Validate(TimeOnly? time, int? overall, int? category, decimal? pace, string status)
    {
        if (overall is <= 0 || category is <= 0 || pace is <= 0) return "Positions and average pace must be positive values.";
        if (!new[] { "Finished", "DNF", "DNS" }.Contains(status)) return "FinishStatus must be Finished, DNF or DNS.";
        if (status == "Finished" && time is null) return "A finish time is required when FinishStatus is Finished.";
        return null;
    }
}