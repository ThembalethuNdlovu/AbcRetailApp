using AbcRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueStorageService _queueService;
        private readonly FileStorageService _fileService;

        public OrdersController(QueueStorageService queueService, FileStorageService fileService)
        {
            _queueService = queueService;
            _fileService = fileService;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var messages = await _queueService.PeekMessagesAsync();
            return View(messages);
        }

        // POST: /Orders/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string messageType, string details)
        {
            if (string.IsNullOrWhiteSpace(messageType) || string.IsNullOrWhiteSpace(details))
            {
                TempData["Message"] = "Please provide both a message type and details.";
                return RedirectToAction(nameof(Index));
            }

            await _queueService.SendMessageAsync(messageType, details);

            TempData["Message"] = "Order/inventory message sent to queue and logged.";
            return RedirectToAction(nameof(Index));
        }
    }
}