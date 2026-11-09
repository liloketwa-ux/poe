using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests;

public sealed class ExtendedApiTests : IAsyncLifetime
{
    private readonly RaceDayApiFactory _factory = new();
    private HttpClient _client = null!;
    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await _factory.InitialiseAsync();
    }
    public Task DisposeAsync() { _client.Dispose(); _factory.Dispose(); return Task.CompletedTask; }

    [Fact]
    public async Task SwaggerDocumentIsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("RaceDay REST API", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AzureUploadDoesNotPretendToSucceedWithoutCredentials()
    {
        await RegisterAndLogin("Organiser", "blob-owner@example.com");
        var eventResponse = await _client.PostAsJsonAsync("/api/events", ValidEvent());
        var ev = await eventResponse.Content.ReadFromJsonAsync<EventSummary>();
        Assert.NotNull(ev);
        using var form = new MultipartFormDataContent();
        using var image = new ByteArrayContent([137,80,78,71,13,10,26,10,0,0,0,0]);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(image, "file", "banner.png");
        var response = await _client.PostAsync($"/api/events/{ev!.EventId}/banner", form);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerAndAnonymousEventCatalogueCanBeUsedWithoutLogin()
    {
        var response = await _client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ParticipantCanUpdateOnlyTheirOwnProfile()
    {
        await RegisterAndLogin("Participant", "profile@example.com");
        var response = await _client.PutAsJsonAsync("/api/profile/me", new UpdateProfileRequest("Updated","Runner","0821234567"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await _client.GetFromJsonAsync<UserResponse>("/api/profile/me");
        Assert.Equal("Updated", profile!.FirstName);
    }

    private async Task RegisterAndLogin(string role, string email)
    {
        var register = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Race", "Tester", email, "Secret123!", role, null));
        register.EnsureSuccessStatusCode();
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Secret123!"));
        login.EnsureSuccessStatusCode();
    }
    private static EventRequest ValidEvent() => new("Storage Test Run", "Azure storage integration test",
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), "Test Stadium", "Johannesburg", "Gauteng", 10, "Run", "Open");
}
