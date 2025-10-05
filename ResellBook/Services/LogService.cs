using ResellBook.Models;
using ResellBook.Helpers;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace ResellBook.Services
{
    public interface ILogService
    {
        Task LogNormalAsync(string controller, string method, string message, string? requestPath = null, string? userId = null);
        Task LogCriticalAsync(string controller, string method, string message, Exception? exception = null, string? requestPath = null, string? userId = null);
        Task<LogResponse> GetNormalLogsAsync(int page = 1, int pageSize = 50);
        Task<LogResponse> GetCriticalLogsAsync(int page = 1, int pageSize = 50);
        Task<LogSummaryResponse> GetLogsSummaryAsync();
    }

    public class FileLogService : ILogService
    {
        private readonly string _normalLogsPath;
        private readonly string _criticalLogsPath;
        private readonly SemaphoreSlim _normalLogSemaphore;
        private readonly SemaphoreSlim _criticalLogSemaphore;

        public FileLogService(IWebHostEnvironment env)
        {
            var logsDir = Path.Combine(env.ContentRootPath, "Logs");
            Directory.CreateDirectory(logsDir);
            
            _normalLogsPath = Path.Combine(logsDir, "normal_logs.txt");
            _criticalLogsPath = Path.Combine(logsDir, "critical_logs.txt");
            
            _normalLogSemaphore = new SemaphoreSlim(1, 1);
            _criticalLogSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LogNormalAsync(string controller, string method, string message, string? requestPath = null, string? userId = null)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = IndianTimeHelper.Now,
                    LogLevel = "NORMAL",
                    Controller = controller,
                    Method = method,
                    Message = message,
                    RequestPath = requestPath,
                    UserId = userId
                };

                await WriteLogAsync(_normalLogsPath, logEntry, _normalLogSemaphore);
            }
            catch
            {
                // Silently fail to prevent logging issues from affecting app functionality
            }
        }

        public async Task LogCriticalAsync(string controller, string method, string message, Exception? exception = null, string? requestPath = null, string? userId = null)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = IndianTimeHelper.Now,
                    LogLevel = "CRITICAL",
                    Controller = controller,
                    Method = method,
                    Message = message,
                    Exception = exception?.Message,
                    StackTrace = exception?.StackTrace,
                    RequestPath = requestPath,
                    UserId = userId
                };

                await WriteLogAsync(_criticalLogsPath, logEntry, _criticalLogSemaphore);
            }
            catch
            {
                // Silently fail to prevent logging issues from affecting app functionality
            }
        }

        private async Task WriteLogAsync(string filePath, LogEntry logEntry, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                var logLine = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });
                
                await File.AppendAllTextAsync(filePath, logLine + Environment.NewLine);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<LogResponse> GetNormalLogsAsync(int page = 1, int pageSize = 50)
        {
            return await ReadLogsAsync(_normalLogsPath, page, pageSize);
        }

        public async Task<LogResponse> GetCriticalLogsAsync(int page = 1, int pageSize = 50)
        {
            return await ReadLogsAsync(_criticalLogsPath, page, pageSize);
        }

        private async Task<LogResponse> ReadLogsAsync(string filePath, int page, int pageSize)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new LogResponse
                    {
                        Success = true,
                        Logs = new List<LogEntry>(),
                        TotalCount = 0
                    };
                }

                var lines = await File.ReadAllLinesAsync(filePath);
                var logs = new List<LogEntry>();

                foreach (var line in lines.Reverse()) // Most recent first
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var logEntry = JsonSerializer.Deserialize<LogEntry>(line);
                            if (logEntry != null)
                                logs.Add(logEntry);
                        }
                    }
                    catch
                    {
                        // Skip malformed log entries
                    }
                }

                var totalCount = logs.Count;
                var pagedLogs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return new LogResponse
                {
                    Success = true,
                    Logs = pagedLogs,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new LogResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to read logs: {ex.Message}",
                    Logs = new List<LogEntry>(),
                    TotalCount = 0
                };
            }
        }

        public async Task<LogSummaryResponse> GetLogsSummaryAsync()
        {
            try
            {
                var normalCount = 0;
                var criticalCount = 0;
                DateTime? latestTime = null;
                DateTime? oldestTime = null;

                // Count normal logs
                if (File.Exists(_normalLogsPath))
                {
                    var normalLines = await File.ReadAllLinesAsync(_normalLogsPath);
                    normalCount = normalLines.Count(line => !string.IsNullOrWhiteSpace(line));
                }

                // Count critical logs
                if (File.Exists(_criticalLogsPath))
                {
                    var criticalLines = await File.ReadAllLinesAsync(_criticalLogsPath);
                    criticalCount = criticalLines.Count(line => !string.IsNullOrWhiteSpace(line));
                }

                // Get time range from both files
                var allTimes = new List<DateTime>();
                
                foreach (var path in new[] { _normalLogsPath, _criticalLogsPath })
                {
                    if (File.Exists(path))
                    {
                        var lines = await File.ReadAllLinesAsync(path);
                        foreach (var line in lines)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    var logEntry = JsonSerializer.Deserialize<LogEntry>(line);
                                    if (logEntry != null)
                                        allTimes.Add(logEntry.Timestamp);
                                }
                            }
                            catch { /* Skip malformed entries */ }
                        }
                    }
                }

                if (allTimes.Any())
                {
                    latestTime = allTimes.Max();
                    oldestTime = allTimes.Min();
                }

                return new LogSummaryResponse
                {
                    Success = true,
                    NormalLogsCount = normalCount,
                    CriticalLogsCount = criticalCount,
                    LatestLogTime = latestTime,
                    OldestLogTime = oldestTime
                };
            }
            catch (Exception ex)
            {
                return new LogSummaryResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to get logs summary: {ex.Message}"
                };
            }
        }
    }
}