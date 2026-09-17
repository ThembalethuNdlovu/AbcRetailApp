using AbcRetailApp.Models;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private const string CartSessionKey = "Cart";
        private const string OrdersTable = "Orders";

        private readonly TableStorageService _tableService;
        private readonly QueueStorageService _queueService;

        public CheckoutController(TableStorageService tableService, QueueStorageService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        // GET: /Checkout
        public IActionResult Index()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Message"] = "Your cart is empty.";
                return RedirectToAction("Index", "Catalog");
            }

            ViewBag.GrandTotal = cart.Sum(c => c.LineTotal);
            return View(cart);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Message"] = "Your cart is empty.";
                return RedirectToAction("Index", "Catalog");
            }

            var customerEmail = User.Identity?.Name ?? "unknown@customer.com";
            var itemsSummary = string.Join(", ", cart.Select(c => $"{c.Quantity}x {c.ProductName}"));
            var total = cart.Sum(c => c.LineTotal);

            var order = new OrderEntity
            {
                CustomerEmail = customerEmail,
                ItemsSummary = itemsSummary,
                TotalAmount = total,
                Status = "Placed"
            };

            await _tableService.AddEntityAsync(OrdersTable, order);

            // Tie into existing Queue Storage — announces the new order for processing,
            // exactly like the "Processing Order" events from Project 1
            await _queueService.SendMessageAsync("Processing Order", $"Order for {customerEmail}: {itemsSummary} (Total: R{total:0.00})");

            // Clear the cart after successful checkout
            HttpContext.Session.Remove(CartSessionKey);

            TempData["Message"] = "Order placed successfully!";
            return RedirectToAction(nameof(Confirmation), new { rowKey = order.RowKey });
        }

        // GET: /Checkout/Confirmation
        public async Task<IActionResult> Confirmation(string rowKey)
        {
            var order = await _tableService.GetEntityAsync<OrderEntity>(OrdersTable, "Order", rowKey);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: /Checkout/History
        public async Task<IActionResult> History()
        {
            var customerEmail = User.Identity?.Name ?? "unknown@customer.com";
            var allOrders = await _tableService.GetAllEntitiesAsync<OrderEntity>(OrdersTable);
            var myOrders = allOrders
                .Where(o => o.CustomerEmail == customerEmail)
                .OrderByDescending(o => o.Timestamp)
                .ToList();

            return View(myOrders);
        }
    }
}