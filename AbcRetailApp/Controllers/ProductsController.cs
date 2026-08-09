using AbcRetailApp.Models;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    public class ProductsController : Controller
    {
        private const string TableName = "Products";
        private readonly TableStorageService _tableService;
        private readonly BlobStorageService _blobService;
        private readonly QueueStorageService _queueService;

        public ProductsController(TableStorageService tableService, BlobStorageService blobService, QueueStorageService queueService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllEntitiesAsync<ProductEntity>(TableName);

            // Build a lookup of blob names -> URLs so the view can display images
            ViewBag.BlobUrls = (await _blobService.ListBlobsAsync())
                .ToDictionary(b => b.Name, b => _blobService.GetBlobUrl(b.Name));

            return View(products);
        }

        // GET: /Products/Create
        public async Task<IActionResult> Create()
        {
            var blobs = await _blobService.ListBlobsAsync();
            ViewBag.AvailableImages = blobs.Select(b => b.Name).ToList();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEntity product)
        {
            if (!ModelState.IsValid) return View(product);

            await _tableService.AddEntityAsync(TableName, product);

            // Log a queue message for inventory tracking, as required by the brief
            await _queueService.SendMessageAsync("Stock Update", $"New product added: {product.ProductName} (Qty: {product.StockQuantity})");

            TempData["Message"] = $"Product '{product.ProductName}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetEntityAsync<ProductEntity>(TableName, partitionKey, rowKey);
            if (product == null) return NotFound();

            var blobs = await _blobService.ListBlobsAsync();
            ViewBag.AvailableImages = blobs.Select(b => b.Name).ToList();

            return View(product);
        }

        // POST: /Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEntity product)
        {
            if (!ModelState.IsValid) return View(product);

            await _tableService.UpdateEntityAsync(TableName, product);
            TempData["Message"] = $"Product '{product.ProductName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Products/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);
            TempData["Message"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}