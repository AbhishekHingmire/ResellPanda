using Microsoft.AspNetCore.Mvc;
using ResellBook.Services;
using ResellBook.Models;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Get normal logs with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of logs per page (default: 50, max: 100)</param>
        /// <returns>Paginated normal logs</returns>
        [HttpGet("GetNormalLogs")]
        public async Task<IActionResult> GetNormalLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                // Validate parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                await _logService.LogNormalAsync("LogsController", "GetNormalLogs", 
                    $"Fetching normal logs - Page: {page}, PageSize: {pageSize}", Request.Path);

                var result = await _logService.GetNormalLogsAsync(page, pageSize);
                
                if (!result.Success)
                {
                    await _logService.LogCriticalAsync("LogsController", "GetNormalLogs", 
                        "Failed to retrieve normal logs", null, Request.Path);
                    return StatusCode(500, new { message = "Failed to retrieve logs", error = result.ErrorMessage });
                }

                return Ok(new
                {
                    success = true,
                    data = result.Logs,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = result.TotalCount,
                        totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                        hasNextPage = page * pageSize < result.TotalCount,
                        hasPreviousPage = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                await _logService.LogCriticalAsync("LogsController", "GetNormalLogs", 
                    "Unexpected error while fetching normal logs", ex, Request.Path);
                    
                return StatusCode(500, new { 
                    success = false, 
                    message = "An unexpected error occurred while retrieving logs" 
                });
            }
        }

        /// <summary>
        /// Get critical logs with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of logs per page (default: 50, max: 100)</param>
        /// <returns>Paginated critical logs</returns>
        [HttpGet("GetCriticalLogs")]
        public async Task<IActionResult> GetCriticalLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                // Validate parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                await _logService.LogNormalAsync("LogsController", "GetCriticalLogs", 
                    $"Fetching critical logs - Page: {page}, PageSize: {pageSize}", Request.Path);

                var result = await _logService.GetCriticalLogsAsync(page, pageSize);
                
                if (!result.Success)
                {
                    await _logService.LogCriticalAsync("LogsController", "GetCriticalLogs", 
                        "Failed to retrieve critical logs", null, Request.Path);
                    return StatusCode(500, new { message = "Failed to retrieve logs", error = result.ErrorMessage });
                }

                return Ok(new
                {
                    success = true,
                    data = result.Logs,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = result.TotalCount,
                        totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                        hasNextPage = page * pageSize < result.TotalCount,
                        hasPreviousPage = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                await _logService.LogCriticalAsync("LogsController", "GetCriticalLogs", 
                    "Unexpected error while fetching critical logs", ex, Request.Path);
                    
                return StatusCode(500, new { 
                    success = false, 
                    message = "An unexpected error occurred while retrieving logs" 
                });
            }
        }

        /// <summary>
        /// Get logs summary with counts and time ranges
        /// </summary>
        /// <returns>Summary of all logs</returns>
        [HttpGet("GetLogsSummary")]
        public async Task<IActionResult> GetLogsSummary()
        {
            try
            {
                await _logService.LogNormalAsync("LogsController", "GetLogsSummary", 
                    "Fetching logs summary", Request.Path);

                var result = await _logService.GetLogsSummaryAsync();
                
                if (!result.Success)
                {
                    await _logService.LogCriticalAsync("LogsController", "GetLogsSummary", 
                        "Failed to retrieve logs summary", null, Request.Path);
                    return StatusCode(500, new { message = "Failed to retrieve logs summary", error = result.ErrorMessage });
                }

                return Ok(new
                {
                    success = true,
                    summary = new
                    {
                        normalLogsCount = result.NormalLogsCount,
                        criticalLogsCount = result.CriticalLogsCount,
                        totalLogsCount = result.NormalLogsCount + result.CriticalLogsCount,
                        latestLogTime = result.LatestLogTime?.ToString("dd/MM/yyyy hh:mm:ss tt"),
                        oldestLogTime = result.OldestLogTime?.ToString("dd/MM/yyyy hh:mm:ss tt"),
                        timeZone = "IST (Indian Standard Time)"
                    }
                });
            }
            catch (Exception ex)
            {
                await _logService.LogCriticalAsync("LogsController", "GetLogsSummary", 
                    "Unexpected error while fetching logs summary", ex, Request.Path);
                    
                return StatusCode(500, new { 
                    success = false, 
                    message = "An unexpected error occurred while retrieving logs summary" 
                });
            }
        }

        /// <summary>
        /// Clear all logs (use with caution)
        /// </summary>
        /// <returns>Confirmation of log clearing</returns>
        [HttpPost("ClearAllLogs")]
        public async Task<IActionResult> ClearAllLogs()
        {
            try
            {
                await _logService.LogCriticalAsync("LogsController", "ClearAllLogs", 
                    "Log clearing requested - This is a critical operation", null, Request.Path);

                var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                var normalLogsPath = Path.Combine(logsPath, "normal_logs.txt");
                var criticalLogsPath = Path.Combine(logsPath, "critical_logs.txt");

                if (System.IO.File.Exists(normalLogsPath))
                    await System.IO.File.WriteAllTextAsync(normalLogsPath, string.Empty);

                if (System.IO.File.Exists(criticalLogsPath))
                    await System.IO.File.WriteAllTextAsync(criticalLogsPath, string.Empty);

                return Ok(new
                {
                    success = true,
                    message = "All logs have been cleared successfully",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to clear logs", 
                    error = ex.Message 
                });
            }
        }
    }
}