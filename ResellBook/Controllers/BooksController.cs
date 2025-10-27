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
                .Where(b => b.UserId == userId /*&& !b.IsSold*/)
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
                    b.Views
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

    // [HttpPut("Boosting/{UserId}/{bookId}/{DistanceBoostingUpto}")]
    // public async Task<IActionResult> Boost(Guid UserId, Guid bookId, int DistanceBoostingUpto)
    // {
    //     try
    //     {
    //         SimpleLogger.LogNormal("BooksController", "Boost", $"Boost request for bookId: {bookId}, userId: {UserId}, distance: {DistanceBoostingUpto}", UserId.ToString());

    //         // Validate distance range (reasonable bounds)
    //         if (DistanceBoostingUpto < 1 || DistanceBoostingUpto > 500)
    //         {
    //             return BadRequest(new { Message = "Boosting distance must be between 1 and 500 km." });
    //         }

    //         var book = await _context.Books.FindAsync(bookId);
    //         if (book == null)
    //         {
    //             SimpleLogger.LogNormal("BooksController", "Boost", $"Book not found: {bookId}", UserId.ToString());
    //             return NotFound(new { Message = "Book not found" });
    //         }

    //         // Check if user owns the book
    //         if (book.UserId != UserId)
    //         {
    //             return Unauthorized(new { Message = "You can only boost your own books." });
    //         }

    //         // Check if book is already sold
    //         if (book.IsSold)
    //         {
    //             return BadRequest(new { Message = "Cannot boost a sold book." });
    //         }

    //         // Update book boosting details
    //         // book.IsBoosted = true;
    //         // book.DistanceBoostingUpto = DistanceBoostingUpto;
    //         // book.ListingLastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

    //         await _context.SaveChangesAsync();

    //         SimpleLogger.LogNormal("BooksController", "Boost", $"Book boosted successfully: {bookId}", UserId.ToString());
    //         return Ok(new { Message = "Book boosted successfully." });
    //     }
    //     catch (Exception ex)
    //     {
    //         SimpleLogger.LogCritical("BooksController", "Boost", "Boost failed", ex, UserId.ToString());
    //         return StatusCode(500, "Failed to boost book");
    //     }
    // }

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
    public async Task<IActionResult> ViewAll(Guid userId, int distance = 50, int page = 1, int pageSize = 50)
    {
        try
        {
            var currentUserLocation = await _context.UserLocations
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUserLocation == null)
            {
                SimpleLogger.LogCritical("BooksController", "ViewAll", $"User location not found for userId: {userId}", null, userId.ToString());
                return BadRequest("User location not found.");
            }

            // Check short-term cache for pagination (30 seconds)
            var cacheKey = $"ViewAll_{userId}_{distance}";
            List<object>? books = null;
            if (_cache.TryGetValue(cacheKey, out List<object>? cachedBooks))
            {
                SimpleLogger.LogNormal("BooksController", "ViewAll", $"Short cache hit for userId: {userId}", userId.ToString());
                books = cachedBooks;
            }
            else
            {
                // Use fixed radius from parameter
                var currentRadius = (double)distance;
                List<dynamic> nearbyBooks = new List<dynamic>();

                const double kmPerDegreeLat = 111.0;
                var kmPerDegreeLon = 111.0 * Math.Cos(ToRadians(currentUserLocation.Latitude));

                var latOffset = currentRadius / kmPerDegreeLat;
                var lonOffset = currentRadius / kmPerDegreeLon;

                var minLat = currentUserLocation.Latitude - latOffset;
                var maxLat = currentUserLocation.Latitude + latOffset;
                var minLon = currentUserLocation.Longitude - lonOffset;
                var maxLon = currentUserLocation.Longitude + lonOffset;

                var query = from b in _context.Books
                            join ul in _context.UserLocations on b.UserId equals ul.UserId
                            where !b.IsSold &&
                                  ul.Latitude >= minLat && ul.Latitude <= maxLat &&
                                  ul.Longitude >= minLon && ul.Longitude <= maxLon
                            select new
                            {
                                Book = b,
                                UserLocation = ul,
                                UserName = b.User.Name,
                                UserPhone = b.User.Phone
                            };

                nearbyBooks = (await query.Take(1000).ToListAsync()).Cast<dynamic>().ToList();

                SimpleLogger.LogNormal("BooksController", "ViewAll", $"Found {nearbyBooks.Count} books within {currentRadius}km", userId.ToString());

                // Remove duplicates by Book.Id
                nearbyBooks = nearbyBooks
                    .GroupBy(b => b.Book.Id)
                    .Select(g => g.First())
                    .ToList();

                // Guard against empty results to skip unnecessary processing
                if (nearbyBooks.Count == 0)
                {
                    books = new List<object>();
                }
                else
                {
                    // Calculate approximate distances (fast), sort, take top candidates
                    var approxBooks = nearbyBooks
                        .Select(item =>
                        {
                            var approxDistance = CalculateApproxDistance(
                                currentUserLocation.Latitude, currentUserLocation.Longitude,
                                item.UserLocation.Latitude, item.UserLocation.Longitude);
                            return new
                            {
                                Item = item,
                                ApproxDistance = approxDistance
                            };
                        })
                        .OrderBy(b => b.ApproxDistance)
                        .Take(100) // Reduced to 100 for fewer exact calculations
                        .ToList();

                    // Calculate exact distances only for top candidates
                    var booksWithDistances = approxBooks
                        .Select(ab =>
                        {
                            var distance = CalculateDistance(
                                currentUserLocation.Latitude, currentUserLocation.Longitude,
                                ab.Item.UserLocation.Latitude, ab.Item.UserLocation.Longitude);
                            return new
                            {
                                ab.Item.Book,
                                Distance = distance,
                                ab.Item.UserName,
                                ab.Item.UserPhone
                            };
                        })
                        .Where(b => b.Distance <= currentRadius) // Ensure within final radius
                        .OrderBy(b => b.Distance)
                        .Take(500) // Limit to 500
                        .ToList();

                    // Convert to final format
                    books = booksWithDistances.Select(b => (object)new
                    {
                        b.Book.Id,
                        b.Book.UserId,
                        UserName = b.UserName,
                        Phone = b.UserPhone,
                        b.Book.BookName,
                        b.Book.AuthorOrPublication,
                        b.Book.Description,
                        b.Book.Category,
                        b.Book.SubCategory,
                        b.Book.SellingPrice,
                        b.Book.IsSold,
                        Images = string.IsNullOrEmpty(b.Book.ImagePathsJson)
                            ? Array.Empty<string>()
                            : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.Book.ImagePathsJson) ?? Array.Empty<string>(),
                        b.Book.CreatedAt,
                        City = "N/A",
                        District = "N/A",
                        DistanceValue = b.Distance,
                        Distance = b.Distance < 1
                            ? $"{Math.Round(b.Distance * 1000)} m"
                            : $"{Math.Round(b.Distance, 2)} km"
                    }).ToList();
                }

                // Short-term cache for 60 seconds (for pagination)
                _cache.Set(cacheKey, books, TimeSpan.FromSeconds(60));
            }

            // Apply pagination
            var totalBooks = books.Count;
            var paginatedBooks = books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalBooks,
                TotalPages = (int)Math.Ceiling(totalBooks / (double)pageSize),
                Books = paginatedBooks
            });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "ViewAll", "ViewAll method failed", ex, userId.ToString());
            return StatusCode(500, "Failed to retrieve books");
        }
    }


    [HttpGet("GetCategories")]
    public IActionResult GetCategories()
    {
        var categories = new List<CategoryDto>
            {
                new CategoryDto
                {
                    Id = 1,
                    Category = "Primary School Books",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "Class I"),
                        new(2, "Class II"),
                        new(3, "Class III"),
                        new(4, "Class IV"),
                        new(5, "Class V"),
                        new(6, "Activity Books"),
                        new(7, "Story Books"),
                        new(8, "Learning Kits")
                    }
                },
                new CategoryDto
                {
                    Id = 2,
                    Category = "Secondary School Books",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "Class VI"),
                        new(2, "Class VII"),
                        new(3, "Class VIII"),
                        new(4, "Class IX"),
                        new(5, "Class X"),
                        new(6, "Science"),
                        new(7, "Math"),
                        new(8, "Social Studies"),
                        new(9, "Language"),
                        new(10, "Exam Prep")
                    }
                },
                new CategoryDto
                {
                    Id = 3,
                    Category = "Senior Secondary Books",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "Class XI"),
                        new(2, "Class XII"),
                        new(3, "Science"),
                        new(4, "Commerce"),
                        new(5, "Arts"),
                        new(6, "Board Exam Guides"),
                        new(7, "Reference Books")
                    }
                },
                new CategoryDto
                {
                    Id = 4,
                    Category = "Engineering & Technology",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "B.Tech"),
                        new(2, "BCA"),
                        new(3, "MCA"),
                        new(4, "M.Tech"),
                        new(5, "Diploma"),
                        new(6, "IT & Computer Science"),
                        new(7, "Electronics"),
                        new(8, "Mechanical"),
                        new(9, "Civil"),
                        new(10, "Electrical")
                    }
                },
                new CategoryDto
                {
                    Id = 5,
                    Category = "Business & Management",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "MBA"),
                        new(2, "BBA"),
                        new(3, "Management"),
                        new(4, "Entrepreneurship"),
                        new(5, "Marketing"),
                        new(6, "Finance"),
                        new(7, "HR"),
                        new(8, "Accounting")
                    }
                },
                new CategoryDto
                {
                    Id = 6,
                    Category = "Medical & Health Sciences",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "MBBS"),
                        new(2, "BDS"),
                        new(3, "Nursing"),
                        new(4, "B.Sc Nursing"),
                        new(5, "Paramedical"),
                        new(6, "Pharmacy"),
                        new(7, "Physiotherapy"),
                        new(8, "Allied Health")
                    }
                },
                new CategoryDto
                {
                    Id = 7,
                    Category = "Competitive Exams",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "SSC"),
                        new(2, "UPSC"),
                        new(3, "Banking (IBPS, SBI)"),
                        new(4, "Railway"),
                        new(5, "Defence (NDA, CDS)"),
                        new(6, "Teaching Exams (TET, CTET)"),
                        new(7, "Private Sector Exams")
                    }
                },
                new CategoryDto
                {
                    Id = 8,
                    Category = "Law & Judiciary",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "LLB"),
                        new(2, "LLM"),
                        new(3, "Judiciary Exam Prep"),
                        new(4, "Legal Studies")
                    }
                },
                new CategoryDto
                {
                    Id = 9,
                    Category = "Arts & Humanities",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "History"),
                        new(2, "Geography"),
                        new(3, "Political Science"),
                        new(4, "Sociology"),
                        new(5, "Psychology"),
                        new(6, "Philosophy"),
                        new(7, "Literature")
                    }
                },
                new CategoryDto
                {
                    Id = 10,
                    Category = "Science & Mathematics",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "Physics"),
                        new(2, "Chemistry"),
                        new(3, "Biology"),
                        new(4, "Mathematics"),
                        new(5, "Statistics"),
                        new(6, "Astronomy"),
                        new(7, "Computer Science")
                    }
                },
                new CategoryDto
                {
                    Id = 11,
                    Category = "Languages",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "English"),
                        new(2, "Hindi"),
                        new(3, "Regional Languages"),
                        new(4, "Foreign Languages (French, German, Spanish, Japanese, etc.)")
                    }
                },
                new CategoryDto
                {
                    Id = 12,
                    Category = "Vocational & Skill Development",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "IT & Software"),
                        new(2, "Hospitality"),
                        new(3, "Culinary"),
                        new(4, "Fashion & Design"),
                        new(5, "Media & Communication"),
                        new(6, "Digital Marketing"),
                        new(7, "Coding & Programming")
                    }
                },
                new CategoryDto
                {
                    Id = 13,
                    Category = "Test Prep & Certifications",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "GRE"),
                        new(2, "GMAT"),
                        new(3, "SAT"),
                        new(4, "IELTS"),
                        new(5, "TOEFL"),
                        new(6, "CA"),
                        new(7, "CS"),
                        new(8, "CMA"),
                        new(9, "Professional Certifications")
                    }
                },
                new CategoryDto
                {
                    Id = 14,
                    Category = "Hobbies & Extracurricular",
                    Subcategories = new List<SubCategoryDto>
                    {
                        new(1, "Arts & Crafts"),
                        new(2, "Music"),
                        new(3, "Sports"),
                        new(4, "Coding for Kids"),
                        new(5, "Puzzle Books"),
                        new(6, "Story Books"),
                        new(7, "Comics")
                    }
                }
            };

        return Ok(categories);
    }


    // DTOs
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public List<SubCategoryDto> Subcategories { get; set; }
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

    // Optimized Haversine formula (pre-calculate constants for speed)
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0; // Earth's radius in km
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        double lat1Rad = ToRadians(lat1);
        double lat2Rad = ToRadians(lat2);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    // Fast approximate distance (Euclidean, scaled to km) for initial sorting
    private double CalculateApproxDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double kmPerDegreeLat = 111.0;
        const double kmPerDegreeLon = 111.0; // Average, adjust for latitude if needed
        var dLat = (lat2 - lat1) * kmPerDegreeLat;
        var dLon = (lon2 - lon1) * kmPerDegreeLon * Math.Cos(ToRadians((lat1 + lat2) / 2));
        return Math.Sqrt(dLat * dLat + dLon * dLon);
    }

    private double ToRadians(double angle) => Math.PI * angle / 180.0;
}

// DTOs
public class SubCategoryDto
{
    public SubCategoryDto(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}
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
