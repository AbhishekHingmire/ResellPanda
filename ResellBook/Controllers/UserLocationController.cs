using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;

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

            var location = new UserLocation
            {
                UserId = request.UserId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreateDate = DateTime.UtcNow
            };
        var userData = _context.UserLocations.Where(u => u.UserId == request.UserId).FirstOrDefault();
        if (userData == null)
        {
            _context.UserLocations.Add(location);
        }
        else
        {
            location.Id = userData.Id;
            _context.UserLocations.Update(location);
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
                user.IsEmailVerified
            };

            return Ok(profile);
        }

    }

