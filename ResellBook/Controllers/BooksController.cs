using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    public BooksController(AppDbContext context, IWebHostEnvironment env, IMemoryCache cache)
    {
        _context = context;
        _env = env;
        _cache = cache;
    }
    [Authorize]
    [Authorize]
    [HttpPost("UserClick/{bookId}")]
    public async Task<IActionResult> UserClick(Guid bookId)
    {
        try
        {
            // Get logged-in user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var loggedInUserId))
                return Unauthorized(new { Message = "Invalid or missing user token." });

            SimpleLogger.LogNormal("BooksController", "UserClick", $"Click request for bookId: {bookId}", loggedInUserId.ToString());

            // Single query to get book and check ownership
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
                return NotFound(new { Message = "Book not found" });

            // Check if the user is the owner of the book
            if (book.UserId == loggedInUserId)
            {
                return Ok(new
                {
                    Message = "Owner's view not counted",
                    BookId = bookId,
                    TotalViews = book.Views
                });
            }

            // Increment the view count using raw SQL for atomic operation
            var updateResult = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Books SET Views = Views + 1 WHERE Id = {bookId} AND UserId != {loggedInUserId}"
            );

            if (updateResult == 0)
            {
                return Ok(new
                {
                    Message = "View not counted (owner or book not found)",
                    BookId = bookId,
                    TotalViews = book.Views
                });
            }

            // Get updated view count
            var totalViews = await _context.Books
                .Where(b => b.Id == bookId)
                .Select(b => b.Views)
                .FirstOrDefaultAsync();

            SimpleLogger.LogNormal("BooksController", "UserClick", $"View updated successfully for bookId: {bookId}", loggedInUserId.ToString());

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
                .Where(b => b.UserId == userId && !b.IsSold)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.BookName,
                    b.AuthorOrPublication,
                    b.Category,
                    b.Description,
                    b.SubCategory,
                    b.SellingPrice,
                    ImagePathsJson = b.ImagePathsJson,
                    b.IsSold,
                    b.CreatedAt,
                    b.Views,
                    b.IsBoosted
                })
                .ToListAsync();

            // Process images after materialization (not in expression tree)
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
                            ? new string[0]
                            : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? new string[0],
                b.IsSold,
                b.CreatedAt,
                b.Views,
                b.IsBoosted
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
        try
        {
            SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Request for bookId: {bookId}", bookId.ToString());

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Book not found: {bookId}", bookId.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            if (book.IsSold)
            {
                return BadRequest(new { Message = "This book is already marked as sold." });
            }

            book.IsSold = true;
            await _context.SaveChangesAsync();

            SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Book marked as sold successfully: {bookId}", bookId.ToString());
            return Ok(new { Message = "Book marked as sold successfully." });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "MarkAsSold", "MarkAsSold failed", ex, bookId.ToString());
            return StatusCode(500, "Failed to mark book as sold");
        }
    }

    [HttpPatch("MarkAsUnSold/{bookId}")]
    public async Task<IActionResult> MarkAsUnSold(Guid bookId)
    {
        try
        {
            SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Request for bookId: {bookId}", bookId.ToString());

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Book not found: {bookId}", bookId.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            if (!book.IsSold)
            {
                return BadRequest(new { Message = "This book is already marked as unsold." });
            }

            book.IsSold = false;
            await _context.SaveChangesAsync();

            SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Book marked as unsold successfully: {bookId}", bookId.ToString());
            return Ok(new { Message = "Book marked as unsold successfully." });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "MarkAsUnSold", "MarkAsUnSold failed", ex, bookId.ToString());
            return StatusCode(500, "Failed to mark book as unsold");
        }
    }
    // DELETE: api/Books/Delete/{bookId}
    [HttpDelete("Delete/{bookId}")]
    public async Task<IActionResult> Delete(Guid bookId)
    {
        try
        {
            SimpleLogger.LogNormal("BooksController", "Delete", $"Delete request for bookId: {bookId}", bookId.ToString());

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                SimpleLogger.LogNormal("BooksController", "Delete", $"Book not found: {bookId}", bookId.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            // Remove images from server if exist
            if (!string.IsNullOrEmpty(book.ImagePathsJson))
            {
                try
                {
                    var images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(book.ImagePathsJson);
                    if (images != null && images.Count > 0)
                    {
                        var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var deletedCount = 0;

                        foreach (var img in images)
                        {
                            var filePath = Path.Combine(wwwRoot, img.Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (System.IO.File.Exists(filePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(filePath);
                                    deletedCount++;
                                }
                                catch (Exception ex)
                                {
                                    SimpleLogger.LogCritical("BooksController", "Delete", $"Failed to delete image: {filePath}", ex, bookId.ToString());
                                }
                            }
                        }

                        SimpleLogger.LogNormal("BooksController", "Delete", $"Deleted {deletedCount} images for bookId: {bookId}", bookId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    SimpleLogger.LogCritical("BooksController", "Delete", $"Failed to deserialize image paths for bookId: {bookId}", ex, bookId.ToString());
                    // Continue with deletion even if image cleanup fails
                }
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            SimpleLogger.LogNormal("BooksController", "Delete", $"Book deleted successfully: {bookId}", bookId.ToString());
            return Ok(new { Message = "Book deleted successfully" });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "Delete", "Delete failed", ex, bookId.ToString());
            return StatusCode(500, "Failed to delete book");
        }
    }
    // POST: List Book

    [HttpPut("Boosting/{UserId}/{bookId}/{DistanceBoostingUpto}")]
    public async Task<IActionResult> Boost(Guid UserId, Guid bookId, int DistanceBoostingUpto)
    {
        try
        {
            SimpleLogger.LogNormal("BooksController", "Boost", $"Boost request for bookId: {bookId}, userId: {UserId}, distance: {DistanceBoostingUpto}", UserId.ToString());

            // Validate distance range (reasonable bounds)
            if (DistanceBoostingUpto < 1 || DistanceBoostingUpto > 500)
            {
                return BadRequest(new { Message = "Boosting distance must be between 1 and 500 km." });
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                SimpleLogger.LogNormal("BooksController", "Boost", $"Book not found: {bookId}", UserId.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            // Check if user owns the book
            if (book.UserId != UserId)
            {
                return Unauthorized(new { Message = "You can only boost your own books." });
            }

            // Check if book is already sold
            if (book.IsSold)
            {
                return BadRequest(new { Message = "Cannot boost a sold book." });
            }

            // Update book boosting details
            book.IsBoosted = true;
            book.DistanceBoostingUpto = DistanceBoostingUpto;
            book.ListingLastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

            await _context.SaveChangesAsync();

            SimpleLogger.LogNormal("BooksController", "Boost", $"Book boosted successfully: {bookId}", UserId.ToString());
            return Ok(new { Message = "Book boosted successfully." });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "Boost", "Boost failed", ex, UserId.ToString());
            return StatusCode(500, "Failed to boost book");
        }
    }

    [HttpPost("ListBook")]
    public async Task<IActionResult> ListBook([FromForm] BookCreateDto dto)
    {
        try
        {
            // Validate user location first
            var currentUserLocation = await _context.UserLocations
                .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

            if (currentUserLocation == null)
            {
                SimpleLogger.LogCritical("BooksController", "ListBook", $"User location not found for userId: {dto.UserId}", null, dto.UserId.ToString());
                return BadRequest("User location not found. Please update your location first.");
            }

            SimpleLogger.LogNormal("BooksController", "ListBook", $"Book listing request for userId: {dto.UserId}, BookName: {dto.BookName}", dto.UserId.ToString());

            // Validate user exists
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                SimpleLogger.LogCritical("BooksController", "ListBook", $"User not found: {dto.UserId}", null, dto.UserId.ToString());
                return NotFound(new { Message = "User not found" });
            }

            // Validate image count
            if (dto.Images == null || dto.Images.Length < 1 || dto.Images.Length > 4)
            {
                SimpleLogger.LogCritical("BooksController", "ListBook", $"Invalid image count: {dto.Images?.Length ?? 0}", null, dto.UserId.ToString());
                return BadRequest(new { Message = "You must upload between 1 and 4 images." });
            }

            // Validate selling price
            if (dto.SellingPrice <= 0 || dto.SellingPrice > 100000)
            {
                return BadRequest(new { Message = "Selling price must be between 1 and 100,000." });
            }

            var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
            Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

            var imagePaths = new List<string>();

            // Process images with better error handling
            foreach (var image in dto.Images)
            {
                if (image.Length == 0)
                {
                    return BadRequest(new { Message = "One or more images are empty." });
                }

                if (image.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return BadRequest(new { Message = "Image size must be less than 5MB." });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { Message = "Only JPG, PNG, and WebP images are allowed." });
                }

                var fileName = Guid.NewGuid() + ".jpg"; // Always save as JPG
                var savePath = Path.Combine(uploadsFolder, fileName);

                try
                {
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
                catch (Exception ex)
                {
                    SimpleLogger.LogCritical("BooksController", "ListBook", $"Image processing failed for {image.FileName}", ex, dto.UserId.ToString());
                    return StatusCode(500, "Failed to process images");
                }
            }

            var book = new Book
            {
                UserId = dto.UserId,
                BookName = dto.BookName.Trim(),
                AuthorOrPublication = string.IsNullOrWhiteSpace(dto.AuthorOrPublication) ? null : dto.AuthorOrPublication.Trim(),
                Description = dto.Description.Trim(),
                Category = dto.Category.Trim(),
                SubCategory = string.IsNullOrWhiteSpace(dto.SubCategory) ? null : dto.SubCategory.Trim(),
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
        try
        {
            SimpleLogger.LogNormal("BooksController", "EditListing", $"Edit request for bookId: {id}", id.ToString());

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                SimpleLogger.LogNormal("BooksController", "EditListing", $"Book not found: {id}", id.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            // Validate selling price if provided
            if (dto.SellingPrice.HasValue && (dto.SellingPrice.Value <= 0 || dto.SellingPrice.Value > 100000))
            {
                return BadRequest(new { Message = "Selling price must be between 1 and 100,000." });
            }

            // Update basic fields (only if provided and not empty)
            if (!string.IsNullOrWhiteSpace(dto.BookName))
                book.BookName = dto.BookName.Trim();
            if (!string.IsNullOrWhiteSpace(dto.AuthorOrPublication))
                book.AuthorOrPublication = dto.AuthorOrPublication.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Category))
                book.Category = dto.Category.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Description))
                book.Description = dto.Description.Trim();
            if (!string.IsNullOrWhiteSpace(dto.SubCategory))
                book.SubCategory = dto.SubCategory.Trim();
            if (dto.SellingPrice.HasValue)
                book.SellingPrice = dto.SellingPrice.Value;

            var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRoot, "uploads/books");
            Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

            // Deserialize existing images from DB
            var existingImagesInDb = string.IsNullOrEmpty(book.ImagePathsJson)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(book.ImagePathsJson) ?? new List<string>();

            var updatedImages = new List<string>();

            // ✅ 1. Keep only the images user wants to retain
            if (dto.ExistingImages != null && dto.ExistingImages.Length > 0)
            {
                // Validate that existing images are actually in the DB
                var validExistingImages = dto.ExistingImages
                    .Where(img => existingImagesInDb.Contains(img))
                    .ToList();

                updatedImages.AddRange(validExistingImages);

                // Delete only images that are NOT in ExistingImages
                var imagesToDelete = existingImagesInDb.Except(validExistingImages).ToList();
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
                            SimpleLogger.LogCritical("BooksController", "EditListing", $"Failed to delete image: {fullPath}", ex, id.ToString());
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
                // Validate total image count
                var totalImages = updatedImages.Count + dto.NewImages.Length;
                if (totalImages < 1 || totalImages > 4)
                {
                    return BadRequest(new { Message = "Total images must be between 1 and 4." });
                }

                foreach (var image in dto.NewImages)
                {
                    if (image.Length == 0)
                    {
                        return BadRequest(new { Message = "One or more new images are empty." });
                    }

                    if (image.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        return BadRequest(new { Message = "New image size must be less than 5MB." });
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var extension = Path.GetExtension(image.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { Message = "Only JPG, PNG, and WebP images are allowed." });
                    }

                    try
                    {
                        using var img = await Image.LoadAsync(image.OpenReadStream());

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
                    catch (Exception ex)
                    {
                        SimpleLogger.LogCritical("BooksController", "EditListing", $"Image processing failed for {image.FileName}", ex, id.ToString());
                        return StatusCode(500, "Failed to process new images");
                    }
                }
            }

            // Validate final image count
            if (updatedImages.Count < 1 || updatedImages.Count > 4)
            {
                return BadRequest(new { Message = "Final image count must be between 1 and 4." });
            }

            // ✅ 3. Update database
            book.ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(updatedImages);

            await _context.SaveChangesAsync();

            SimpleLogger.LogNormal("BooksController", "EditListing", $"Book updated successfully: {id}", id.ToString());
            return Ok(new { Message = "Book updated successfully" });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "EditListing", "EditListing failed", ex, id.ToString());
            return StatusCode(500, "Failed to update book");
        }
    }

    [Authorize]
    [HttpGet("ViewAll/{userId}")]
    public async Task<IActionResult> ViewAll(Guid userId, int page = 1, int pageSize = 50)
    {
        try
        {
            SimpleLogger.LogNormal("BooksController", "ViewAll", $"ViewAll request for userId: {userId}, page: {page}, pageSize: {pageSize}", userId.ToString());

            var currentUserLocation = await _context.UserLocations
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUserLocation == null)
            {
                SimpleLogger.LogCritical("BooksController", "ViewAll", $"User location not found for userId: {userId}", null, userId.ToString());
                return BadRequest("User location not found.");
            }

            // Calculate how many books we need: enough for current page + buffer for accurate sorting
            var booksNeeded = (page * pageSize) + (pageSize * 3); // Extra buffer for better distance sorting
            var batchSize = 500; // Small batches to minimize memory usage

            // Get all user locations once (needed for distance calculations) - with caching
            var allUserLocations = await _cache.GetOrCreateAsync("AllUserLocations", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // Cache for 10 minutes
                return await _context.UserLocations
                    .FromSqlRaw(@"
                        SELECT t1.*
                        FROM (
                            SELECT *, ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY CreateDate DESC) as rn
                            FROM UserLocations
                        ) as t1
                        WHERE t1.rn = 1")
                    .ToDictionaryAsync(u => u.UserId, u => u);
            });

            SimpleLogger.LogNormal("BooksController", "ViewAll", $"Loaded {allUserLocations.Count} user locations", userId.ToString());

            // Efficient approach: Load books in batches and keep track of nearest ones
            var nearestBooks = new List<(Book book, double? distance)>();
            var skipCount = 0;
            var totalProcessed = 0;

            // Keep loading batches until we have enough books or run out
            while (nearestBooks.Count < booksNeeded && totalProcessed < 10000) // Safety limit
            {
                var batch = await _context.Books
                    .Include(b => b.User)
                    .Where(b => !b.IsSold)
                    .OrderByDescending(b => b.CreatedAt) // Most recent first
                    .Skip(skipCount)
                    .Take(batchSize)
                    .ToListAsync();

                if (batch.Count == 0) break; // No more books

                // Process this batch and calculate distances
                foreach (var book in batch)
                {
                    double? distance = null;
                    if (allUserLocations.ContainsKey(book.UserId))
                    {
                        var loc = allUserLocations[book.UserId];
                        distance = CalculateDistance(
                            currentUserLocation.Latitude,
                            currentUserLocation.Longitude,
                            loc.Latitude,
                            loc.Longitude);
                    }

                    nearestBooks.Add((book, distance));
                    totalProcessed++;
                }

                skipCount += batch.Count;

                // Sort periodically to keep only the nearest books
                if (nearestBooks.Count >= booksNeeded * 2)
                {
                    nearestBooks = nearestBooks
                        .OrderBy(b => b.distance ?? double.MaxValue)
                        .Take(booksNeeded)
                        .ToList();
                }
            }

            SimpleLogger.LogNormal("BooksController", "ViewAll", $"Processed {totalProcessed} books, kept {nearestBooks.Count} nearest", userId.ToString());

            // Final sort by distance
            var sortedBooks = nearestBooks
                .OrderBy(b => b.distance ?? double.MaxValue)
                .ToList();

            // Apply pagination
            var paginatedBooks = sortedBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Convert to final format
            var books = new List<object>();
            foreach (var (book, distance) in paginatedBooks)
            {
                books.Add(new
                {
                    book.Id,
                    book.UserId,
                    UserName = book.User.Name,
                    book.User.Phone,
                    book.BookName,
                    book.AuthorOrPublication,
                    book.Description,
                    book.Category,
                    book.SubCategory,
                    book.SellingPrice,
                    book.IsSold,
                    book.IsBoosted,
                    Images = string.IsNullOrEmpty(book.ImagePathsJson)
                        ? Array.Empty<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<string[]>(book.ImagePathsJson) ?? Array.Empty<string>(),
                    book.CreatedAt,
                    City = "N/A",
                    District = "N/A",
                    DistanceValue = distance,
                    Distance = distance.HasValue
                        ? (distance < 1
                            ? $"{Math.Round(distance.Value * 1000)} m"
                            : $"{Math.Round(distance.Value, 2)} km")
                        : "N/A"
                });
            }

            // Get total count for pagination metadata
            var totalBooks = await _context.Books.CountAsync(b => !b.IsSold);

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalBooks,
                TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize),
                Books = books
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
