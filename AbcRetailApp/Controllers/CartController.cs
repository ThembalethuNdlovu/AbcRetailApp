using AbcRetailApp.Models;
using AbcRetailApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private const string TableName = "Products";
        private const string CartSessionKey = "Cart";
        private readonly TableStorageService _tableService;

        public CartController(TableStorageService tableService)
        {
            _tableService = tableService;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.GrandTotal = cart.Sum(c => c.LineTotal);
            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetEntityAsync<ProductEntity>(TableName, partitionKey, rowKey);
            if (product == null)
            {
                TempData["Message"] = "Product not found.";
                return RedirectToAction("Index", "Catalog");
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductRowKey == rowKey);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductPartitionKey = product.PartitionKey,
                    ProductRowKey = product.RowKey,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1,
                    ImageBlobName = product.ImageBlobName
                });
            }

            SaveCart(cart);
            TempData["Message"] = $"'{product.ProductName}' added to cart.";
            return RedirectToAction("Index", "Catalog");
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(string rowKey, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductRowKey == rowKey);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string rowKey)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductRowKey == rowKey);
            SaveCart(cart);
            TempData["Message"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }
    }
}