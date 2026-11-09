using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDayRoute = RaceDay.Api.Models.Route;

namespace RaceDay.Api.Controllers;

[Route("api/events/{eventId:int}/route")]
public sealed class RoutesController(RaceDayDbContext db) : ApiControllerBase(db)
{
    /// <summary>Gets public course route information for an event.</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int eventId, CancellationToken token)
    {
        if (!await Db.Events.AnyAsync(x => x.EventId == eventId, token)) return NotFound();
        var route = await Db.Routes.AsNoTracking().FirstOrDefaultAsync(x => x.EventId == eventId, token);
        return route is null ? NotFound() : Ok(ToResponse(route));
    }

    /// <summary>Creates or replaces course route information for an event owned by the Organiser.</summary>
    /// <remarks>JSON body: startLocation, finishLocation, routeUrl, elevationGainM and notes.</remarks>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Put(int eventId, RouteRequest request, CancellationToken token)
    {
        if (!await Db.Events.AnyAsync(x => x.EventId == eventId, token)) return NotFound();
        if (!await OwnsEventAsync(eventId, token)) return Forbid();
        if (request.ElevationGainM is < 0) return BadRequest(new { message = "Elevation gain cannot be negative." });
        var item = await Db.Routes.FirstOrDefaultAsync(x => x.EventId == eventId, token);
        if (item is null) { item = new RaceDayRoute { EventId = eventId }; Db.Routes.Add(item); }
        item.StartLocation = request.StartLocation.Trim(); item.FinishLocation = request.FinishLocation.Trim();
        item.RouteUrl = request.RouteUrl; item.ElevationGainM = request.ElevationGainM; item.Notes = request.Notes;
        await Db.SaveChangesAsync(token); return Ok(ToResponse(item));
    }
    private static RouteResponse ToResponse(RaceDayRoute x) => new(x.RouteId, x.EventId, x.StartLocation,
        x.FinishLocation, x.RouteUrl, x.ElevationGainM, x.Notes);
}