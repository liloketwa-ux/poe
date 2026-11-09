using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers;

[Route("api/events/{eventId:int}/weather")]
[AllowAnonymous]
public sealed class WeatherController(RaceDayDbContext db, IHttpClientFactory clients) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int eventId, CancellationToken token)
    {
        var item = await db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.EventId == eventId, token);
        if (item is null) return NotFound(new { message = "Event not found." });
        try
        {
            var client = clients.CreateClient("Weather");
            var locationJson = await client.GetStringAsync($"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(item.City)}&count=1&language=en&format=json", token);
            using var locationDoc = JsonDocument.Parse(locationJson);
            if (!locationDoc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return NotFound(new { message = "Weather coordinates could not be found for this city." });
            var first = results[0];
            var lat = first.GetProperty("latitude").GetDouble();
            var lon = first.GetProperty("longitude").GetDouble();
            var forecastJson = await client.GetStringAsync($"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,weather_code,wind_speed_10m", token);
            using var forecastDoc = JsonDocument.Parse(forecastJson);
            return Ok(new { eventId, city = item.City, province = item.Province,
                source = "Open-Meteo", current = forecastDoc.RootElement.GetProperty("current").Clone() });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "The weather service is temporarily unavailable." });
        }
        catch (TaskCanceledException) when (!token.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "The weather service timed out." });
        }
    }
}