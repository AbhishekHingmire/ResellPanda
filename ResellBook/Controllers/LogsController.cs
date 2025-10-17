using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResellBook.Utils;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("GetNormalLogs")]
        public IActionResult GetNormalLogs([FromQuery] int maxLines = 100)
        {
            try
            {
                SimpleLogger.LogNormal("LogsController", "GetNormalLogs", $"Fetching {maxLines} normal logs");
                
                var logs = SimpleLogger.GetNormalLogs(maxLines);
                
                return Ok(new
                {
                    success = true,
                    data = logs,
                    count = logs.Count,
                    message = $"Retrieved {logs.Count} normal log entries"
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("LogsController", "GetNormalLogs", "Failed to retrieve normal logs", ex);
                
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Failed to retrieve normal logs", 
                    error = ex.Message 
                });
            }
        }
        [AllowAnonymous]
        [HttpGet("GetCriticalLogs")]
        public IActionResult GetCriticalLogs([FromQuery] int maxLines = 100)
        {
            try
            {
                SimpleLogger.LogNormal("LogsController", "GetCriticalLogs", $"Fetching {maxLines} critical logs");
                
                var logs = SimpleLogger.GetCriticalLogs(maxLines);
                
                return Ok(new
                {
                    success = true,
                    data = logs,
                    count = logs.Count,
                    message = $"Retrieved {logs.Count} critical log entries"
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("LogsController", "GetCriticalLogs", "Failed to retrieve critical logs", ex);
                
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Failed to retrieve critical logs", 
                    error = ex.Message 
                });
            }
        }
        [AllowAnonymous]
        [HttpGet("GetLogsSummary")]
        public IActionResult GetLogsSummary()
        {
            try
            {
                SimpleLogger.LogNormal("LogsController", "GetLogsSummary", "Fetching logs summary");
                
                var (normalCount, criticalCount) = SimpleLogger.GetLogCounts();
                
                return Ok(new
                {
                    success = true,
                    summary = new
                    {
                        normalLogsCount = normalCount,
                        criticalLogsCount = criticalCount,
                        totalLogsCount = normalCount + criticalCount,
                        timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"),
                        timeZone = "IST (Indian Standard Time)"
                    }
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("LogsController", "GetLogsSummary", "Failed to get logs summary", ex);
                
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Failed to get logs summary", 
                    error = ex.Message 
                });
            }
        }
        [AllowAnonymous]
        [HttpPost("ClearAllLogs")]
        public IActionResult ClearAllLogs()
        {
            try
            {
                SimpleLogger.LogCritical("LogsController", "ClearAllLogs", "Log clearing requested - CRITICAL OPERATION");
                
                SimpleLogger.ClearLogs();
                
                return Ok(new
                {
                    success = true,
                    message = "All logs have been cleared successfully",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Failed to clear logs", 
                    error = ex.Message 
                });
            }
        }
        [AllowAnonymous]
        [HttpGet("TestLogging")]
        public IActionResult TestLogging()
        {
            try
            {
                SimpleLogger.LogNormal("LogsController", "TestLogging", "Test normal log entry created");
                SimpleLogger.LogCritical("LogsController", "TestLogging", "Test critical log entry created");
                
                return Ok(new
                {
                    success = true,
                    message = "Test logs created successfully",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Failed to create test logs", 
                    error = ex.Message 
                });
            }
        }
    }
}