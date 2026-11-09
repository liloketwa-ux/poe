using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace RaceDay.Api.Services;

public interface IFileStorageService
{
    bool IsConfigured { get; }
    Task<string> UploadImageAsync(string folder, IFormFile file, CancellationToken cancellationToken);
}

public sealed class AzureBlobFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private readonly string? _connectionString = configuration["AzureBlobStorage:ConnectionString"];
    private readonly string _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-media";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public async Task<string> UploadImageAsync(string folder, IFormFile file, CancellationToken cancellationToken)
    {
        if (!IsConfigured) throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage__ConnectionString.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var blobName = $"{folder.Trim('/')}/{safeName}";
        var service = new BlobServiceClient(_connectionString);
        var container = service.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None, cancellationToken: cancellationToken);
        var blob = container.GetBlobClient(blobName);
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: cancellationToken);
        if (blob.CanGenerateSasUri)
        {
            var sas = new BlobSasBuilder
            {
                BlobContainerName = _containerName, BlobName = blobName, Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(30)
            };
            sas.SetPermissions(BlobSasPermissions.Read);
            return blob.GenerateSasUri(sas).ToString();
        }
        return blob.Uri.ToString();
    }
}