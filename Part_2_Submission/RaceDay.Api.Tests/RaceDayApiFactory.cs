using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public sealed class RaceDayApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<RaceDayDbContext>();
            var options = new DbContextOptionsBuilder<RaceDayDbContext>().UseSqlite(_connection).Options;
            services.AddSingleton(options);
            services.AddScoped(provider => new RaceDayDbContext(provider.GetRequiredService<DbContextOptions<RaceDayDbContext>>()));
        });
    }
    public async Task InitialiseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RaceDayDbContext>().Database.EnsureCreatedAsync();
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) _connection.Dispose(); }
}