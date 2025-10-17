using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Helpers;
using ResellBook.Models;
using ResellBook.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public BooksController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    [HttpPost("UserClick/{bookId}")]
    public async Task<IActionResult> UserClick(Guid bookId)
    {
        try
        {
            int updatedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Books SET Views = Views + 1 WHERE Id = {bookId}"
            );

            if (updatedRows == 0)
                return NotFound(new { Message = "Book not found" });

            var totalViews = await _context.Books
                .Where(b => b.Id == bookId)
                .Select(b => b.Views)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                Message = "Book view updated successfully",
                BookId = bookId,
                TotalViews = totalViews
            });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "UserClick", "UserClick failed", ex, bookId.ToString());
            return StatusCode(500, "Failed to update book view count.");
        }
    }
    [HttpGet("ViewMyListings/{userId}")]

    public async Task<IActionResult> ViewMyListings(Guid userId)
    {
        try
        {
            SimpleLogger.LogNormal("BooksController", "ViewMyListings", $"Request for userId: {userId}", userId.ToString());

            var books = await _context.Books
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var result = books.Select(b => new
            {
                b.Id,
                b.BookName,
                b.AuthorOrPublication,
                b.Category,
                b.Description,
                b.SubCategory,
                b.SellingPrice,
                Images = string.IsNullOrEmpty(b.ImagePathsJson)
                            ? Array.Empty<string>()
                            : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? Array.Empty<string>(),
                b.IsSold,
                b.CreatedAt,
                b.Views
            });

            SimpleLogger.LogNormal("BooksController", "ViewMyListings", $"Retrieved {books.Count} books", userId.ToString());
            return Ok(result);
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "ViewMyListings", "ViewMyListings failed", ex, userId.ToString());
            return StatusCode(500, "Failed to retrieve listings");
        }
    }


    [HttpPatch("MarkAsSold/{bookId}")]
    public async Task<IActionResult> MarkAsSold(Guid bookId)
    {
        SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Request for bookId: {bookId}", bookId.ToString());
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { Message = "Book not found" });
        SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"BookId not found: {bookId}", bookId.ToString());
        if (book.IsSold)
            return BadRequest(new { Message = "This book is already marked as sold." });

        book.IsSold = true;
        await _context.SaveChangesAsync();
        SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Book marked as sold successfully: {bookId}", bookId.ToString());
        return Ok(new { Message = "Book marked as sold successfully." });
    }
    [HttpPatch("MarkAsUnSold/{bookId}")]
    public async Task<IActionResult> MarkAsUnSold(Guid bookId)
    {
        SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Request for bookId: {bookId}", bookId.ToString());
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { Message = "Book not found" });
        SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"BookId not found: {bookId}", bookId.ToString());
        if (book.IsSold == false)
            return BadRequest(new { Message = "This book is already marked as Unsold." });

        book.IsSold = false;
        await _context.SaveChangesAsync();
        SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Book marked as Unsoldsold successfully: {bookId}", bookId.ToString());
        return Ok(new { Message = "Book marked as Unsold successfully." });
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

    [HttpPut("Boosting/{UserId}/{bookId}/{DistanceBoostingUpto}")]
    public async Task<IActionResult> Boost(Guid UserId, Guid bookId, int DistanceBoostingUpto)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            SimpleLogger.LogNormal("BooksController", "Boost", $"Book not found: {bookId}", bookId.ToString());
            return NotFound(new { Message = "Book not found" });
        }

        // Update book
        book.IsBoosted = true;
        book.DistanceBoostingUpto = DistanceBoostingUpto;
        book.ListingLastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

        await _context.SaveChangesAsync();

        SimpleLogger.LogNormal("BooksController", "Boost", $"Book Boosted successfully: {bookId}", bookId.ToString());
        return Ok(new { Message = "Book Boosted successfully." });
    }

    [HttpPost("ListBook")]
    public async Task<IActionResult> ListBook([FromForm] BookCreateDto dto)
    {
        try
        {
            var currentUserLocation = await _context.UserLocations
                .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

            if (currentUserLocation == null)
            {
                SimpleLogger.LogCritical("BooksController", "ListBook", $"User location not found for userId: {dto.UserId}", null, dto.UserId.ToString());
                return BadRequest("User location not found.");
            }
            SimpleLogger.LogNormal("BooksController", "ListBook", $"Book listing request for userId: {dto.UserId}, BookName: {dto.BookName}", dto.UserId.ToString());

            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                SimpleLogger.LogCritical("BooksController", "ListBook", $"User not found: {dto.UserId}", null, dto.UserId.ToString());
                return NotFound(new { Message = "User not found" });
            }

            if (dto.Images == null || dto.Images.Length < 1 || dto.Images.Length > 4)
            {
                SimpleLogger.LogCritical("BooksController", "ListBook", $"Invalid image count: {dto.Images?.Length ?? 0}", null, dto.UserId.ToString());
                return BadRequest(new { Message = "You must upload between 1 and 4 images." });
            }

            var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var imagePaths = new List<string>();

            foreach (var image in dto.Images)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var savePath = Path.Combine(uploadsFolder, fileName);

                using var imageStream = image.OpenReadStream();
                using var img = await Image.LoadAsync(imageStream);

                // Compress image to target ≤ 90 KB
                var quality = 75;
                var options = new JpegEncoder { Quality = quality };
                using var ms = new MemoryStream();
                await img.SaveAsJpegAsync(ms, options);
                while (ms.Length > 90 * 1024 && quality > 10)
                {
                    ms.SetLength(0);
                    quality -= 5;
                    options = new JpegEncoder { Quality = quality };
                    await img.SaveAsJpegAsync(ms, options);
                }

                // Save final image
                await System.IO.File.WriteAllBytesAsync(savePath, ms.ToArray());
                imagePaths.Add(Path.Combine("uploads/books", fileName));
            }

            var book = new Book
            {
                UserId = dto.UserId,
                BookName = dto.BookName,
                AuthorOrPublication = dto.AuthorOrPublication,
                Description = dto.Description,
                Category = dto.Category,
                SubCategory = dto.SubCategory,
                SellingPrice = dto.SellingPrice,
                ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(imagePaths),
                CreatedAt = IndianTimeHelper.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            SimpleLogger.LogNormal("BooksController", "ListBook", $"Book listed successfully with ID: {book.Id}", dto.UserId.ToString());
            return Ok(new { Message = "Book listed successfully", BookId = book.Id });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "ListBook", "ListBook method failed", ex, dto.UserId.ToString());
            return StatusCode(500, "Failed to list book");
        }
    }

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
        book.Description = string.IsNullOrWhiteSpace(dto.Description) ? book.Description : dto.Description;
        book.SubCategory = string.IsNullOrWhiteSpace(dto.SubCategory) ? book.SubCategory : dto.SubCategory;
        book.SellingPrice = dto.SellingPrice.HasValue ? dto.SellingPrice.Value : book.SellingPrice;

        // Deserialize existing images from DB
        var existingImagesInDb = string.IsNullOrEmpty(book.ImagePathsJson)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(book.ImagePathsJson)!;

        var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var updatedImages = new List<string>();

        // ✅ 1. Keep only the images user wants to retain
        if (dto.ExistingImages != null && dto.ExistingImages.Length > 0)
        {
            updatedImages.AddRange(dto.ExistingImages);
            // Delete only images that are NOT in ExistingImages
            var imagesToDelete = existingImagesInDb.Except(dto.ExistingImages).ToList();
            foreach (var imgPath in imagesToDelete)
            {
                var fullPath = Path.Combine(wwwRoot, imgPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting file {fullPath}: {ex.Message}");
                    }
                }
            }
        }
        else
        {
            // If ExistingImages not sent → keep all DB images as default
            updatedImages.AddRange(existingImagesInDb);
        }

        // ✅ 2. Add new images if uploaded
        if (dto.NewImages != null && dto.NewImages.Length > 0)
        {
            foreach (var image in dto.NewImages)
            {
                using var img = await Image.LoadAsync(image.OpenReadStream()); // SixLabors.ImageSharp

                // Compress image to target ≤ 90 KB
                var quality = 75;
                var options = new JpegEncoder { Quality = quality };
                using var ms = new MemoryStream();
                await img.SaveAsJpegAsync(ms, options);
                while (ms.Length > 90 * 1024 && quality > 10)
                {
                    ms.SetLength(0);
                    quality -= 5;
                    options = new JpegEncoder { Quality = quality };
                    await img.SaveAsJpegAsync(ms, options);
                }

                var fileName = Guid.NewGuid() + ".jpg";
                var savePath = Path.Combine(uploadsFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(savePath, ms.ToArray());

                updatedImages.Add(Path.Combine("uploads/books", fileName));
            }
        }

        // ✅ 3. Update database
        book.ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(updatedImages);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Book updated successfully" });
    }


    [Authorize]
    [HttpGet("ViewAll/{userId}")]
    public async Task<IActionResult> ViewAll(Guid userId,int page = 1, int pageSize = 50)
    {
        try
        {
            SimpleLogger.LogNormal("BooksController", "ViewAll", $"ViewAll request for userId: {userId}, search: {search}", userId.ToString());

            var currentUserLocation = await _context.UserLocations
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUserLocation == null)
            {
                SimpleLogger.LogCritical("BooksController", "ViewAll", $"User location not found for userId: {userId}", null, userId.ToString());
                return BadRequest("User location not found.");
            }

            // Base query
            var booksQuery = _context.Books
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

         

            var booksData = await booksQuery.ToListAsync();

            SimpleLogger.LogNormal("BooksController", "ViewAll", $"Retrieved {booksData.Count} books after search filter", userId.ToString());

            // Get unique userIds from books
            var userIds = booksData.Select(b => b.UserId).Distinct().ToList();

            // Create a comma-separated string of user IDs with single quotes
            var idList = string.Join(",", userIds.Select(id => $"'{id}'"));

            var sqlQuery = $@"
        SELECT t1.*
        FROM (
            SELECT *, ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY CreateDate DESC) as rn
            FROM UserLocations
            WHERE UserId IN ({idList})
        ) as t1
        WHERE t1.rn = 1;
    ";

            var userLocations = await _context.UserLocations
                .FromSqlRaw(sqlQuery)
                .ToDictionaryAsync(u => u.UserId, u => u);

            SimpleLogger.LogNormal("BooksController", "ViewAll", $"Retrieved locations for {userLocations.Count} users", userId.ToString());

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ResellBookApp");

            var books = new List<object>();

            foreach (var b in booksData)
            {
                double? distanceKm = null;
                string cityName = "N/A";
                string districtName = "N/A";

                if (userLocations.ContainsKey(b.UserId))
                {
                    var loc = userLocations[b.UserId];

                    distanceKm = CalculateDistance(
                        currentUserLocation.Latitude,
                        currentUserLocation.Longitude,
                        loc.Latitude,
                        loc.Longitude
                    );

                    // try
                    // {
                    //     string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={loc.Latitude}&lon={loc.Longitude}";
                    //     var response = await httpClient.GetAsync(url);

                    //     if (response.IsSuccessStatusCode)
                    //     {
                    //         var json = await response.Content.ReadAsStringAsync();
                    //         using (JsonDocument doc = JsonDocument.Parse(json))
                    //         {
                    //             if (doc.RootElement.TryGetProperty("address", out JsonElement address))
                    //             {
                    //                 if (address.TryGetProperty("city", out var city))
                    //                     cityName = city.GetString();
                    //                 else if (address.TryGetProperty("town", out var town))
                    //                     cityName = town.GetString();
                    //                 else if (address.TryGetProperty("village", out var village))
                    //                     cityName = village.GetString();

                    //                 if (address.TryGetProperty("state_district", out var district))
                    //                     districtName = district.GetString();
                    //             }
                    //         }
                    //     }
                    // }
                    // catch { }
                }

                books.Add(new
                {
                    b.Id,
                    b.UserId,
                    UserName = b.User.Name,
                    b.BookName,
                    b.AuthorOrPublication,
                    b.Description,
                    b.Category,
                    b.SubCategory,
                    b.SellingPrice,
                    b.IsSold,
                    Images = string.IsNullOrEmpty(b.ImagePathsJson)
                        ? Array.Empty<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? Array.Empty<string>(),
                    b.CreatedAt,
                    City = cityName,
                    District = districtName,
                    DistanceValue = distanceKm,
                    Distance = distanceKm.HasValue
                        ? (distanceKm < 1
                            ? $"{Math.Round(distanceKm.Value * 1000)} m"
                            : $"{Math.Round(distanceKm.Value, 2)} km")
                        : "N/A"
                });
            }

            // Sort by distance
            var sortedBooks = books
                .OrderBy(b => ((dynamic)b).DistanceValue ?? double.MaxValue)
                .ToList();

            // Pagination
            var pagedBooks = sortedBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = sortedBooks.Count,
                TotalPages = (int)Math.Ceiling(sortedBooks.Count / (double)pageSize),
                Books = pagedBooks
            });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "ViewAll", "ViewAll method failed", ex, userId.ToString());
            return StatusCode(500, "Failed to retrieve books");
        }
    }


    [HttpGet("GetCityName")]
    public async Task<IActionResult> GetCityName(double latitude, double longitude)
    {
        // REMOVED: OpenStreetMap API call for performance - was causing slow responses
        // Return placeholder values to maintain API compatibility
        return Ok(new
        {
            City = "N/A",
            District = "N/A"
        });
    }








    private string[] DeserializeImages(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return Array.Empty<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch (Exception ex)
        {
            return Array.Empty<string>();
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
    [Required] public required string Description { get; set; }
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
    public string? Description { get; set; }
    public IFormFile[]? NewImages { get; set; }
    public string[]? ExistingImages { get; set; }
}
