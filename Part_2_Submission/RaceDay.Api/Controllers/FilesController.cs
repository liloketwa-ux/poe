using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;
using System.Security.Claims;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class FilesController(RaceDayDbContext db, IFileStorageService storage) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];

    [HttpPost("events/{eventId:int}/banner")]
    [Authorize(Roles = "Organiser")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile file, CancellationToken token)
    {
        var item = await db.Events.FirstOrDefaultAsync(x => x.EventId == eventId, token);
        if (item is null) return NotFound();
        if (item.OrganiserId != UserId) return Forbid();
        var error = Validate(file); if (error is not null) return BadRequest(new { message = error });
        try { item.BannerImageUrl = await storage.UploadImageAsync("event-banners", file, token); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
        await db.SaveChangesAsync(token);
        return Ok(new { eventId, imageUrl = item.BannerImageUrl });
    }

    [HttpPost("profile/me/picture")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken token)
    {
        var item = await db.Users.FindAsync(new object?[] { UserId }, token);
        if (item is null) return NotFound();
        var error = Validate(file); if (error is not null) return BadRequest(new { message = error });
        try { item.ProfileImageUrl = await storage.UploadImageAsync("profile-pictures", file, token); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
        await db.SaveChangesAsync(token);
        return Ok(new { imageUrl = item.ProfileImageUrl });
    }

    private static string? Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0) return "Choose an image file.";
        if (file.Length > 5_242_880) return "Image must be 5 MB or smaller.";
        if (!AllowedTypes.Contains(file.ContentType)) return "Only JPEG, PNG and WebP images are allowed.";
        return null;
    }
}