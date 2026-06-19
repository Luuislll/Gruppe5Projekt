using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Gruppe5Projekt.Services;

/// <summary>
/// Kapselt das Hoch- und Herunterladen von Kapitel-Material (PDFs)
/// im Azure Blob Storage Container "kapitel-material".
/// </summary>
public class KapitelMaterialService
{
    private const string ContainerName = "kapitel-material";
    private readonly BlobServiceClient _blobServiceClient;

    public KapitelMaterialService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    private async Task<BlobContainerClient> GetContainerAsync()
    {
        var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await container.CreateIfNotExistsAsync();
        return container;
    }

    /// <summary>
    /// Lädt eine Datei hoch und gibt den Blob-Namen zurück, der zur
    /// späteren Identifikation in der Datenbank gespeichert wird.
    /// </summary>
    public async Task<string> UploadAsync(int kapitelId, IFormFile datei)
    {
        var container = await GetContainerAsync();

        // Pro Kapitel ein eigenes "Verzeichnis", damit Dateinamen kollisionsfrei sind.
        var blobName = $"{kapitelId}/{datei.FileName}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = datei.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = datei.ContentType }
        });

        return blobName;
    }

    /// <summary>
    /// Lädt einen Blob herunter. Gibt null zurück, wenn der Blob nicht existiert.
    /// </summary>
    public async Task<(Stream Content, string ContentType, string FileName)?> DownloadAsync(string blobName)
    {
        var container = await GetContainerAsync();
        var blob = container.GetBlobClient(blobName);

        if (!await blob.ExistsAsync())
        {
            return null;
        }

        var response = await blob.DownloadStreamingAsync();
        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
        var fileName = Path.GetFileName(blobName);

        return (response.Value.Content, contentType, fileName);
    }

    /// <summary>
    /// Löscht einen Blob, falls vorhanden.
    /// </summary>
    public async Task DeleteAsync(string? blobName)
    {
        if (string.IsNullOrEmpty(blobName))
        {
            return;
        }

        var container = await GetContainerAsync();
        await container.DeleteBlobIfExistsAsync(blobName);
    }
}