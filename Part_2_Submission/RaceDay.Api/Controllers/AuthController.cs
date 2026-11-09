using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>Registration, login and logout for the Organiser and Participant roles.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(RaceDayDbContext db, IPasswordHasher<User> passwordHasher) : ControllerBase
{
    /// <summary>Registers a new user. Public registration accepts only Organiser or Participant.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken token)
    {
        var role = request.Role.Trim();
        if (!string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Role must be Organiser or Participant." });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, token))
            return Conflict(new { message = "An account with this email already exists." });
        var user = new User
        {
            FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(),
            Email = email, Role = char.ToUpperInvariant(role[0]) + role[1..].ToLowerInvariant(),
            Phone = request.Phone?.Trim()
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(token);
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    /// <summary>Verifies credentials and creates an HTTP-only authenticated session cookie.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken token)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email && x.IsActive, token);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Email or password is incorrect." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role);
        return Ok(ToResponse(user));
    }

    /// <summary>Ends the authenticated session and clears the session state.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private static UserResponse ToResponse(User user) => new(user.UserId, user.FirstName, user.LastName,
        user.Email, user.Role, user.Phone, user.ProfileImageUrl);
}
