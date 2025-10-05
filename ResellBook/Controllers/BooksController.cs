using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;
using ResellBook.Helpers;
using ResellBook.Services;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogService _logService;

    public BooksController(AppDbContext context, IWebHostEnvironment env, ILogService logService)
    {
        _context = context;
        _env = env;
        _logService = logService;
    }
    [HttpGet("ViewMyListings/{userId}")]
    public async Task<IActionResult> ViewMyListings(Guid userId)
    {
        try
        {
            await _logService.LogNormalAsync("BooksController", "ViewMyListings", 
                $"ViewMyListings request for userId: {userId}", Request.Path, userId.ToString());

            if (userId == Guid.Empty)
            {
                await _logService.LogCriticalAsync("BooksController", "ViewMyListings", 
                    "Invalid userId provided - Empty GUID", null, Request.Path, userId.ToString());
                return BadRequest(new { success = false, message = "Invalid user ID provided." });
            }

            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            await _logService.LogNormalAsync("BooksController", "ViewMyListings", 
                $"Retrieved {books.Count} books for user {userId}", Request.Path, userId.ToString());

            var result = books.Select(b => new
            {
                b.Id,
                b.BookName,
                b.AuthorOrPublication,
                b.Category,
                b.SubCategory,
                b.SellingPrice,
                Images = string.IsNullOrEmpty(b.ImagePathsJson)
                            ? Array.Empty<string>()
                            : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? Array.Empty<string>(),
                b.IsSold,
                CreatedAt = b.CreatedAt.ToString("dd/MM/yyyy hh:mm:ss tt")
            });

            return Ok(new { success = true, data = result, totalCount = books.Count });
        }
        catch (Exception ex)
        {
            await _logService.LogCriticalAsync("BooksController", "ViewMyListings", 
                "Unexpected error in ViewMyListings method", ex, Request.Path, userId.ToString());
                
            return StatusCode(500, new 
            { 
                success = false, 
                message = "Failed to retrieve your listings. Please try again later.",
                errorId = Guid.NewGuid().ToString()
            });
        }
    }


    [HttpPatch("MarkAsSold/{bookId}")]
    public async Task<IActionResult> MarkAsSold(Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { Message = "Book not found" });

        if (book.IsSold)
            return BadRequest(new { Message = "This book is already marked as sold." });

        book.IsSold = true;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Book marked as sold successfully." });
    }
    // DELETE: api/Books/Delete/{bookId}
    [HttpDelete("Delete/{bookId}")]
    public async Task<IActionResult> Delete(Guid bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { Message = "Book not found" });

        // Remove images from server if exist
        if (!string.IsNullOrEmpty(book.ImagePathsJson))
        {
            var images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(book.ImagePathsJson);
            if (images != null)
            {
                var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                foreach (var img in images)
                {
                    var filePath = Path.Combine(wwwRoot, img);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Book deleted successfully" });
    }
    // POST: List Book
    [HttpPost("ListBook")]
    public async Task<IActionResult> ListBook([FromForm] BookCreateDto dto)
    {
        try
        {
            await _logService.LogNormalAsync("BooksController", "ListBook", 
                $"Book listing request for userId: {dto.UserId}, BookName: {dto.BookName}", 
                Request.Path, dto.UserId.ToString());

            // Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                await _logService.LogCriticalAsync("BooksController", "ListBook", 
                    $"User not found with ID: {dto.UserId}", null, Request.Path, dto.UserId.ToString());
                return NotFound(new { success = false, message = "User not found" });
            }

            // Validate images
            if (dto.Images == null || dto.Images.Length < 2 || dto.Images.Length > 4)
            {
                await _logService.LogCriticalAsync("BooksController", "ListBook", 
                    $"Invalid image count: {dto.Images?.Length ?? 0}. Required: 2-4 images", 
                    null, Request.Path, dto.UserId.ToString());
                return BadRequest(new { success = false, message = "You must upload between 2 and 4 images." });
            }

            // Validate image sizes and types
            foreach (var image in dto.Images)
            {
                if (image.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    await _logService.LogCriticalAsync("BooksController", "ListBook", 
                        $"Image too large: {image.FileName} ({image.Length} bytes)", 
                        null, Request.Path, dto.UserId.ToString());
                    return BadRequest(new { success = false, message = $"Image {image.FileName} is too large. Maximum size is 5MB." });
                }

                var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedTypes.Contains(extension))
                {
                    await _logService.LogCriticalAsync("BooksController", "ListBook", 
                        $"Invalid image type: {image.FileName} ({extension})", 
                        null, Request.Path, dto.UserId.ToString());
                    return BadRequest(new { success = false, message = $"Invalid image type: {extension}. Allowed types: jpg, jpeg, png, webp" });
                }
            }

            // Ensure upload directory exists
            var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
            
            try
            {
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    await _logService.LogNormalAsync("BooksController", "ListBook", 
                        $"Created uploads directory: {uploadsFolder}", Request.Path, dto.UserId.ToString());
                }
            }
            catch (Exception dirEx)
            {
                await _logService.LogCriticalAsync("BooksController", "ListBook", 
                    "Failed to create uploads directory", dirEx, Request.Path, dto.UserId.ToString());
                return StatusCode(500, new { success = false, message = "Failed to prepare file storage. Please try again." });
            }

            // Process and save images
            var imagePaths = new List<string>();
            var savedFiles = new List<string>(); // Track saved files for cleanup on error

            try
            {
                foreach (var image in dto.Images)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var savePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    
                    var relativePath = Path.Combine("uploads/books", fileName).Replace('\\', '/');
                    imagePaths.Add(relativePath);
                    savedFiles.Add(savePath);
                }

                await _logService.LogNormalAsync("BooksController", "ListBook", 
                    $"Successfully saved {imagePaths.Count} images", Request.Path, dto.UserId.ToString());
            }
            catch (Exception imgEx)
            {
                // Cleanup any successfully saved files
                foreach (var filePath in savedFiles)
                {
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                await _logService.LogCriticalAsync("BooksController", "ListBook", 
                    "Failed to save images", imgEx, Request.Path, dto.UserId.ToString());
                return StatusCode(500, new { success = false, message = "Failed to save images. Please try again." });
            }

            // Create book record
            var book = new Book
            {
                UserId = dto.UserId,
                BookName = dto.BookName,
                AuthorOrPublication = dto.AuthorOrPublication,
                Category = dto.Category,
                SubCategory = dto.SubCategory,
                SellingPrice = dto.SellingPrice,
                ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(imagePaths),
                CreatedAt = IndianTimeHelper.UtcNow
            };

            try
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                await _logService.LogNormalAsync("BooksController", "ListBook", 
                    $"Book listed successfully with ID: {book.Id}", Request.Path, dto.UserId.ToString());

                return Ok(new 
                { 
                    success = true,
                    message = "Book listed successfully", 
                    bookId = book.Id,
                    imagesCount = imagePaths.Count
                });
            }
            catch (Exception dbEx)
            {
                // Cleanup uploaded files if database save fails
                foreach (var filePath in savedFiles)
                {
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                await _logService.LogCriticalAsync("BooksController", "ListBook", 
                    "Failed to save book to database", dbEx, Request.Path, dto.UserId.ToString());
                return StatusCode(500, new { success = false, message = "Failed to save book listing. Please try again." });
            }
        }
        catch (Exception ex)
        {
            await _logService.LogCriticalAsync("BooksController", "ListBook", 
                "Unexpected error in ListBook method", ex, Request.Path, dto.UserId.ToString());
                
            return StatusCode(500, new 
            { 
                success = false, 
                message = "An unexpected error occurred while listing your book. Please try again later.",
                errorId = Guid.NewGuid().ToString()
            });
        }
    }

    // PUT: Edit Book
    [HttpPut("EditListing/{id}")]
    public async Task<IActionResult> EditListing(Guid id, [FromForm] BookEditDto dto)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { Message = "Book not found" });

        // Update basic fields
        book.BookName = string.IsNullOrWhiteSpace(dto.BookName) ? book.BookName : dto.BookName;
        book.AuthorOrPublication = string.IsNullOrWhiteSpace(dto.AuthorOrPublication) ? book.AuthorOrPublication : dto.AuthorOrPublication;
        book.Category = string.IsNullOrWhiteSpace(dto.Category) ? book.Category : dto.Category;
        book.SubCategory = string.IsNullOrWhiteSpace(dto.SubCategory) ? book.SubCategory : dto.SubCategory;
        book.SellingPrice = dto.SellingPrice.HasValue ? dto.SellingPrice.Value : book.SellingPrice;

        // Deserialize existing images
        var currentImages = string.IsNullOrEmpty(book.ImagePathsJson)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(book.ImagePathsJson)!;

        // Keep only selected existing images
        if (dto.ExistingImages != null)
            currentImages = currentImages.Where(x => dto.ExistingImages.Contains(x)).ToList();

        // Add new images
        if (dto.NewImages != null && dto.NewImages.Length > 0)
        {
            var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            foreach (var image in dto.NewImages)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var savePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await image.CopyToAsync(stream);

                currentImages.Add(Path.Combine("uploads/books", fileName));
            }
        }

        book.ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(currentImages);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Book updated successfully" });
    }

