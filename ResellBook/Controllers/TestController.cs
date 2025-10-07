using Microsoft.AspNetCore.Mvc;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "Pong!", timestamp = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") });
        }
        
        [HttpGet("check-logs-folder")]
        public IActionResult CheckLogsFolder()
        {
            var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            var exists = Directory.Exists(logsPath);
            
            return Ok(new 
            { 
                logsPath = logsPath,
                exists = exists,
                files = exists ? Directory.GetFiles(logsPath) : Array.Empty<string>()
            });
        }
    }
}