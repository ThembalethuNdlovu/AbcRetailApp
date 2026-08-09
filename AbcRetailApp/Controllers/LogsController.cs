using AbcRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AbcRetailApp.Controllers
{
    public class LogsController : Controller
    {
        private readonly FileStorageService _fileService;

        public LogsController(FileStorageService fileService)
        {
            _fileService = fileService;
        }

        // GET: /Logs
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.ListLogFilesAsync();
            return View(files);
        }

        // GET: /Logs/View?fileName=xxx
        public async Task<IActionResult> View(string fileName)
        {
            var content = await _fileService.ReadLogFileAsync(fileName);
            ViewBag.FileName = fileName;
            ViewBag.Content = content;
            return View();
        }

        // POST: /Logs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string logContent)
        {
            if (string.IsNullOrWhiteSpace(logContent))
            {
                TempData["Message"] = "Please enter some log content.";
                return RedirectToAction(nameof(Index));
            }

            var fileName = await _fileService.WriteLogFileAsync($"[{DateTime.UtcNow:u}] {logContent}");
            TempData["Message"] = $"Log file '{fileName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}