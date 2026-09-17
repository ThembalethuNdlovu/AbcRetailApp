using Azure;
using Azure.Data.Tables;

namespace AbcRetailApp.Models
{
    // Represents a completed customer order, stored in the "Orders" Azure Table
    public class OrderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Order";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerEmail { get; set; } = string.Empty;
        public string ItemsSummary { get; set; } = string.Empty; // human-readable summary, e.g. "2x Dirt Bike, 1x Tea Bags"
        public double TotalAmount { get; set; }
        public string Status { get; set; } = "Placed";
    }
}