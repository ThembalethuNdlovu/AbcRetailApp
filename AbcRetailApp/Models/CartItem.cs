namespace AbcRetailApp.Models
{
    // Represents one line item in a customer's shopping cart, held in Session (not Azure Storage)
    public class CartItem
    {
        public string ProductPartitionKey { get; set; } = string.Empty;
        public string ProductRowKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageBlobName { get; set; }

        public double LineTotal => Price * Quantity;
    }
}