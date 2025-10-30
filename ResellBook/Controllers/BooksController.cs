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

            //SimpleLogger.LogNormal("BooksController", "UserClick", $"Click request for bookId: {bookId}", loggedInUserId.ToString());

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

            //SimpleLogger.LogNormal("BooksController", "UserClick", $"View updated successfully for bookId: {bookId}", loggedInUserId.ToString());

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
            //SimpleLogger.LogNormal("BooksController", "ViewMyListings", $"Request for userId: {userId}", userId.ToString());

            var books = await _context.Books
                .Where(b => b.UserId == userId /*&& !b.IsSold*/)
                .Select(b => new
                {
                    b.Id,
                    b.BookName,
                    b.AuthorOrPublication,
                    b.CategoryId,
                    b.Category,
                    b.Description,
                    b.SubCategoryId,
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
                b.CategoryId,
                b.Category,
                b.Description,
                b.SubCategoryId,
                b.SubCategory,
                b.SellingPrice,
                Images = string.IsNullOrEmpty(b.ImagePathsJson)
                            ? new string[0]
                            : System.Text.Json.JsonSerializer.Deserialize<string[]>(b.ImagePathsJson) ?? new string[0],
                b.IsSold,
                b.CreatedAt,
                b.Views
            });

           // SimpleLogger.LogNormal("BooksController", "ViewMyListings", $"Retrieved {books.Count} books", userId.ToString());
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
            //SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Request for bookId: {bookId}", bookId.ToString());

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
               // SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Book not found: {bookId}", bookId.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            if (book.IsSold)
            {
                return BadRequest(new { Message = "This book is already marked as sold." });
            }

            book.IsSold = true;
            await _context.SaveChangesAsync();

            //SimpleLogger.LogNormal("BooksController", "MarkAsSold", $"Book marked as sold successfully: {bookId}", bookId.ToString());
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
            //SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Request for bookId: {bookId}", bookId.ToString());

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                //SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Book not found: {bookId}", bookId.ToString());
                return NotFound(new { Message = "Book not found" });
            }

            if (!book.IsSold)
            {
                return BadRequest(new { Message = "This book is already marked as unsold." });
            }

            book.IsSold = false;
            await _context.SaveChangesAsync();

            //SimpleLogger.LogNormal("BooksController", "MarkAsUnSold", $"Book marked as unsold successfully: {bookId}", bookId.ToString());
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
           // SimpleLogger.LogNormal("BooksController", "Delete", $"Delete request for bookId: {bookId}", bookId.ToString());

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

                       // SimpleLogger.LogNormal("BooksController", "Delete", $"Deleted {deletedCount} images for bookId: {bookId}", bookId.ToString());
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

            //SimpleLogger.LogNormal("BooksController", "Delete", $"Book deleted successfully: {bookId}", bookId.ToString());
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
               // SimpleLogger.LogCritical("BooksController", "ListBook", $"User location not found for userId: {dto.UserId}", null, dto.UserId.ToString());
                return BadRequest("User location not found. Please update your location first.");
            }

            SimpleLogger.LogNormal("BooksController", "ListBook", $"Book listing request for userId: {dto.UserId}, BookName: {dto.BookName}", dto.UserId.ToString());

            // Validate user exists
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
               // SimpleLogger.LogCritical("BooksController", "ListBook", $"User not found: {dto.UserId}", null, dto.UserId.ToString());
                return NotFound(new { Message = "User not found" });
            }

            // Validate image count
            if (dto.Images == null || dto.Images.Length < 1 || dto.Images.Length > 4)
            {
                //SimpleLogger.LogCritical("BooksController", "ListBook", $"Invalid image count: {dto.Images?.Length ?? 0}", null, dto.UserId.ToString());
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
                Category = dto.Category,
                CategoryId = dto.CategoryId,
                SubCategory = dto.SubCategory,
                SubCategoryId = dto.SubCategoryId,
                SellingPrice = dto.SellingPrice,
                ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(imagePaths),
                CreatedAt = IndianTimeHelper.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

           // SimpleLogger.LogNormal("BooksController", "ListBook", $"Book listed successfully with ID: {book.Id}", dto.UserId.ToString());
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
                book.Category = dto.Category;
            if (dto.CategoryId.HasValue)
                book.CategoryId = dto.CategoryId;
            if (!string.IsNullOrWhiteSpace(dto.Description))
                book.Description = dto.Description.Trim();
            if (!string.IsNullOrWhiteSpace(dto.SubCategory))
                book.SubCategory = dto.SubCategory;
            if (dto.SubCategoryId.HasValue)
                book.SubCategoryId = dto.SubCategoryId;
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

            //SimpleLogger.LogNormal("BooksController", "EditListing", $"Book updated successfully: {id}", id.ToString());
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
    public async Task<IActionResult> ViewAll(Guid userId, int distance = 50, int page = 1, int pageSize = 50, string? search = null)
    {
        try
        {
            var currentUserLocation = await _context.UserLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUserLocation == null)
            {
                SimpleLogger.LogCritical("BooksController", "ViewAll", $"User location not found for userId: {userId}", null, userId.ToString());
                return BadRequest("User location not found.");
            }

            List<object>? books = null;
            List<object>? banners = null;
            bool isSearchMode = !string.IsNullOrWhiteSpace(search);

            var cacheKey = $"ViewAll_{userId}_{distance}";

            if (!isSearchMode)
            {
                if (_cache.TryGetValue(cacheKey, out Tuple<List<object>, List<object>> cacheResult))
                {
                    books = cacheResult.Item1;
                    banners = cacheResult.Item2;
                }
            }

            if (books == null || banners == null)
            {
                double currentRadius = distance;
                const double kmPerDegreeLat = 111.0;
                double kmPerDegreeLon = 111.0 * Math.Cos(ToRadians(currentUserLocation.Latitude));
                double latOffset = currentRadius / kmPerDegreeLat;
                double lonOffset = currentRadius / kmPerDegreeLon;
                double minLat = currentUserLocation.Latitude - latOffset;
                double maxLat = currentUserLocation.Latitude + latOffset;
                double minLon = currentUserLocation.Longitude - lonOffset;
                double maxLon = currentUserLocation.Longitude + lonOffset;

                // Project only necessary fields
                var query = from b in _context.Books.AsNoTracking()
                            join ul in _context.UserLocations.AsNoTracking() on b.UserId equals ul.UserId
                            where !b.IsSold &&
                                  ul.Latitude >= minLat && ul.Latitude <= maxLat &&
                                  ul.Longitude >= minLon && ul.Longitude <= maxLon &&
                                  (isSearchMode ? EF.Functions.Like(b.BookName, $"%{search}%") : true)
                            orderby
                                Math.Abs(ul.Latitude - currentUserLocation.Latitude) +
                                Math.Abs(ul.Longitude - currentUserLocation.Longitude)
                            select new
                            {
                                b.Id,
                                b.UserId,
                                BookName = b.BookName,
                                AuthorOrPublication = b.AuthorOrPublication,
                                Description = b.Description,
                                Category = b.Category,
                                CategoryId = b.CategoryId,
                                SubCategory = b.SubCategory,
                                SubCategoryId = b.SubCategoryId,
                                SellingPrice = b.SellingPrice,
                                IsSold = b.IsSold,
                                ImagePathsJson = b.ImagePathsJson,
                                CreatedAt = b.CreatedAt,
                                UserName = b.User.Name,
                                UserPhone = b.User.Phone,
                                Latitude = ul.Latitude,
                                Longitude = ul.Longitude
                            };

                int takeCount = pageSize > 200 ? pageSize : 200;
                var nearbyBooks = (await query.Take(takeCount).ToListAsync())
                    .GroupBy(b => b.Id)
                    .Select(g => g.First())
                    .ToList();

                if (nearbyBooks.Count == 0)
                {
                    books = new List<object>();
                }
                else
                {
                    var booksWithDistances = nearbyBooks
                        .AsParallel()
                        .Select(item =>
                        {
                            var exactDistance = CalculateDistance(
                                currentUserLocation.Latitude,
                                currentUserLocation.Longitude,
                                item.Latitude,
                                item.Longitude);
                            return new
                            {
                                item.Id,
                                item.UserId,
                                item.BookName,
                                item.AuthorOrPublication,
                                item.Description,
                                item.Category,
                                item.SubCategory,
                                item.SellingPrice,
                                item.IsSold,
                                item.ImagePathsJson,
                                item.CreatedAt,
                                item.UserName,
                                item.UserPhone,
                                Distance = exactDistance
                            };
                        })
                        .Where(b => b.Distance <= currentRadius)
                        .OrderBy(b => b.Distance)
                        .Take(200)
                        .ToList();

                    books = booksWithDistances.Select(b => (object)new
                    {
                        b.Id,
                        b.UserId,
                        UserName = b.UserName,
                        Phone = b.UserPhone,
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
                        City = "N/A",
                        District = "N/A",
                        DistanceValue = b.Distance,
                        Distance = b.Distance < 1
                            ? $"{Math.Round(b.Distance * 1000)} m"
                            : $"{Math.Round(b.Distance, 2)} km"
                    }).ToList();
                }

                // Fetch all banners with valid location and radius
                var bannerCandidates = await _context.Banners
                    .AsNoTracking()
                    .Where(b => b.Latitude.HasValue && b.Longitude.HasValue && b.Radius.HasValue)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.ImageURL,
                        b.RedirectURL,
                        b.Latitude,
                        b.Longitude,
                        b.Radius
                    })
                    .ToListAsync();

                // Filter in memory using the Haversine formula
                banners = bannerCandidates
                    .Where(b => CalculateDistance(
                        currentUserLocation.Latitude,
                        currentUserLocation.Longitude,
                        b.Latitude.Value,
                        b.Longitude.Value
                    ) <= b.Radius.Value)
                    .Select(b => (object)b)
                    .ToList();

                // Cache books and banners together for ViewAll (no search)
                if (!isSearchMode)
                {
                    _cache.Set(cacheKey, Tuple.Create(books, banners), TimeSpan.FromSeconds(60));
                }
            }

            // Pagination for books only
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
                Books = paginatedBooks,
                banners
            });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("BooksController", "ViewAll", "ViewAll method failed", ex, userId.ToString());
            return StatusCode(500, "Failed to retrieve books");
        }
    }

    // Helper for radians
    private static double ToRadians(double deg) => deg * Math.PI / 180.0;

    // Haversine formula for accurate geo distance (in kilometers)
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    // DTO for books and banners caching
    public class BooksAndBannersResult
    {
        public List<object> Books { get; set; }
        public List<object> Banners { get; set; }
    }

    // Optionally, a fast approximate distance (not used in the final code)
    //private static double CalculateApproxDistance(double lat1, double lon1, double lat2, double lon2)
    //{
    //    // Pythagorean approximation (not as accurate as Haversine for larger distances)
    //    const double kmPerDegreeLat = 111.0;
    //    var kmPerDegreeLon = 111.0 * Math.Cos(ToRadians(lat1));
    //    var dLat = lat1 - lat2;
    //    var dLon = lon1 - lon2;
    //    return Math.Sqrt(Math.Pow(dLat * kmPerDegreeLat, 2) + Math.Pow(dLon * kmPerDegreeLon, 2));
    //}

    // Your CalculateDistance and CalculateApproxDistance should be present as before


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

    // Helper methods for category name resolution
    private string GetCategoryName(int? categoryId)
    {
        if (!categoryId.HasValue) return string.Empty;

        return categoryId.Value switch
        {
            1 => "Primary School Books",
            2 => "Secondary School Books",
            3 => "Senior Secondary Books",
            4 => "Engineering Books",
            5 => "Medical & Health Sciences",
            6 => "Law Books",
            7 => "Business & Management",
            8 => "Arts & Humanities",
            9 => "Computer Science & IT",
            10 => "Competitive Exams",
            11 => "Novels & Literature",
            12 => "Children's Books",
            13 => "Reference Books",
            14 => "Magazines & Periodicals",
            15 => "Others",
            _ => "Unknown Category"
        };
    }

    private string GetSubCategoryName(int? categoryId, int? subCategoryId)
    {
        if (!categoryId.HasValue || !subCategoryId.HasValue) return string.Empty;

        return (categoryId.Value, subCategoryId.Value) switch
        {
            // Primary School Books
            (1, 1) => "Class I",
            (1, 2) => "Class II",
            (1, 3) => "Class III",
            (1, 4) => "Class IV",
            (1, 5) => "Class V",
            (1, 6) => "Activity Books",
            (1, 7) => "Story Books",
            (1, 8) => "Learning Kits",

            // Secondary School Books
            (2, 1) => "Class VI",
            (2, 2) => "Class VII",
            (2, 3) => "Class VIII",
            (2, 4) => "Class IX",
            (2, 5) => "Class X",
            (2, 6) => "Science",
            (2, 7) => "Math",
            (2, 8) => "Social Studies",
            (2, 9) => "Language",
            (2, 10) => "Exam Prep",

            // Senior Secondary Books
            (3, 1) => "Class XI",
            (3, 2) => "Class XII",
            (3, 3) => "Science",
            (3, 4) => "Commerce",
            (3, 5) => "Arts",
            (3, 6) => "Board Exam Guides",
            (3, 7) => "Reference Books",

            // Engineering Books
            (4, 1) => "Computer Science",
            (4, 2) => "Mechanical",
            (4, 3) => "Electrical",
            (4, 4) => "Civil",
            (4, 5) => "Chemical",
            (4, 6) => "Electronics",
            (4, 7) => "Aerospace",
            (4, 8) => "Biomedical",

            // Medical & Health Sciences
            (5, 1) => "Anatomy",
            (5, 2) => "Physiology",
            (5, 3) => "Pharmacology",
            (5, 4) => "Pathology",
            (5, 5) => "Microbiology",
            (5, 6) => "Surgery",
            (5, 7) => "Medicine",
            (5, 8) => "Nursing",

            // Law Books
            (6, 1) => "Constitutional Law",
            (6, 2) => "Criminal Law",
            (6, 3) => "Civil Law",
            (6, 4) => "Corporate Law",
            (6, 5) => "International Law",
            (6, 6) => "Tax Law",
            (6, 7) => "Environmental Law",
            (6, 8) => "Human Rights",

            // Business & Management
            (7, 1) => "Marketing",
            (7, 2) => "Finance",
            (7, 3) => "Human Resources",
            (7, 4) => "Operations",
            (7, 5) => "Strategy",
            (7, 6) => "Entrepreneurship",
            (7, 7) => "Economics",
            (7, 8) => "Accounting",

            // Arts & Humanities
            (8, 1) => "History",
            (8, 2) => "Philosophy",
            (8, 3) => "Literature",
            (8, 4) => "Psychology",
            (8, 5) => "Sociology",
            (8, 6) => "Political Science",
            (8, 7) => "Geography",
            (8, 8) => "Languages",

            // Computer Science & IT
            (9, 1) => "Programming",
            (9, 2) => "Data Structures",
            (9, 3) => "Algorithms",
            (9, 4) => "Databases",
            (9, 5) => "Web Development",
            (9, 6) => "Mobile Development",
            (9, 7) => "AI & Machine Learning",
            (9, 8) => "Cybersecurity",

            // Competitive Exams
            (10, 1) => "UPSC",
            (10, 2) => "SSC",
            (10, 3) => "Banking",
            (10, 4) => "Railway",
            (10, 5) => "Defence",
            (10, 6) => "Teaching",
            (10, 7) => "Engineering",
            (10, 8) => "Medical",

            // Novels & Literature
            (11, 1) => "Fiction",
            (11, 2) => "Non-Fiction",
            (11, 3) => "Biographies",
            (11, 4) => "Poetry",
            (11, 5) => "Drama",
            (11, 6) => "Short Stories",
            (11, 7) => "Classics",
            (11, 8) => "Contemporary",

            // Children's Books
            (12, 1) => "Picture Books",
            (12, 2) => "Early Readers",
            (12, 3) => "Chapter Books",
            (12, 4) => "Young Adult",
            (12, 5) => "Educational",
            (12, 6) => "Arts & Crafts",
            (12, 7) => "Music",
            (12, 8) => "Sports",
            (12, 9) => "Coding for Kids",
            (12, 10) => "Puzzle Books",
            (12, 11) => "Story Books",
            (12, 12) => "Comics",

            // Reference Books
            (13, 1) => "Dictionaries",
            (13, 2) => "Encyclopedias",
            (13, 3) => "Atlases",
            (13, 4) => "Almanacs",
            (13, 5) => "Directories",
            (13, 6) => "Guides",
            (13, 7) => "Manuals",
            (13, 8) => "Yearbooks",

            // Magazines & Periodicals
            (14, 1) => "News",
            (14, 2) => "Technology",
            (14, 3) => "Science",
            (14, 4) => "Business",
            (14, 5) => "Entertainment",
            (14, 6) => "Sports",
            (14, 7) => "Health",
            (14, 8) => "Lifestyle",

            // Others
            (15, 1) => "Cookbooks",
            (15, 2) => "Travel",
            (15, 3) => "Self-Help",
            (15, 4) => "Religion",
            (15, 5) => "Hobbies",
            (15, 6) => "Miscellaneous",
            (15, 7) => "Antiques",
            (15, 8) => "Collectibles",

            _ => "Unknown SubCategory"
        };
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
    //private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    //{
    //    const double R = 6371.0; // Earth's radius in km
    //    double dLat = ToRadians(lat2 - lat1);
    //    double dLon = ToRadians(lon2 - lon1);
    //    double lat1Rad = ToRadians(lat1);
    //    double lat2Rad = ToRadians(lat2);

    //    double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
    //               Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
    //               Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

    //    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    //    return R * c;
    //}

    // Fast approximate distance (Euclidean, scaled to km) for initial sorting
    //private double CalculateApproxDistance(double lat1, double lon1, double lat2, double lon2)
    //{
    //    const double kmPerDegreeLat = 111.0;
    //    const double kmPerDegreeLon = 111.0; // Average, adjust for latitude if needed
    //    var dLat = (lat2 - lat1) * kmPerDegreeLat;
    //    var dLon = (lon2 - lon1) * kmPerDegreeLon * Math.Cos(ToRadians((lat1 + lat2) / 2));
    //    return Math.Sqrt(dLat * dLat + dLon * dLon);
    //}
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
    [Required] public required int CategoryId { get; set; }
    [Required] public required string Category { get; set; }
    [Required] public required string Description { get; set; }
    public int? SubCategoryId { get; set; }
    public string? SubCategory { get; set; }
    [Required] public decimal SellingPrice { get; set; }

    [Required]
    public IFormFile[] Images { get; set; } = Array.Empty<IFormFile>();
}

public class BookEditDto
{
    public string? BookName { get; set; }
    public string? AuthorOrPublication { get; set; }
    public int? CategoryId { get; set; }
    public string? Category { get; set; }
    public int? SubCategoryId { get; set; }
    public string? SubCategory { get; set; }
    public decimal? SellingPrice { get; set; }
    public string? Description { get; set; }
    public IFormFile[]? NewImages { get; set; }
    public string[]? ExistingImages { get; set; }
}
