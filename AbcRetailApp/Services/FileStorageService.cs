using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Text;

namespace AbcRetailApp.Services
{
    // Handles writing and listing log files in an Azure File Share
    public class FileStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareDirectoryClient _directoryClient;

        public FileStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            _shareClient = new ShareClient(connectionString, "logs-share");
            _shareClient.CreateIfNotExists();

            _directoryClient = _shareClient.GetRootDirectoryClient();
        }

        // Creates a new log file named with a timestamp, e.g. "2026-08-09_143210_log.txt"
        public async Task<string> WriteLogFileAsync(string logContent)
        {
            var fileName = $"{DateTime.UtcNow:yyyy-MM-dd_HHmmss}_log.txt";
            var fileClient = _directoryClient.GetFileClient(fileName);

            var bytes = Encoding.UTF8.GetBytes(logContent);
            using var stream = new MemoryStream(bytes);

            await fileClient.CreateAsync(bytes.Length);
            await fileClient.UploadRangeAsync(new Azure.HttpRange(0, bytes.Length), stream);

            return fileName;
        }

        public async Task<List<ShareFileItem>> ListLogFilesAsync()
        {
            var files = new List<ShareFileItem>();
            await foreach (var item in _directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory) files.Add(item);
            }
            return files;
        }

        public async Task<string> ReadLogFileAsync(string fileName)
        {
            var fileClient = _directoryClient.GetFileClient(fileName);
            var download = await fileClient.DownloadAsync();
            using var reader = new StreamReader(download.Value.Content);
            return await reader.ReadToEndAsync();
        }
    }
}