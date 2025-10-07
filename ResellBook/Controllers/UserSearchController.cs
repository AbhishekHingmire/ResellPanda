using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSearchController : ControllerBase
    {
        private readonly string _logFilePath;

        public UserSearchController(IWebHostEnvironment env)
        {
            // Store log file in wwwroot/logs folder
            _logFilePath = Path.Combine("AppLogs", "UserSearchLog.txt");

            // Ensure folder exists
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        [HttpPost("LogSearch")]
        public IActionResult LogSearch([FromBody] UserSearchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | UserId: {dto.UserId} | SearchTerm: {dto.SearchTerm}";

            // Append log to file
            System.IO.File.AppendAllText(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);

            return Ok(new { Message = "Search logged successfully" });
        }
    }
    public class UserSearchDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string SearchTerm { get; set; }
        
       
    }
}
