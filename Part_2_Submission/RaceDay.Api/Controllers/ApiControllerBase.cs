using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase(RaceDayDbContext db) : ControllerBase
{
    protected RaceDayDbContext Db { get; } = db;
    protected int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    protected Task<User?> CurrentUserAsync(CancellationToken token = default) => Db.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId, token);
    protected async Task<bool> OwnsEventAsync(int eventId, CancellationToken token = default) =>
        await Db.Events.AnyAsync(e => e.EventId == eventId && e.OrganiserId == CurrentUserId, token);
    protected static UserResponse ToUserResponse(User user) => new(user.UserId, user.FirstName, user.LastName,
        user.Email, user.Role, user.Phone, user.ProfileImageUrl);
}
