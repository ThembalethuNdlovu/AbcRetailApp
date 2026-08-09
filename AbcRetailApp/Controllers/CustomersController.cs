using AbcRetailApp.Models;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    public class CustomersController : Controller
    {
        private const string TableName = "CustomerProfiles";
        private readonly TableStorageService _tableService;

        public CustomersController(TableStorageService tableService)
        {
            _tableService = tableService;
        }

        // GET: /Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllEntitiesAsync<CustomerProfileEntity>(TableName);
            return View(customers);
        }

        // GET: /Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfileEntity customer)
        {
            if (!ModelState.IsValid) return View(customer);

            await _tableService.AddEntityAsync(TableName, customer);
            TempData["Message"] = $"Customer '{customer.FullName}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Customers/Edit?partitionKey=Customer&rowKey=xxx
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<CustomerProfileEntity>(TableName, partitionKey, rowKey);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: /Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerProfileEntity customer)
        {
            if (!ModelState.IsValid) return View(customer);

            await _tableService.UpdateEntityAsync(TableName, customer);
            TempData["Message"] = $"Customer '{customer.FullName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Customers/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);
            TempData["Message"] = "Customer deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}