using Microsoft.AspNetCore.Mvc;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileTestController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileTestController> _logger;

        public FileTestController(IWebHostEnvironment environment, ILogger<FileTestController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to check what files exist in wwwroot
        /// </summary>
        [HttpGet("CheckFiles")]
        public ActionResult CheckFiles()
        {
            try
            {
                // Try multiple possible paths
                var contentRootPath = _environment.ContentRootPath;
                var webRootPath = _environment.WebRootPath;
                
                var possiblePaths = new[]
                {
                    webRootPath,
                    Path.Combine(contentRootPath, "wwwroot"),
                    Path.Combine(contentRootPath, "wwwroot", "wwwroot"), // Check for nested wwwroot
                    contentRootPath
                };

                string actualWwwrootPath = null;
                foreach (var path in possiblePaths.Where(p => !string.IsNullOrEmpty(p)))
                {
                    if (Directory.Exists(Path.Combine(path, "uploads", "books")))
                    {
                        actualWwwrootPath = path;
                        break;
                    }
                }

                var wwwrootPath = actualWwwrootPath ?? (webRootPath ?? Path.Combine(contentRootPath, "wwwroot"));
                var uploadsPath = Path.Combine(wwwrootPath, "uploads", "books");

                var result = new
                {
                    ContentRootPath = contentRootPath,
                    WebRootPath = webRootPath,
                    ActualWwwrootPath = actualWwwrootPath,
                    WwwrootPath = wwwrootPath,
                    WwwrootExists = Directory.Exists(wwwrootPath),
                    UploadsPath = uploadsPath,
                    UploadsExists = Directory.Exists(uploadsPath),
                    Files = Directory.Exists(uploadsPath) 
                        ? Directory.GetFiles(uploadsPath).Select(f => Path.GetFileName(f)).ToArray()
                        : Array.Empty<string>(),
                    AllPossiblePaths = possiblePaths.Where(p => !string.IsNullOrEmpty(p))
                        .Select(p => new { Path = p, Exists = Directory.Exists(p) })
                        .ToArray(),
                    AllWwwrootContents = Directory.Exists(wwwrootPath)
                        ? GetDirectoryContents(wwwrootPath)
                        : Array.Empty<string>()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking files");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private string[] GetDirectoryContents(string path)
        {
            try
            {
                var contents = new List<string>();
                
                // Get all files recursively
                var allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (var file in allFiles)
                {
                    var relativePath = Path.GetRelativePath(path, file);
                    contents.Add($"FILE: {relativePath}");
                }

                // Get all directories
                var allDirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                foreach (var dir in allDirs)
                {
                    var relativePath = Path.GetRelativePath(path, dir);
                    contents.Add($"DIR: {relativePath}");
                }

                return contents.ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Test a specific file accessibility
        /// </summary>
        [HttpGet("TestFile/{fileName}")]
        public ActionResult TestFile(string fileName)
        {
            try
            {
                // Try multiple possible paths to find the file
                var contentRootPath = _environment.ContentRootPath;
                var webRootPath = _environment.WebRootPath;
                
                var possibleWwwrootPaths = new[]
                {
                    webRootPath,
                    Path.Combine(contentRootPath, "wwwroot"),
                    Path.Combine(contentRootPath, "wwwroot", "wwwroot"), // Check for nested wwwroot
                    contentRootPath
                };

                string actualFilePath = null;
                string actualWwwrootPath = null;
                
                foreach (var wwwrootPath in possibleWwwrootPaths.Where(p => !string.IsNullOrEmpty(p)))
                {
                    var testPath = Path.Combine(wwwrootPath, "uploads", "books", fileName);
                    if (System.IO.File.Exists(testPath))
                    {
                        actualFilePath = testPath;
                        actualWwwrootPath = wwwrootPath;
                        break;
                    }
                }

                var filePath = actualFilePath ?? Path.Combine(webRootPath ?? Path.Combine(contentRootPath, "wwwroot"), "uploads", "books", fileName);

                var result = new
                {
                    FileName = fileName,
                    FilePath = filePath,
                    ActualFilePath = actualFilePath,
                    ActualWwwrootPath = actualWwwrootPath,
                    FileExists = System.IO.File.Exists(filePath),
                    ActualFileExists = actualFilePath != null,
                    ExpectedUrl = $"https://resellbook20250929183655.azurewebsites.net/uploads/books/{fileName}",
                    FileSize = System.IO.File.Exists(filePath) ? new FileInfo(filePath).Length : 0,
                    AllTestedPaths = possibleWwwrootPaths.Where(p => !string.IsNullOrEmpty(p))
                        .Select(p => new { 
                            WwwrootPath = p, 
                            TestPath = Path.Combine(p, "uploads", "books", fileName),
                            Exists = System.IO.File.Exists(Path.Combine(p, "uploads", "books", fileName))
                        })
                        .ToArray()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error testing file {fileName}");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}