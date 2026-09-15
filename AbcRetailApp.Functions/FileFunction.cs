using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AbcRetailApp.Functions
{
    public class FileFunction
    {
        private readonly ILogger<FileFunction> _logger;
        private readonly IConfiguration _configuration;

        public FileFunction(ILogger<FileFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // POST /api/WriteFile
        // Body (JSON): { "content": "Order #1023 processed successfully" }
        [Function("WriteFile")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WriteFile")] HttpRequestData req)
        {
            _logger.LogInformation("WriteFile function triggered.");

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<FileInput>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (input == null || string.IsNullOrWhiteSpace(input.Content))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please provide 'content' in the request body.");
                return badResponse;
            }

            var connectionString = _configuration["AzureStorage"];
            var shareClient = new ShareClient(connectionString, "logs-share");
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileName = $"{DateTime.UtcNow:yyyy-MM-dd_HHmmss}_function_log.txt";
            var fileClient = directoryClient.GetFileClient(fileName);

            var logText = $"[{DateTime.UtcNow:u}] {input.Content}";
            var bytes = Encoding.UTF8.GetBytes(logText);

            await fileClient.CreateAsync(bytes.Length);
            using (var stream = new MemoryStream(bytes))
            {
                await fileClient.UploadRangeAsync(new Azure.HttpRange(0, bytes.Length), stream);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "File written successfully to Azure Files.",
                fileName = fileName
            });
            return response;
        }
    }

    public class FileInput
    {
        public string Content { get; set; } = string.Empty;
    }
}