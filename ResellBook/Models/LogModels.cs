using System.ComponentModel.DataAnnotations;

namespace ResellBook.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string? RequestPath { get; set; }
        public string? UserId { get; set; }
    }

    public class LogResponse
    {
        public bool Success { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LogSummaryResponse
    {
        public bool Success { get; set; }
        public int NormalLogsCount { get; set; }
        public int CriticalLogsCount { get; set; }
        public DateTime? LatestLogTime { get; set; }
        public DateTime? OldestLogTime { get; set; }
        public string? ErrorMessage { get; set; }
    }
}