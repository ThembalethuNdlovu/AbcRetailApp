using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AbcRetailApp.Functions
{
    public class QueueFunction
    {
        private readonly ILogger<QueueFunction> _logger;
        private readonly IConfiguration _configuration;

        public QueueFunction(ILogger<QueueFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // POST /api/QueueTransaction
        // Body (JSON): { "messageType": "Processing Order", "details": "Order #1023 for John Smith" }
        // Writes a message to the queue, then reads back the most recent messages to confirm delivery.
        [Function("QueueTransaction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "QueueTransaction")] HttpRequestData req)
        {
            _logger.LogInformation("QueueTransaction function triggered.");

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<QueueInput>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (input == null || string.IsNullOrWhiteSpace(input.MessageType) || string.IsNullOrWhiteSpace(input.Details))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please provide messageType and details in the request body.");
                return badResponse;
            }

            var connectionString = _configuration["AzureStorage"];
            var queueClient = new QueueClient(connectionString, "order-processing", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queueClient.CreateIfNotExistsAsync();

            // WRITE to the queue
            var messagePayload = JsonSerializer.Serialize(new
            {
                MessageType = input.MessageType,
                Details = input.Details,
                CreatedAt = DateTime.UtcNow
            });
            await queueClient.SendMessageAsync(messagePayload);

            // READ from the queue (peek, so we don't remove messages other parts of the app rely on)
            PeekedMessage[] peeked = await queueClient.PeekMessagesAsync(maxMessages: 5);
            var peekedContents = peeked.Select(m => m.MessageText).ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Message sent to Azure Queue Storage successfully.",
                sentMessage = messagePayload,
                recentMessagesInQueue = peekedContents
            });
            return response;
        }
    }

    public class QueueInput
    {
        public string MessageType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}