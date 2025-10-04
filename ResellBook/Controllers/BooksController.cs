using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;
using System.ComponentModel.DataAnnotations;

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
    [HttpGet("ViewMyListings/{userId}")]
    public async Task<IActionResult> ViewMyListings(Guid userId)
    {
        
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
            b.SubCategory,
            b.SellingPrice,
            Images = string.IsNullOrEmpty(b.ImagePathsJson)
                        ? Array.Empty<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? Array.Empty<string>(),
            b.IsSold,
            b.CreatedAt
        });

        return Ok(result);
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
        if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            return NotFound(new { Message = "User not found" });

        if (dto.Images == null || dto.Images.Length < 2 || dto.Images.Length > 4)
            return BadRequest(new { Message = "You must upload between 2 and 4 images." });

        var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var imagePaths = new List<string>();
        foreach (var image in dto.Images)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var savePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(savePath, FileMode.Create);
            await image.CopyToAsync(stream);
            imagePaths.Add(Path.Combine("uploads/books", fileName));
        }

        var book = new Book
        {
            UserId = dto.UserId,
            BookName = dto.BookName,
            AuthorOrPublication = dto.AuthorOrPublication,
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            SellingPrice = dto.SellingPrice,
            ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(imagePaths),
            CreatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Book listed successfully", BookId = book.Id });
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
    // Get current user location
    var currentUserLocation = await _context.UserLocations
        .FirstOrDefaultAsync(u => u.UserId == userId);

    if (currentUserLocation == null)
        return BadRequest("User location not found.");

    // Get all books with user information
    var booksData = await _context.Books
        .Include(b => b.User)  // Include User data to get UserName
        .OrderByDescending(b => b.CreatedAt)
        .ToListAsync();

    // Get unique userIds from books
    var userIds = booksData.Select(b => b.UserId).Distinct().ToList();

    // Fetch all user locations for these userIds
    var userLocations = await _context.UserLocations
        .Where(u => userIds.Contains(u.UserId))
        .ToDictionaryAsync(u => u.UserId, u => u);

    var books = booksData.Select(b =>
    {
        double? distanceKm = null;
        if (userLocations.ContainsKey(b.UserId))
        {
            var loc = userLocations[b.UserId];
            distanceKm = CalculateDistance(
                currentUserLocation.Latitude,
                currentUserLocation.Longitude,
                loc.Latitude,
                loc.Longitude
            );
        }

        return new
        {
            b.Id,
            b.UserId,        // Include the userId of who listed the book
            UserName = b.User.Name,  // ✨ ADDED: Include the name of who listed the book
            b.BookName,
            b.AuthorOrPublication,
            b.Category,
            b.SubCategory,
            b.SellingPrice,
            b.IsSold,
            Images = string.IsNullOrEmpty(b.ImagePathsJson)
                        ? Array.Empty<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? Array.Empty<string>(),
            b.CreatedAt,
            Distance = distanceKm.HasValue
                ? (distanceKm < 1
                    ? $"{Math.Round(distanceKm.Value * 1000)} m"
                    : $"{Math.Round(distanceKm.Value, 2)} km")
                : "N/A"
        };
    }).ToList();

    return Ok(books);
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
