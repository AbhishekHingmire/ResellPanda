using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;
using ResellBook.Helpers;
using System.ComponentModel.DataAnnotations;

    [ApiController]
    [Route("api/[controller]")]
    public class UserLocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserLocationController(AppDbContext context)
        {
            _context = context;
        }

    // POST: api/UserLocation/SyncLocation
    [HttpPost("SyncLocation")]
    public IActionResult SyncLocation([FromBody] UserLocation request)
    {
        if (!_context.Users.Any(u => u.Id == request.UserId))
            return NotFound(new { Message = "User not found" });

        var userData = _context.UserLocations.FirstOrDefault(u => u.UserId == request.UserId);
        if (userData == null)
        {
            var location = new UserLocation
            {
                UserId = request.UserId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreateDate = IndianTimeHelper.UtcNow
            };
            _context.UserLocations.Add(location);
        }
        else
        {
            // This is the actual update command:
            userData.Latitude = request.Latitude;
            userData.Longitude = request.Longitude;
            userData.CreateDate = IndianTimeHelper.UtcNow;
            // No need to call Update; EF Core tracks changes automatically
        }
        _context.SaveChanges();
        return Ok(new { Message = "Location synced successfully" });
    }

    // GET: api/UserLocation/GetLocations/{userId}
    [HttpGet("GetLocations/{userId}")]
        public IActionResult GetLocations(Guid userId)
        {
            var locations = _context.UserLocations
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreateDate)
                .ToList();

            if (!locations.Any())
                return NotFound(new { Message = "No locations found for this user" });

            return Ok(locations);
        }
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found.");

            var profile = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.IsEmailVerified
            };

            return Ok(profile);
    }
    [HttpPut("EditUser/{id}")]
    public async Task<IActionResult> EditUser(Guid id, [FromBody] EditUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        // Update fields
        user.Name = string.IsNullOrWhiteSpace(dto.Name) ? user.Name : dto.Name;
        user.Email = string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email;
        user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? user.Phone : dto.Phone;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "User updated successfully" });
    }
    public class EditUserDto
    {
        public string? Name { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}

