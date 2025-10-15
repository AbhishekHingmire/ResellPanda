using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResellBook.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSearchController : ControllerBase
    {
        private readonly string _logFilePath;

        public UserSearchController(IWebHostEnvironment env)
        {
            // Store search data in dedicated user searches log file
            _logFilePath = Path.Combine("AppLogs", "UserSearches.log");

            // Ensure folder exists
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }
        [Authorize]
        [HttpPost("LogSearch")]
        public IActionResult LogSearch([FromBody] UserSearchDto dto)
        {
            try
            {
                SimpleLogger.LogNormal("UserSearchController", "LogSearch", $"Search logged for userId: {dto.UserId}, term: {dto.SearchTerm}", dto.UserId.ToString());

                if (!ModelState.IsValid)
                {
                    SimpleLogger.LogCritical("UserSearchController", "LogSearch", "Invalid model state", null, dto.UserId.ToString());
                    return BadRequest(ModelState);
                }

                var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | UserId: {dto.UserId} | SearchTerm: {dto.SearchTerm}";

                // Append log to file
                System.IO.File.AppendAllText(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);

                SimpleLogger.LogNormal("UserSearchController", "LogSearch", "Search logged successfully", dto.UserId.ToString());
                return Ok(new { Message = "Search logged successfully" });
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("UserSearchController", "LogSearch", "LogSearch failed", ex, dto?.UserId.ToString());
                return StatusCode(500, "Failed to log search");
            }
        }
        [Authorize]
        [HttpGet("GetAllSearches")]
        public IActionResult GetAllSearches([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                SimpleLogger.LogNormal("UserSearchController", "GetAllSearches", $"Request for page: {page}, pageSize: {pageSize}", "System");

                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                // Check if log file exists
                if (!System.IO.File.Exists(_logFilePath))
                {
                    SimpleLogger.LogNormal("UserSearchController", "GetAllSearches", "No search log file found", "System");
                    return Ok(new GetAllSearchesResponse
                    {
                        Success = true,
                        Message = "No searches found",
                        Data = new List<UserSearchLogEntry>(),
                        TotalCount = 0,
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = 0
                    });
                }

                // Read all lines from file
                var allLines = System.IO.File.ReadAllLines(_logFilePath, Encoding.UTF8);
                
                // Parse log entries
                var searchEntries = new List<UserSearchLogEntry>();
                var regex = new Regex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \| UserId: ([a-fA-F0-9\-]{36}) \| SearchTerm: (.+)$");

                foreach (var line in allLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var match = regex.Match(line.Trim());
                    if (match.Success)
                    {
                        if (DateTime.TryParse(match.Groups[1].Value, out var searchDate) &&
                            Guid.TryParse(match.Groups[2].Value, out var userId))
                        {
                            searchEntries.Add(new UserSearchLogEntry
                            {
                                Id = Guid.NewGuid(), // Generate unique ID for this session
                                SearchDate = searchDate,
                                UserId = userId,
                                SearchTerm = match.Groups[3].Value.Trim(),
                                LogEntry = line.Trim()
                            });
                        }
                    }
                }

                // Sort by date ascending (oldest first)
                var sortedEntries = searchEntries.OrderBy(e => e.SearchDate).ToList();

                // Calculate pagination
                var totalCount = sortedEntries.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;
                var pagedEntries = sortedEntries.Skip(skip).Take(pageSize).ToList();

                var response = new GetAllSearchesResponse
                {
                    Success = true,
                    Message = $"Retrieved {pagedEntries.Count} searches (page {page} of {totalPages})",
                    Data = pagedEntries,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                SimpleLogger.LogNormal("UserSearchController", "GetAllSearches", $"Retrieved {pagedEntries.Count} searches from {totalCount} total", "System");
                return Ok(response);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("UserSearchController", "GetAllSearches", "GetAllSearches failed", ex, "System");
                return StatusCode(500, "Failed to retrieve searches");
            }
        }
    }
    public class UserSearchDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class UserSearchLogEntry
    {
        public Guid Id { get; set; }
        public DateTime SearchDate { get; set; }
        public Guid UserId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string LogEntry { get; set; } = string.Empty;
    }

    public class GetAllSearchesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<UserSearchLogEntry> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
