using Azure;
using Azure.Data.Tables;

namespace AbcRetailApp.Models
{
    // Represents a row in the "CustomerProfiles" Azure Table
    public class CustomerProfileEntity : ITableEntity
    {
        // PartitionKey groups related customers together (e.g. by region)
        public string PartitionKey { get; set; } = "Customer";

        // RowKey uniquely identifies the customer within the partition
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
    }
}