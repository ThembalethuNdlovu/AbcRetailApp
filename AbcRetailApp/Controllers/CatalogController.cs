using AbcRetailApp.Models;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    // Customer-facing, read-only product browsing — separate from the Admin ProductsController,
    // which handles Create/Edit/Delete and is locked to Admin only.
    [Authorize(Roles = "Customer")]
    public class CatalogController : Controller
    {
        private const string TableName = "Products";
        private readonly TableStorageService _tableService;
        private readonly BlobStorageService _blobService;

        public CatalogController(TableStorageService tableService, BlobStorageService blobService)
        {
            _tableService = tableService;
            _blobService = blobService;
        }

        // GET: /Catalog
        public async Task<IActionResult> Index(string? search)
        {
            var products = await _tableService.GetAllEntitiesAsync<ProductEntity>(TableName);

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products
                    .Where(p => p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.BlobUrls = (await _blobService.ListBlobsAsync())
                .ToDictionary(b => b.Name, b => _blobService.GetBlobUrl(b.Name));

            ViewBag.SearchTerm = search;

            return View(products);
        }
    }
}