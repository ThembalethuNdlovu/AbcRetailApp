using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AbcRetailApp.Services
{
    // Handles uploading, listing, and deleting images / multimedia in Azure Blob Storage
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            var serviceClient = new BlobServiceClient(connectionString);

            // Container that holds product images / multimedia content
            _containerClient = serviceClient.GetBlobContainerClient("product-images");
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadBlobAsync(IFormFile file)
        {
            // Ensure a unique blob name so uploads never overwrite each other
            var blobName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobName;
        }

        public async Task<List<BlobItem>> ListBlobsAsync()
        {
            var blobs = new List<BlobItem>();
            await foreach (var blob in _containerClient.GetBlobsAsync())
            {
                blobs.Add(blob);
            }
            return blobs;
        }

        public string GetBlobUrl(string blobName)
        {
            return _containerClient.GetBlobClient(blobName).Uri.ToString();
        }

        public async Task DeleteBlobAsync(string blobName)
        {
            await _containerClient.GetBlobClient(blobName).DeleteIfExistsAsync();
        }
    }
}