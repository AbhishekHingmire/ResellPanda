using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;

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

        // Validate final image count
        if (currentImages.Count < 2)
            return BadRequest(new { Message = "Each book must have at least 2 images." });
        if (currentImages.Count > 4)
            return BadRequest(new { Message = "Maximum 4 images allowed." });

        book.ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(currentImages);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Book updated successfully" });
    }

    [HttpGet("ViewAll")]
    public async Task<IActionResult> ViewAll()
    {
        var booksData = await _context.Books
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var books = booksData.Select(b => new
        {
            b.Id,
            b.BookName,
            b.AuthorOrPublication,
            b.Category,
            b.SubCategory,
            b.SellingPrice,
            Images = DeserializeImages(b.ImagePathsJson),
            b.CreatedAt
        }).ToList();

        return Ok(books);
    }
    private string[] DeserializeImages(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return Array.Empty<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }



}

// DTOs

public class BookCreateDto
{
    [Required] public Guid UserId { get; set; }
    [Required] public string BookName { get; set; }
    public string? AuthorOrPublication { get; set; }
    [Required] public string Category { get; set; }
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
