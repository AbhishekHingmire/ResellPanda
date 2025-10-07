# 📋 Logs Controller API Documentation

## Overview
The Logs Controller provides comprehensive logging functionality for the ResellBook application, enabling real-time monitoring, debugging, and system health tracking through a simple file-based logging system.

## Base URL
```
https://resellbook20250929183655.azurewebsites.net/api/Logs
```
**Local Development:**
```
http://localhost:5125/api/Logs
```

---

## 🔍 **API Endpoints**

### 1. Get Normal Logs
**Endpoint:** `GET /api/Logs/GetNormalLogs`

**Description:** Retrieves normal operation logs from the system.

**Parameters:**
- `maxLines` (query, optional): Maximum number of log entries to retrieve (default: 100)

**Example Request:**
```http
GET /api/Logs/GetNormalLogs?maxLines=50
Content-Type: application/json
```

**Example Response:**
```json
{
  "success": true,
  "data": [
    "05-10-2025 09:47:55 PM | BooksController | ViewAll | Retrieved 4 books | UserId: 02b994fb-9c5d-4c38-9ce7-8b91c3e9e298",
    "05-10-2025 09:47:55 PM | BooksController | ViewAll | ViewAll request for userId: 02b994fb-9c5d-4c38-9ce7-8b91c3e9e298 | UserId: 02b994fb-9c5d-4c38-9ce7-8b91c3e9e298",
    "05-10-2025 09:39:33 PM | AuthController | Login | Login successful for email: test@example.com | UserId: 12345678-1234-5678-9012-123456789012"
  ],
  "count": 3,
  "message": "Retrieved 3 normal log entries"
}
```

**Response Fields:**
- `success`: Boolean indicating operation success
- `data`: Array of log entries (most recent first)
- `count`: Number of log entries returned
- `message`: Descriptive message about the operation

---

### 2. Get Critical Logs
**Endpoint:** `GET /api/Logs/GetCriticalLogs`

**Description:** Retrieves critical error logs and exceptions from the system.

**Parameters:**
- `maxLines` (query, optional): Maximum number of log entries to retrieve (default: 100)

**Example Request:**
```http
GET /api/Logs/GetCriticalLogs?maxLines=10
Content-Type: application/json
```

