using Microsoft.AspNetCore.Http;
using RaceDay.Api.Services;

namespace RaceDay.Api.Tests;

public sealed class FakeFileStorageService : IFileStorageService
{
    public bool IsConfigured => true;
    public Task<string> UploadImageAsync(string folder, IFormFile file, CancellationToken cancellationToken) =>
        Task.FromResult($"https://storage.test/{folder}/test-banner.png");
}