using AbcRetailApp.Models;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly TableStorageService _tableService;
        private readonly BlobStorageService _blobService;
        private readonly QueueStorageService _queueService;
        private readonly FileStorageService _fileService;

        public DashboardController(
            TableStorageService tableService,
            BlobStorageService blobService,
            QueueStorageService queueService,
            FileStorageService fileService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllEntitiesAsync<CustomerProfileEntity>("CustomerProfiles");
            var products = await _tableService.GetAllEntitiesAsync<ProductEntity>("Products");
            var orders = await _tableService.GetAllEntitiesAsync<OrderEntity>("Orders");
            var blobs = await _blobService.ListBlobsAsync();
            var logFiles = await _fileService.ListLogFilesAsync();
            var queueMessages = await _queueService.PeekMessagesAsync(20);

            const int lowStockThreshold = 3;
            var lowStockProducts = products.Where(p => p.StockQuantity <= lowStockThreshold).ToList();

            ViewBag.CustomerCount = customers.Count;
            ViewBag.ProductCount = products.Count;
            ViewBag.OrderCount = orders.Count;
            ViewBag.MediaCount = blobs.Count;
            ViewBag.LogCount = logFiles.Count;
            ViewBag.QueueMessageCount = queueMessages.Count;
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
            ViewBag.LowStockProducts = lowStockProducts;
            ViewBag.RecentOrders = orders.OrderByDescending(o => o.Timestamp).Take(5).ToList();

            return View();
        }
    }
}