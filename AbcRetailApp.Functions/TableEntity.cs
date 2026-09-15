using Azure;
using Azure.Data.Tables;

namespace AbcRetailApp.Functions
{
    // Generic entity used by the Table Storage function to log transaction/event records
    public class TransactionEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Transaction";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Amount { get; set; }
    }
}