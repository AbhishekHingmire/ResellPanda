using System;

namespace ResellBook.Helpers
{
    /// <summary>
    /// Helper class for handling Indian Standard Time (IST) timezone operations
    /// Ensures all datetime operations use IST regardless of server location
    /// </summary>
    public static class IndianTimeHelper
    {
        // Indian Standard Time timezone
        private static readonly TimeZoneInfo IndianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        
        /// <summary>
        /// Gets current Indian Standard Time
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndianTimeZone);
        
        /// <summary>
        /// Gets current UTC time (for database storage)
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;
        
        /// <summary>
        /// Converts UTC datetime to Indian Standard Time
        /// </summary>
        /// <param name="utcDateTime">UTC datetime</param>
        /// <returns>IST datetime</returns>
        public static DateTime ToIndianTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IndianTimeZone);
        }
        
        /// <summary>
        /// Converts Indian time to UTC for database storage
        /// </summary>
        /// <param name="indianDateTime">IST datetime</param>
        /// <returns>UTC datetime</returns>
        public static DateTime ToUtc(DateTime indianDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(indianDateTime, IndianTimeZone);
        }
        
        /// <summary>
        /// Formats datetime in 12-hour format with IST
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <param name="includeDate">Whether to include date</param>
        /// <returns>Formatted datetime string</returns>
        public static string ToIndianFormat(DateTime dateTime, bool includeDate = true)
        {
            var istTime = ToIndianTime(dateTime);
            
            if (includeDate)
                return istTime.ToString("dd/MM/yyyy hh:mm:ss tt") + " IST";
            else
                return istTime.ToString("hh:mm:ss tt") + " IST";
        }
        
        /// <summary>
        /// Formats datetime in 12-hour format for display (short format)
        /// </summary>
        /// <param name="dateTime">DateTime to format</param>
        /// <returns>Short formatted datetime string</returns>
        public static string ToShortIndianFormat(DateTime dateTime)
        {
            var istTime = ToIndianTime(dateTime);
            return istTime.ToString("dd/MM/yy hh:mm tt");
        }
        
        /// <summary>
        /// Adds minutes to current IST time and returns UTC for storage
        /// </summary>
        /// <param name="minutes">Minutes to add</param>
        /// <returns>UTC datetime for storage</returns>
        public static DateTime AddMinutesToNow(int minutes)
        {
            return UtcNow.AddMinutes(minutes);
        }
        
        /// <summary>
        /// Checks if UTC datetime has expired compared to current IST time
        /// </summary>
        /// <param name="utcExpiryTime">UTC expiry time</param>
        /// <returns>True if expired</returns>
        public static bool IsExpired(DateTime utcExpiryTime)
        {
            return utcExpiryTime < UtcNow;
        }
        
        /// <summary>
        /// Gets relative time string (e.g., "2 minutes ago", "1 hour ago")
        /// </summary>
        /// <param name="utcDateTime">UTC datetime</param>
        /// <returns>Relative time string in IST context</returns>
        public static string GetRelativeTime(DateTime utcDateTime)
        {
            var istTime = ToIndianTime(utcDateTime);
            var currentIst = Now;
            
            var timeSpan = currentIst - istTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            
            return istTime.ToString("dd/MM/yyyy");
        }
    }
}