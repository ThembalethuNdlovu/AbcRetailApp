using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;

namespace AbcRetailApp.Services
{
    // Represents a single order/inventory event pushed onto the queue
    public class OrderMessage
    {
        public string MessageType { get; set; } = string.Empty;   // e.g. "Processing Order", "Image Uploaded", "Stock Update"
        public string Details { get; set; } = string.Empty;       // e.g. "imageName.jpg", "Order #1023 for John Smith"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;
        private readonly FileStorageService _fileService;

        public QueueStorageService(IConfiguration configuration, FileStorageService fileService)
        {
            _fileService = fileService;
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _queueClient = new QueueClient(connectionString, "order-processing", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(string messageType, string details)
        {
            var message = new OrderMessage { MessageType = messageType, Details = details };
            var json = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(json);

            // Log every queue event to Azure Files, regardless of where it was triggered from
            var logContent = $"[{DateTime.UtcNow:u}] Queue message sent - Type: {messageType}, Details: {details}";
            await _fileService.WriteLogFileAsync(logContent);
        }

        // Peeks at messages without removing them from the queue, so we can display them in the UI
        public async Task<List<OrderMessage>> PeekMessagesAsync(int maxMessages = 20)
        {
            var results = new List<OrderMessage>();
            PeekedMessage[] peeked = await _queueClient.PeekMessagesAsync(maxMessages);

            foreach (var msg in peeked)
            {
                try
                {
                    var order = JsonSerializer.Deserialize<OrderMessage>(msg.MessageText);
                    if (order != null) results.Add(order);
                }
                catch (JsonException)
                {
                    // Ignore malformed messages
                }
            }
            return results;
        }
    }
}