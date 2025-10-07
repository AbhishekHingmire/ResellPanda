using ResellBook.Helpers;

namespace ResellBook.Utils
{
    public static class SimpleLogger
    {
        private static readonly string LogsFolder = Path.Combine(Directory.GetCurrentDirectory(), "AppLogs");
        private static readonly string NormalLogFile = Path.Combine(LogsFolder, "normal.txt");
        private static readonly string CriticalLogFile = Path.Combine(LogsFolder, "critical.txt");

        static SimpleLogger()
        {
            // Ensure logs directory exists
            if (!Directory.Exists(LogsFolder))
                Directory.CreateDirectory(LogsFolder);
        }

        public static void LogNormal(string controller, string method, string message, string? userId = null)
        {
            try
            {
                var timestamp = IndianTimeHelper.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                var logLine = $"{timestamp} | {controller} | {method} | {message} | UserId: {userId ?? "N/A"}";
                
                File.AppendAllText(NormalLogFile, logLine + Environment.NewLine);
            }
            catch
            {
                // Silent fail - don't break app if logging fails
            }
        }

        public static void LogCritical(string controller, string method, string message, Exception? ex = null, string? userId = null)
        {
            try
            {
                var timestamp = IndianTimeHelper.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
                var errorInfo = ex != null ? $" | Error: {ex.Message} | Stack: {ex.StackTrace}" : "";
                var logLine = $"{timestamp} | {controller} | {method} | {message} | UserId: {userId ?? "N/A"}{errorInfo}";
                
                File.AppendAllText(CriticalLogFile, logLine + Environment.NewLine);
            }
            catch
            {
                // Silent fail - don't break app if logging fails
            }
        }

        public static List<string> GetNormalLogs(int maxLines = 100)
        {
            try
            {
                if (!File.Exists(NormalLogFile))
                    return new List<string>();

                var lines = File.ReadAllLines(NormalLogFile);
                return lines.Reverse().Take(maxLines).ToList(); // Most recent first
            }
            catch
            {
                return new List<string> { "Error reading normal logs" };
            }
        }

        public static List<string> GetCriticalLogs(int maxLines = 100)
        {
            try
            {
                if (!File.Exists(CriticalLogFile))
                    return new List<string>();

                var lines = File.ReadAllLines(CriticalLogFile);
                return lines.Reverse().Take(maxLines).ToList(); // Most recent first
            }
            catch
            {
                return new List<string> { "Error reading critical logs" };
            }
        }

        public static (int normalCount, int criticalCount) GetLogCounts()
        {
            try
            {
                var normalCount = File.Exists(NormalLogFile) ? File.ReadAllLines(NormalLogFile).Length : 0;
                var criticalCount = File.Exists(CriticalLogFile) ? File.ReadAllLines(CriticalLogFile).Length : 0;
                return (normalCount, criticalCount);
            }
            catch
            {
                return (0, 0);
            }
        }

        public static void ClearLogs()
        {
            try
            {
                if (File.Exists(NormalLogFile))
                    File.WriteAllText(NormalLogFile, string.Empty);
                if (File.Exists(CriticalLogFile))
                    File.WriteAllText(CriticalLogFile, string.Empty);
            }
            catch
            {
                // Silent fail
            }
        }
    }
}