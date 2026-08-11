using Azure;
using Azure.Data.Tables;

namespace AbcRetailApp.Services
{
    // Wraps Azure.Data.Tables so controllers don't talk to the SDK directly.
    // Works for any entity type that implements ITableEntity (CustomerProfileEntity, ProductEntity, ...)
    public class TableStorageService
    {
        private readonly TableServiceClient _serviceClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _serviceClient = new TableServiceClient(connectionString);
        }

        private TableClient GetTableClient(string tableName)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            tableClient.CreateIfNotExists();
            return tableClient;
        }

        public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var table = GetTableClient(tableName);
            await table.AddEntityAsync(entity);
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            var table = GetTableClient(tableName);
            var results = new List<T>();
            await foreach (var entity in table.QueryAsync<T>())
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var table = GetTableClient(tableName);
            try
            {
                var response = await table.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var table = GetTableClient(tableName);
            await table.UpdateEntityAsync(entity, Azure.ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
        {
            var table = GetTableClient(tableName);
            await table.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}