[HttpGet("ViewAll/{userId}")]
public async Task<IActionResult> ViewAll(Guid userId)
{
    try
    {
        await _logService.LogNormalAsync("BooksController", "ViewAll", 
            $"ViewAll request received for userId: {userId}", Request.Path, userId.ToString());

        // Input validation
        if (userId == Guid.Empty)
        {
            await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                "Invalid userId provided - Empty GUID", null, Request.Path, userId.ToString());
            return BadRequest(new { success = false, message = "Invalid user ID provided." });
        }

        // Get current user location with timeout and error handling
        UserLocation? currentUserLocation = null;
        try
        {
            var locationTask = _context.UserLocations
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            // Add timeout to prevent hanging queries
            currentUserLocation = await locationTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                "Failed to fetch user location", ex, Request.Path, userId.ToString());
            return StatusCode(500, new { success = false, message = "Failed to retrieve user location." });
        }

        if (currentUserLocation == null)
        {
            await _logService.LogNormalAsync("BooksController", "ViewAll", 
                $"User location not found for userId: {userId}", Request.Path, userId.ToString());
            return BadRequest(new { success = false, message = "User location not found. Please update your location first." });
        }

        await _logService.LogNormalAsync("BooksController", "ViewAll", 
            "User location retrieved successfully", Request.Path, userId.ToString());

        // Get all books with user information - with proper error handling
        List<Book> booksData;
        try
        {
            booksData = await _context.Books
                .Include(b => b.User)  // Include User data to get UserName
                .Where(b => !b.IsSold)  // Only show available books
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync()
                .ConfigureAwait(false);
                
            await _logService.LogNormalAsync("BooksController", "ViewAll", 
                $"Retrieved {booksData.Count} books from database", Request.Path, userId.ToString());
        }
        catch (Exception ex)
        {
            await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                "Failed to fetch books data", ex, Request.Path, userId.ToString());
            return StatusCode(500, new { success = false, message = "Failed to retrieve books data." });
        }

        if (!booksData.Any())
        {
            await _logService.LogNormalAsync("BooksController", "ViewAll", 
                "No books found in database", Request.Path, userId.ToString());
            return Ok(new { success = true, data = new List<object>(), message = "No books available at the moment." });
        }

        // Get unique userIds from books
        var userIds = booksData.Select(b => b.UserId).Distinct().ToList();
        await _logService.LogNormalAsync("BooksController", "ViewAll", 
            $"Processing books from {userIds.Count} unique users", Request.Path, userId.ToString());

        // Fetch all user locations for these userIds - with error handling
        Dictionary<Guid, UserLocation> userLocations;
        try
        {
            userLocations = await _context.UserLocations
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u)
                .ConfigureAwait(false);
                
            await _logService.LogNormalAsync("BooksController", "ViewAll", 
                $"Retrieved locations for {userLocations.Count} users", Request.Path, userId.ToString());
        }
        catch (Exception ex)
        {
            await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                "Failed to fetch user locations", ex, Request.Path, userId.ToString());
            // Continue without locations rather than fail completely
            userLocations = new Dictionary<Guid, UserLocation>();
        }

        // Process books data safely
        var books = new List<object>();
        var processingErrors = 0;

        foreach (var book in booksData)
        {
            try
            {
                double? distanceKm = null;
                if (userLocations.ContainsKey(book.UserId))
                {
                    var loc = userLocations[book.UserId];
                    try
                    {
                        distanceKm = CalculateDistance(
                            currentUserLocation.Latitude,
                            currentUserLocation.Longitude,
                            loc.Latitude,
                            loc.Longitude
                        );
                    }
                    catch (Exception distEx)
                    {
                        await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                            $"Distance calculation failed for book {book.Id}", distEx, Request.Path, userId.ToString());
                        // Continue without distance
                    }
                }

                // Safe image parsing
                string[] images;
                try
                {
                    images = string.IsNullOrEmpty(book.ImagePathsJson)
                        ? Array.Empty<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<string[]>(book.ImagePathsJson) ?? Array.Empty<string>();
                }
                catch (Exception imgEx)
                {
                    await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                        $"Image JSON parsing failed for book {book.Id}", imgEx, Request.Path, userId.ToString());
                    images = Array.Empty<string>();
                }

                var bookResult = new
                {
                    book.Id,
                    book.UserId,        // Include the userId of who listed the book
                    UserName = book.User?.Name ?? "Unknown User",  // Safe access to User name
                    book.BookName,
                    book.AuthorOrPublication,
                    book.Category,
                    book.SubCategory,
                    book.SellingPrice,
                    book.IsSold,
                    Images = images,
                    CreatedAt = book.CreatedAt.ToString("dd/MM/yyyy hh:mm:ss tt"), // IST formatted
                    Distance = distanceKm.HasValue
                        ? (distanceKm < 1
                            ? $"{Math.Round(distanceKm.Value * 1000)} m"
                            : $"{Math.Round(distanceKm.Value, 2)} km")
                        : "N/A"
                };

                books.Add(bookResult);
            }
            catch (Exception bookEx)
            {
                processingErrors++;
                await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                    $"Failed to process book {book.Id}", bookEx, Request.Path, userId.ToString());
                // Continue with other books
            }
        }

        if (processingErrors > 0)
        {
            await _logService.LogCriticalAsync("BooksController", "ViewAll", 
                $"Encountered {processingErrors} errors while processing books", null, Request.Path, userId.ToString());
        }

        await _logService.LogNormalAsync("BooksController", "ViewAll", 
            $"Successfully processed {books.Count} books for user {userId}", Request.Path, userId.ToString());

        return Ok(new
        {
            success = true,
            data = books,
            totalCount = books.Count,
            processingErrors = processingErrors,
            message = processingErrors > 0 
                ? $"Retrieved {books.Count} books with {processingErrors} processing errors."
                : $"Successfully retrieved {books.Count} books."
        });
    }
    catch (Exception ex)
    {
        await _logService.LogCriticalAsync("BooksController", "ViewAll", 
            "Unexpected error in ViewAll method", ex, Request.Path, userId.ToString());
            
        return StatusCode(500, new 
        { 
            success = false, 
            message = "An unexpected error occurred while retrieving books. Please try again later.",
            errorId = Guid.NewGuid().ToString() // Provide error ID for tracking
        });
    }
}

    // Haversine formula to calculate distance (in km)
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371; // Radius of Earth in km
    var dLat = ToRadians(lat2 - lat1);
    var dLon = ToRadians(lon2 - lon1);

    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c; // distance in km
}

private double ToRadians(double angle) => Math.PI * angle / 180.0;
}

// DTOs

public class BookCreateDto
{
    [Required] public Guid UserId { get; set; }
    [Required] public required string BookName { get; set; }
    public string? AuthorOrPublication { get; set; }
    [Required] public required string Category { get; set; }
    public string? SubCategory { get; set; }
    [Required] public decimal SellingPrice { get; set; }

    [Required]
    public IFormFile[] Images { get; set; } = Array.Empty<IFormFile>();
}

public class BookEditDto
{
    public string? BookName { get; set; }
    public string? AuthorOrPublication { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public decimal? SellingPrice { get; set; }

    public IFormFile[]? NewImages { get; set; }
    public string[]? ExistingImages { get; set; }
}