**Example Response:**
```json
{
  "success": true,
  "data": [
    "05-10-2025 09:43:00 PM | BooksController | ViewAll | ViewAll method failed | UserId: 02b994fb-9c5d-4c38-9ce7-8b91c3e9e298 | Error: An item with the same key has already been added. Key: e518e64f-ba1b-4035-92ba-e3a6fb45b7c7 | Stack:    at System.Collections.Generic.Dictionary`2.TryInsert(TKey key, TValue value, InsertionBehavior behavior)",
    "05-10-2025 09:35:22 PM | AuthController | Login | Login failed for email: invalid@example.com | UserId: N/A | Error: Invalid credentials"
  ],
  "count": 2,
  "message": "Retrieved 2 critical log entries"
}
```

---

### 3. Get Logs Summary
**Endpoint:** `GET /api/Logs/GetLogsSummary`

**Description:** Provides an overview of the logging system including log counts and system status.

**Example Request:**
```http
GET /api/Logs/GetLogsSummary
Content-Type: application/json
```

**Example Response:**
```json
{
  "success": true,
  "data": {
    "normalLogsCount": 125,
    "criticalLogsCount": 8,
    "totalLogsCount": 133,
    "lastNormalLog": "05-10-2025 09:47:55 PM | BooksController | ViewAll | Retrieved 4 books",
    "lastCriticalLog": "05-10-2025 09:43:00 PM | BooksController | ViewAll | ViewAll method failed",
    "logFilesStatus": {
      "normalLogFile": "AppLogs/normal.txt - 15.2 KB",
      "criticalLogFile": "AppLogs/critical.txt - 3.8 KB"
    }
  },
  "message": "Logs summary retrieved successfully"
}
```

---

### 4. Test Logging
**Endpoint:** `GET /api/Logs/TestLogging`

**Description:** Creates test log entries to verify the logging system is working correctly.

**Example Request:**
```http
GET /api/Logs/TestLogging
Content-Type: application/json
```

**Example Response:**
```json
{
  "success": true,
  "message": "Test logs created successfully",
  "data": {
    "normalLogCreated": true,
    "criticalLogCreated": true,
    "timestamp": "05-10-2025 09:39:33 PM"
  }
}
```

---

### 5. Clear All Logs
**Endpoint:** `DELETE /api/Logs/ClearAllLogs`

**Description:** Clears all log files (both normal and critical logs). Use with caution in production.

**Example Request:**
```http
DELETE /api/Logs/ClearAllLogs
Content-Type: application/json
```

**Example Response:**
```json
{
  "success": true,
  "message": "All logs cleared successfully",
  "data": {
    "normalLogsCleared": true,
    "criticalLogsCleared": true,
    "timestamp": "05-10-2025 09:50:00 PM"
  }
}
```

---

## 🛠 **Technical Implementation**

### Logging System Architecture
- **Static Utility**: `Utils/SimpleLogger.cs` provides static logging methods
- **File Storage**: Logs stored in `AppLogs/` directory
- **Separation**: Normal and critical logs stored in separate files
- **Time Zone**: All timestamps in Indian Standard Time (IST)
- **No Dependencies**: Direct file I/O without complex dependency injection

### Log File Structure
```
AppLogs/
├── normal.txt     # Normal operation logs
└── critical.txt   # Critical errors and exceptions
```

### Log Entry Format
```
DD-MM-YYYY HH:MM:SS AM/PM | Controller | Method | Message | UserId: GUID | [Error: Details] | [Stack: StackTrace]
```

**Example:**
```
05-10-2025 09:47:55 PM | BooksController | ViewAll | Retrieved 4 books | UserId: 02b994fb-9c5d-4c38-9ce7-8b91c3e9e298
```

---

## 🚀 **Integration Examples**

### Android/Kotlin Integration
```kotlin
// Retrofit Interface
interface LogsApi {
    @GET("api/Logs/GetNormalLogs")
    suspend fun getNormalLogs(
        @Header("Authorization") token: String,
        @Query("maxLines") maxLines: Int = 50
    ): Response<LogsResponse>
    
    @GET("api/Logs/GetCriticalLogs")
    suspend fun getCriticalLogs(
        @Header("Authorization") token: String,
        @Query("maxLines") maxLines: Int = 50
    ): Response<LogsResponse>
    
    @GET("api/Logs/GetLogsSummary")
    suspend fun getLogsSummary(
        @Header("Authorization") token: String
    ): Response<LogsSummaryResponse>
}

// Data Classes
data class LogsResponse(
    val success: Boolean,
    val data: List<String>,
    val count: Int,
    val message: String
)

data class LogsSummaryResponse(
    val success: Boolean,
    val data: LogsSummaryData,
    val message: String
)

data class LogsSummaryData(
    val normalLogsCount: Int,
    val criticalLogsCount: Int,
    val totalLogsCount: Int,
    val lastNormalLog: String?,
    val lastCriticalLog: String?,
    val logFilesStatus: Map<String, String>
)

