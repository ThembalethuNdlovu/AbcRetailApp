using Azure;
using Azure.Data.Tables;

namespace AbcRetailApp.Models
{
    // Represents a row in the "Products" Azure Table
    public class ProductEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int StockQuantity { get; set; }

        // Name of the blob (image) associated with this product, if any
        public string? ImageBlobName { get; set; }
    }
}