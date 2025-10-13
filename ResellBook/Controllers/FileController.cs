using Microsoft.AspNetCore.Mvc;

namespace ResellBook.Controllers
{
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;

        public FileController(IWebHostEnvironment environment, ILogger<FileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Serve images directly from uploads/books folder
        /// This ensures your frontend URLs continue to work exactly as before
        /// </summary>
        [HttpGet("/uploads/books/{fileName}")]
        public ActionResult ServeBookImage(string fileName)
        {
            try
            {
                // Try multiple possible locations to find the file
                var contentRootPath = _environment.ContentRootPath;
                var webRootPath = _environment.WebRootPath;

                var possiblePaths = new[]
                {
                    // Standard locations
                    webRootPath != null ? Path.Combine(webRootPath, "uploads", "books", fileName) : null,
                    Path.Combine(contentRootPath, "wwwroot", "uploads", "books", fileName),
                    // Nested wwwroot (likely location based on your earlier test)
                    Path.Combine(contentRootPath, "wwwroot", "wwwroot", "uploads", "books", fileName),
                    // Alternative locations
                    Path.Combine(contentRootPath, "uploads", "books", fileName),
                    Path.Combine(contentRootPath, "site", "wwwroot", "uploads", "books", fileName)
                };

                string actualFilePath = null;
                foreach (var path in possiblePaths.Where(p => !string.IsNullOrEmpty(p)))
                {
                    if (System.IO.File.Exists(path))
                    {
                        actualFilePath = path;
                        _logger.LogInformation($"Found file at: {path}");
                        break;
                    }
                }

                if (actualFilePath == null)
                {
                    _logger.LogWarning($"File not found: {fileName}. Searched paths: {string.Join(", ", possiblePaths.Where(p => !string.IsNullOrEmpty(p)))}");
                    return NotFound($"Image not found: {fileName}");
                }

                // Determine content type
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                // Return the file
                var fileBytes = System.IO.File.ReadAllBytes(actualFilePath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving file: {fileName}");
                return StatusCode(500, $"Error serving file: {fileName}");
            }
        }

        /// <summary>
        /// Debug endpoint to show all possible file locations
        /// </summary>
        [HttpGet("/uploads/books/debug/{fileName}")]
        public ActionResult DebugFileLocations(string fileName)
        {
            var contentRootPath = _environment.ContentRootPath;
            var webRootPath = _environment.WebRootPath;

            var possiblePaths = new[]
            {
                webRootPath != null ? Path.Combine(webRootPath, "uploads", "books", fileName) : null,
                Path.Combine(contentRootPath, "wwwroot", "uploads", "books", fileName),
                Path.Combine(contentRootPath, "wwwroot", "wwwroot", "uploads", "books", fileName),
                Path.Combine(contentRootPath, "uploads", "books", fileName),
                Path.Combine(contentRootPath, "site", "wwwroot", "uploads", "books", fileName)
            };

            var result = new
            {
                FileName = fileName,
                ContentRootPath = contentRootPath,
                WebRootPath = webRootPath,
                SearchedPaths = possiblePaths.Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => new { Path = p, Exists = System.IO.File.Exists(p) })
                    .ToArray(),
                FoundAt = possiblePaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && System.IO.File.Exists(p))
            };

            return Ok(result);
        }
    }
}