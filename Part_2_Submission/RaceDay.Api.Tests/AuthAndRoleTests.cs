using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests;

public sealed class AuthAndRoleTests : IAsyncLifetime
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
    public async Task RegistrationRejectsInvalidRoleAndPersistsOnlyHash()
    {
        var bad = await _client.PostAsJsonAsync("/api/auth/register", new { firstName="Rae", lastName="Tester", email="bad@example.com", password="Secret123!", role="Admin" });
        Assert.True(bad.StatusCode == HttpStatusCode.BadRequest, await bad.Content.ReadAsStringAsync());
        var registered = await Register("Participant", "hashed@example.com");
        Assert.True(registered.StatusCode == HttpStatusCode.Created, await registered.Content.ReadAsStringAsync());
        var duplicate = await Register("Participant", "hashed@example.com");
        Assert.True(duplicate.StatusCode == HttpStatusCode.Conflict, await duplicate.Content.ReadAsStringAsync());
        using var scope = _factory.Services.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>().Users.Single(x => x.Email == "hashed@example.com");
        Assert.NotEqual("Secret123!", user.PasswordHash);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, "Secret123!"));
    }

    [Fact]
    public async Task LoginCreatesSessionAndRejectsInvalidCredentials()
    {
        await Register("Participant", "login@example.com");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login@example.com", "wrong-password"))).StatusCode);
        var good = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login@example.com", "Secret123!"));
        Assert.True(good.StatusCode == HttpStatusCode.OK, await good.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/profile/me")).StatusCode);
    }

    [Fact]
    public async Task AnonymousAndWrongRoleRequestsAreRejected()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync("/api/events", ValidEvent())).StatusCode);
        await Register("Participant", "participant@example.com"); await Login("participant@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.PostAsJsonAsync("/api/events", ValidEvent())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.GetAsync("/api/events/mine")).StatusCode);
    }

    [Fact]
    public async Task OrganiserCanCreateEventButNotEditAnotherOrganisersEvent()
    {
        await Register("Organiser", "organiser@example.com"); await Login("organiser@example.com");
        var created = await _client.PostAsJsonAsync("/api/events", ValidEvent());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var payload = await created.Content.ReadFromJsonAsync<EventSummary>();
        Assert.NotNull(payload);
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/events/{payload!.EventId}", ValidEvent() with { Name = "Updated Test Run" })).StatusCode);
        await Register("Organiser", "organiser2@example.com"); await Login("organiser2@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await _client.PutAsJsonAsync($"/api/events/{payload.EventId}", ValidEvent())).StatusCode);
    }

    [Fact]
    public async Task ParticipantCanEnrolAndEnrolmentPersists()
    {
        await Register("Organiser", "host@example.com"); await Login("host@example.com");
        var eventResponse = await _client.PostAsJsonAsync("/api/events", ValidEvent());
        var ev = await eventResponse.Content.ReadFromJsonAsync<EventSummary>();
        var catResponse = await _client.PostAsJsonAsync($"/api/events/{ev!.EventId}/categories", new CategoryRequest("10 km", 10, 100, 100, null, null));
        Assert.Equal(HttpStatusCode.Created, catResponse.StatusCode);
        var cat = await catResponse.Content.ReadFromJsonAsync<CategorySummary>();
        await Register("Participant", "runner@example.com"); await Login("runner@example.com");
        var enrol = await _client.PostAsJsonAsync($"/api/events/{ev.EventId}/enrolments", new EnrolRequest(cat!.CategoryId));
        Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);
        var mine = await _client.GetFromJsonAsync<List<EnrolmentResponse>>("/api/enrolments/me");
        Assert.Single(mine!); Assert.Equal(ev.EventId, mine![0].EventId);
    }

    private Task<HttpResponseMessage> Register(string role, string email) =>
        _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test", "Person", email, "Secret123!", role, null));
    private async Task Login(string email) =>
        (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Secret123!"))).EnsureSuccessStatusCode();
    private static EventRequest ValidEvent() => new("Test Run", "Testing event",
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), "Test Stadium", "Johannesburg", "Gauteng", 10, "Run", "Open");
}