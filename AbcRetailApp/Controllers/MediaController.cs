using AbcRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    public class MediaController : Controller
    {
        private readonly BlobStorageService _blobService;
        private readonly QueueStorageService _queueService;

        public MediaController(BlobStorageService blobService, QueueStorageService queueService)
        {
            _blobService = blobService;
            _queueService = queueService;
        }

        // GET: /Media
        public async Task<IActionResult> Index()
        {
            var blobs = await _blobService.ListBlobsAsync();

            // Pass name + URL pairs to the view
            var items = blobs.Select(b => new
            {
                Name = b.Name,
                Url = _blobService.GetBlobUrl(b.Name)
            }).ToList();

            ViewBag.MediaItems = items;
            return View();
        }

        // POST: /Media/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please choose a file to upload.";
                return RedirectToAction(nameof(Index));
            }

            var blobName = await _blobService.UploadBlobAsync(file);

            // Log the upload event to the queue, as required by the brief
            await _queueService.SendMessageAsync("Image Uploaded", blobName);

            TempData["Message"] = $"File '{file.FileName}' uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Media/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string blobName)
        {
            await _blobService.DeleteBlobAsync(blobName);
            TempData["Message"] = "File deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}