// Usage Example
class LogsRepository(private val logsApi: LogsApi) {
    suspend fun getNormalLogs(token: String, maxLines: Int): Result<List<String>> {
        return try {
            val response = logsApi.getNormalLogs("Bearer $token", maxLines)
            if (response.isSuccessful && response.body()?.success == true) {
                Result.success(response.body()!!.data)
            } else {
                Result.failure(Exception("Failed to fetch normal logs"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun getCriticalLogs(token: String, maxLines: Int): Result<List<String>> {
        return try {
            val response = logsApi.getCriticalLogs("Bearer $token", maxLines)
            if (response.isSuccessful && response.body()?.success == true) {
                Result.success(response.body()!!.data)
            } else {
                Result.failure(Exception("Failed to fetch critical logs"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
```

### C# Client Integration
```csharp
public class LogsService
{
    private readonly HttpClient _httpClient;
    
    public LogsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<LogsResponse> GetNormalLogsAsync(string token, int maxLines = 50)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
        var response = await _httpClient.GetAsync($"api/Logs/GetNormalLogs?maxLines={maxLines}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LogsResponse>(content);
    }
    
    public async Task<LogsResponse> GetCriticalLogsAsync(string token, int maxLines = 50)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
        var response = await _httpClient.GetAsync($"api/Logs/GetCriticalLogs?maxLines={maxLines}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LogsResponse>(content);
    }
}
```

---

## 🔒 **Security Considerations**

### Production Recommendations
1. **Access Control**: Implement role-based access (admin-only)
2. **Rate Limiting**: Add rate limiting to prevent log API abuse
3. **Log Rotation**: Implement automatic log rotation for large files
4. **Sanitization**: Ensure no sensitive data is logged
5. **Monitoring**: Set up alerts for high critical log volumes

### Sample Security Implementation
```csharp
[Authorize(Roles = "Admin")] // Add role-based authorization
[ApiController]
public class LogsController : ControllerBase
{
    // Implementation with security
}
```

---

## 📊 **Monitoring & Alerts**

### Health Check Integration
- Monitor log file sizes
- Alert on critical log spikes
- Track logging system availability
- Monitor disk space for log storage

### Metrics to Track
- Normal logs per hour
- Critical logs per hour  
- Log file growth rate
- API response times
- Error rates in logging endpoints

---

## 🐛 **Troubleshooting**

### Common Issues

**1. Empty Log Response**
```json
{"success": true, "data": [], "count": 0, "message": "No logs found"}
```
**Solution:** Check if logging is enabled and log files exist in AppLogs directory.

**2. File Access Errors**
**Solution:** Ensure application has write permissions to AppLogs directory.

**3. Large Response Times**
**Solution:** Use smaller `maxLines` values for better performance.

### Log File Location
- **Development**: `c:\Repos\ResellPanda\ResellBook\AppLogs\`
- **Production**: `wwwroot/AppLogs/` or configured path

---

## 📈 **Performance Considerations**

- **File Size Management**: Log files grow over time, implement rotation
- **Response Optimization**: Use pagination for large log sets  
- **Caching**: Consider caching recent logs for faster access
- **Background Processing**: For heavy log analysis, use background jobs

---

## 🔄 **Version History**

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 05-10-2025 | Initial implementation with basic logging endpoints |
| 1.0.1 | 05-10-2025 | Added comprehensive error handling and IST timestamps |
| 1.1.0 | 05-10-2025 | **PRODUCTION DEPLOYMENT** - Logging system deployed with bulletproof deployment process |

## 🚀 **Deployment Information**

**Current Status**: ✅ **DEPLOYED TO PRODUCTION**
- **Deployment Method**: Safe bulletproof deployment (`deploy.ps1`)
- **User Files**: ✅ Preserved during deployment (no image loss)
- **ViewAll Bug**: ✅ Fixed (was returning 500, now returns 200)
- **Logging System**: ✅ Fully operational in production

**Production URLs**:
- **Get Normal Logs**: `https://resellbook20250929183655.azurewebsites.net/api/Logs/GetNormalLogs`
- **Get Critical Logs**: `https://resellbook20250929183655.azurewebsites.net/api/Logs/GetCriticalLogs`
- **Logs Summary**: `https://resellbook20250929183655.azurewebsites.net/api/Logs/GetLogsSummary`
- **Test Logging**: `https://resellbook20250929183655.azurewebsites.net/api/Logs/TestLogging`

**For Future Deployments**: Only use `.\deploy.ps1` - all other deployment scripts have been removed for safety.

---

## 📞 **Support**

For technical support or questions regarding the Logs Controller:
- **Repository**: ResellPanda/ResellBook
- **Branch**: listing
- **Contact**: Development Team

---

*This documentation is part of the ResellBook API suite. Last updated: October 5, 2025*