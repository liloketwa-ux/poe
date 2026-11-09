using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController(RaceDayDbContext db) : ControllerBase
{
    /// <summary>Gets the signed-in user's profile without exposing their password hash.</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken token)
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(new object?[] { id }, token);
        return user is null ? NotFound() : Ok(new UserResponse(user.UserId, user.FirstName, user.LastName,
            user.Email, user.Role, user.Phone, user.ProfileImageUrl));
    }

    /// <summary>Updates the signed-in user's own first name, last name and phone number.</summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMine(UpdateProfileRequest request, CancellationToken token)
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(new object?[] { id }, token);
        if (user is null) return NotFound();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.Phone?.Trim();
        await db.SaveChangesAsync(token);
        return Ok(new UserResponse(user.UserId, user.FirstName, user.LastName, user.Email, user.Role, user.Phone, user.ProfileImageUrl));
    }
}