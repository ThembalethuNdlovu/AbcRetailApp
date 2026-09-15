using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace AbcRetailApp.Functions
{
    public class BlobFunction
    {
        private readonly ILogger<BlobFunction> _logger;
        private readonly IConfiguration _configuration;

        public BlobFunction(ILogger<BlobFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // POST /api/UploadBlob
        // Body: multipart/form-data with a single file field
        [Function("UploadBlob")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadBlob")] HttpRequestData req)
        {
            _logger.LogInformation("UploadBlob function triggered.");

            var contentType = req.Headers.TryGetValues("Content-Type", out var values) ? values.FirstOrDefault() : null;
            if (contentType == null || !contentType.Contains("multipart/form-data"))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please send the file as multipart/form-data.");
                return badResponse;
            }

            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary).Value;
            var reader = new MultipartReader(boundary!, req.Body);

            MultipartSection? section;
            byte[]? fileBytes = null;
            string fileName = $"{Guid.NewGuid()}.bin";

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var hasContentDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDisposition && contentDisposition!.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    fileName = $"{Guid.NewGuid()}_{contentDisposition.FileName.Value.Trim('"')}";
                    using var ms = new MemoryStream();
                    await section.Body.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }
            }

            if (fileBytes == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("No file was found in the request.");
                return badResponse;
            }

            var connectionString = _configuration["AzureStorage"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("product-images");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            using (var uploadStream = new MemoryStream(fileBytes))
            {
                await blobClient.UploadAsync(uploadStream, overwrite: true);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "File uploaded successfully to Azure Blob Storage.",
                blobName = fileName,
                url = blobClient.Uri.ToString()
            });
            return response;
        }
    }
}