using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AbcRetailApp.Functions
{
    public class TableFunction
    {
        private readonly ILogger<TableFunction> _logger;
        private readonly IConfiguration _configuration;

        public TableFunction(ILogger<TableFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // POST /api/StoreTransaction
        // Body (JSON): { "customerName": "John Smith", "description": "Order #1023", "amount": 499.99 }
        [Function("StoreTransaction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "StoreTransaction")] HttpRequestData req)
        {
            _logger.LogInformation("StoreTransaction function triggered.");

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonSerializer.Deserialize<TransactionInput>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (input == null || string.IsNullOrWhiteSpace(input.CustomerName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please provide customerName, description, and amount in the request body.");
                return badResponse;
            }

            var connectionString = _configuration["AzureStorage"];
            var tableClient = new TableClient(connectionString, "FunctionTransactions");
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TransactionEntity
            {
                CustomerName = input.CustomerName,
                Description = input.Description ?? string.Empty,
                Amount = input.Amount
            };

            await tableClient.AddEntityAsync(entity);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Transaction stored successfully in Azure Table Storage.",
                partitionKey = entity.PartitionKey,
                rowKey = entity.RowKey
            });
            return response;
        }
    }

    public class TransactionInput
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Amount { get; set; }
    }
}