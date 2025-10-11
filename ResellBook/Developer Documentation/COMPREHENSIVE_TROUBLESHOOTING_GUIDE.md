# 🛠️ Comprehensive Troubleshooting Guide
### Solutions for Common Deployment and Runtime Issues

> **For Beginners:** Step-by-step solutions with detailed explanations  
> **Updated:** October 2025  
> **Covers:** Azure deployment, SQL connectivity, runtime errors, performance issues  

---

## 📋 **Table of Contents**

1. [Pre-Deployment Issues](#1-pre-deployment-issues)
2. [Azure Resource Creation Problems](#2-azure-resource-creation-problems)
3. [Database Connection Issues](#3-database-connection-issues)
4. [Application Deployment Failures](#4-application-deployment-failures)
5. [Runtime Error Solutions](#5-runtime-error-solutions)
6. [Performance Issues](#6-performance-issues)
7. [API Endpoint Problems](#7-api-endpoint-problems)
8. [File Upload & Storage Issues](#8-file-upload--storage-issues)
9. [Authentication & JWT Issues](#9-authentication--jwt-issues)
10. [Monitoring & Diagnostics](#10-monitoring--diagnostics)

---

## **1. Pre-Deployment Issues**

### 🚨 **Issue: `dotnet build` Fails**

#### **Error Messages:**
```
MSB3644: The reference assemblies for .NETCoreApp,Version=v8.0 were not found
MSB4236: The SDK 'Microsoft.NET.Sdk' specified could not be found
```

#### **🔧 Solutions:**

**Solution 1: Install/Update .NET SDK**
```powershell
# Check current version
dotnet --version

# Download latest .NET 8 SDK from: https://dotnet.microsoft.com/download
# After installation, verify:
dotnet --version
# Should show 8.x.x or higher

# Clear NuGet cache if needed
dotnet nuget locals all --clear
```

**Solution 2: Fix Project File Issues**
```xml
<!-- Ensure ResellBook.csproj has correct TargetFramework -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Solution 3: Clean and Rebuild**
```powershell
# Clean everything
dotnet clean
Remove-Item -Path "bin","obj" -Recurse -Force -ErrorAction SilentlyContinue

# Restore and rebuild
dotnet restore --verbosity detailed
dotnet build --verbosity detailed
```

---

### 🚨 **Issue: Git Clone Fails**

#### **Error Messages:**
```
fatal: repository 'https://github.com/user/repo.git' not found
Permission denied (publickey)
```

#### **🔧 Solutions:**

**Solution 1: Repository Access**
```powershell
# Check if repository exists and you have access
# If private repository, ensure you're logged into correct GitHub account

# Alternative: Download ZIP instead
# Go to GitHub repository → Code → Download ZIP
```

**Solution 2: Authentication Issues**
```powershell
# Configure Git credentials
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# For private repositories, use personal access token
# GitHub → Settings → Developer settings → Personal access tokens
```

---

## **2. Azure Resource Creation Problems**

### 🚨 **Issue: `az login` Fails**

#### **Error Messages:**
```
Please ensure you have network connection. Error detail: HTTPSConnectionPool
The command failed with an unexpected error. Here is the traceback:
```

#### **🔧 Solutions:**

**Solution 1: Network/Firewall Issues**
```powershell
# Test Azure connectivity
Test-NetConnection -ComputerName login.microsoftonline.com -Port 443

# If behind corporate firewall, contact IT admin
# Or use device code flow:
az login --use-device-code
```

**Solution 2: Clear Azure CLI Cache**
```powershell
# Clear cached credentials
az account clear

# Remove Azure directory (if needed)
Remove-Item -Path "$env:USERPROFILE\.azure" -Recurse -Force -ErrorAction SilentlyContinue

# Reinstall Azure CLI if persistent issues
```

---

### 🚨 **Issue: Resource Creation Fails**

#### **Error Messages:**
```
The subscription is not registered to use namespace 'Microsoft.Web'
Location 'East US' is not available for resource type 'Microsoft.Web/sites'
The requested resource type is not available in region 'East US'
```

#### **🔧 Solutions:**

**Solution 1: Register Resource Providers**
```powershell
# Register required providers
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.Storage

# Check registration status
az provider show --namespace Microsoft.Web --query registrationState
```

**Solution 2: Choose Available Regions**
```powershell
# List available regions for App Service
az appservice list-locations --sku B1 --linux-workers-enabled false

# Alternative regions to try:
# - "Central US"
# - "East US 2"  
# - "West US 2"
# - "North Europe"
# - "West Europe"
```

**Solution 3: Check Subscription Limits**
```powershell
# Check current usage and limits
az vm list-usage --location "East US" --output table
az network list-usages --location "East US" --output table

# If limits exceeded, request quota increase in Azure portal
```

---

### 🚨 **Issue: Unique Name Conflicts**

#### **Error Messages:**
```
Website with given name already exists
Storage account 'name' is not available
The specified DNS name is not available
```

#### **🔧 Solutions:**

**Solution: Generate Unique Names**
```powershell
# Use timestamp for uniqueness
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$appName = "ResellBook$timestamp"
$storageAccount = "resellbook$timestamp"  # must be lowercase, no special chars

# Or use random number
$random = Get-Random -Maximum 9999
$appName = "ResellBook$random"

# Check availability before creating
az webapp list --query "[?name=='$appName']" --output table
az storage account check-name --name $storageAccount
```

---

## **3. Database Connection Issues**

### 🚨 **Issue: Cannot Connect to SQL Server**

#### **Error Messages:**
```
Cannot open server 'xxx' requested by the login
A network-related or instance-specific error occurred
Login failed for user 'xxx'
```

#### **🔧 Solutions:**

**Solution 1: Firewall Configuration**
```powershell
# Get your current public IP
$myIP = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
Write-Host "Your IP: $myIP"

# Add firewall rule
az sql server firewall-rule create `
    --server $sqlServerName `
    --resource-group $resourceGroup `
    --name "MyIP" `
    --start-ip-address $myIP `
    --end-ip-address $myIP

# Allow Azure services
az sql server firewall-rule create `
    --server $sqlServerName `
    --resource-group $resourceGroup `
    --name "AllowAzureServices" `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0
```

**Solution 2: Test Connectivity**
```powershell
# Test if port 1433 is accessible
Test-NetConnection -ComputerName "$sqlServerName.database.windows.net" -Port 1433

# Test with sqlcmd (if installed)
sqlcmd -S "$sqlServerName.database.windows.net" -d $databaseName -U $sqlAdminUser -P $sqlAdminPassword -Q "SELECT 1"

# Alternative: Use Azure Cloud Shell for testing
# https://shell.azure.com
```

**Solution 3: Connection String Issues**
```powershell
# Verify connection string format
$connectionString = "Server=$sqlServerName.database.windows.net;Database=$databaseName;User Id=$sqlAdminUser;Password=$sqlAdminPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Test connection string
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $connection.Open()
    Write-Host "✅ Connection successful!" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "❌ Connection failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

---

### 🚨 **Issue: Entity Framework Migration Fails**

#### **Error Messages:**
```
Unable to create an object of type 'AppDbContext'
No database provider has been configured for this DbContext
The migration '20231001_InitialCreate' has already been applied to the database
```

#### **🔧 Solutions:**

**Solution 1: Configuration Issues**
```powershell
# Check appsettings.json has correct connection string
Get-Content appsettings.json | ConvertFrom-Json | Select-Object -ExpandProperty ConnectionStrings

# Update connection string if needed
$appsettings = Get-Content appsettings.json | ConvertFrom-Json
$appsettings.ConnectionStrings.DefaultConnection = $connectionString
$appsettings | ConvertTo-Json -Depth 10 | Set-Content appsettings.json
```

**Solution 2: Migration Reset**
```powershell
# If migrations are corrupted, reset them
Remove-Item -Path "Migrations" -Recurse -Force -ErrorAction SilentlyContinue

# Recreate initial migration
dotnet ef migrations add InitialCreate
dotnet ef database update

# If database exists but migrations table is corrupted
dotnet ef database drop --force
dotnet ef database update
```

**Solution 3: Design-Time DbContext Factory** (Create if missing)
```csharp
// Create Data/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ResellBook.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
```

---

## **4. Application Deployment Failures**

### 🚨 **Issue: ZIP Deployment Fails**

#### **Error Messages:**
```
Failed to deploy web package to App Service
Package deployment using ZIP file is not supported
Error: Request timeout
```

#### **🔧 Solutions:**

**Solution 1: Check Package Size**
```powershell
# Check deployment package size
$zipFile = "deployment.zip"
if (Test-Path $zipFile) {
    $size = (Get-Item $zipFile).Length / 1MB
    Write-Host "Package size: $([math]::Round($size, 2)) MB"
    
    if ($size -gt 50) {
        Write-Host "⚠️ Package is too large. Consider removing unnecessary files." -ForegroundColor Yellow
    }
}
```

**Solution 2: Alternative Deployment Method**
```powershell
# Use Azure DevOps or GitHub Actions for large deployments
# Or deploy from source:

# Deploy from local Git repository
az webapp deployment source config-local-git `
    --name $appName `
    --resource-group $resourceGroup

# Get Git URL and deploy
$gitUrl = az webapp deployment list-publishing-credentials `
    --name $appName `
    --resource-group $resourceGroup `
    --query scmUri `
    --output tsv

git remote add azure $gitUrl
git push azure main:master
```

**Solution 3: Kudu Deployment**
```powershell
# Access Kudu console
$scmUrl = "https://$appName.scm.azurewebsites.net"
Write-Host "Kudu Console: $scmUrl"

# Manual file upload through Kudu
# Navigate to: https://yourapp.scm.azurewebsites.net/newui
# Go to CMD → site/wwwroot → upload files
```

---

### 🚨 **Issue: App Won't Start After Deployment**

#### **Error Messages:**
```
Application Error: The application cannot be started
HTTP Error 500.30 - ANCM In-Process Start Failure
```

#### **🔧 Solutions:**

**Solution 1: Check Application Logs**
```powershell
# Enable detailed error messages
az webapp config set `
    --name $appName `
    --resource-group $resourceGroup `
    --detailed-error-messages-enabled true

# Stream logs
az webapp log tail --name $appName --resource-group $resourceGroup

# Download logs for analysis
az webapp log download --name $appName --resource-group $resourceGroup
```

**Solution 2: Environment-Specific Issues**
```powershell
# Check environment variables
az webapp config appsettings list --name $appName --resource-group $resourceGroup

# Common missing settings:
az webapp config appsettings set `
    --name $appName `
    --resource-group $resourceGroup `
    --settings `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "WEBSITE_TIME_ZONE=UTC"
```

**Solution 3: Application Insights Diagnostics**
```powershell
# Enable Application Insights for better diagnostics
$appInsightsName = "$appName-insights"

az monitor app-insights component create `
    --app $appInsightsName `
    --location $location `
    --resource-group $resourceGroup

$instrumentationKey = az monitor app-insights component show `
    --app $appInsightsName `
    --resource-group $resourceGroup `
    --query instrumentationKey `
    --output tsv

az webapp config appsettings set `
    --name $appName `
    --resource-group $resourceGroup `
    --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey"
```

---

## **5. Runtime Error Solutions**

### 🚨 **Issue: HTTP 500 Internal Server Error**

#### **Common Causes & Solutions:**

**Cause 1: Database Connection Issues**
```csharp
// Add connection retry logic in Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});
```

**Cause 2: Missing Configuration**
```powershell
# Check and fix missing app settings
$requiredSettings = @(
    "JWT_SECRET",
    "JWT_ISSUER", 
    "JWT_AUDIENCE"
)

foreach ($setting in $requiredSettings) {
    $value = az webapp config appsettings list `
        --name $appName `
        --resource-group $resourceGroup `
        --query "[?name=='$setting'].value" `
        --output tsv
    
    if (-not $value) {
        Write-Host "❌ Missing setting: $setting" -ForegroundColor Red
    } else {
        Write-Host "✅ Found setting: $setting" -ForegroundColor Green
    }
}
```

**Cause 3: Unhandled Exceptions**
```csharp
// Add global exception handler in Program.cs
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        
        if (exceptionFeature?.Error is Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "An internal server error occurred",
                requestId = Activity.Current?.Id ?? context.TraceIdentifier
            }));
        }
    });
});
```

---

### 🚨 **Issue: Memory or Performance Issues**

#### **Error Messages:**
```
OutOfMemoryException
Request timeout
High CPU usage
```

#### **🔧 Solutions:**

**Solution 1: Scale Up App Service**
```powershell
# Check current plan
az appservice plan show --name "$appName-plan" --resource-group $resourceGroup

# Scale up to higher tier
az appservice plan update `
    --name "$appName-plan" `
    --resource-group $resourceGroup `
    --sku P1V2  # or P2V2, P3V2 for more resources

# Scale out (add instances)
az appservice plan update `
    --name "$appName-plan" `
    --resource-group $resourceGroup `
    --number-of-workers 2
```

**Solution 2: Database Performance Optimization**
```sql
-- Check database performance
SELECT 
    query_stats.query_hash,
    SUM(query_stats.total_worker_time) / SUM(query_stats.execution_count) AS avg_cpu_time,
    MIN(query_stats.statement_text) AS sample_statement_text
FROM 
    (SELECT QS.*,
     SUBSTRING(ST.text, (QS.statement_start_offset/2) + 1,
     ((CASE statement_end_offset 
       WHEN -1 THEN DATALENGTH(ST.text)
       ELSE QS.statement_end_offset END 
           - QS.statement_start_offset)/2) + 1) AS statement_text
     FROM sys.dm_exec_query_stats AS QS
     CROSS APPLY sys.dm_exec_sql_text(QS.sql_handle) AS ST) AS query_stats
GROUP BY query_stats.query_hash
ORDER BY avg_cpu_time DESC;

-- Add indexes for slow queries (example)
CREATE INDEX IX_Books_UserId ON Books (UserId);
CREATE INDEX IX_Books_CreatedAt ON Books (CreatedAt);
```

**Solution 3: Implement Caching**
```csharp
// Add memory caching in Program.cs
builder.Services.AddMemoryCache();

// Use in controllers
public class BooksController : ControllerBase
{
    private readonly IMemoryCache _cache;
    
    public BooksController(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        const string cacheKey = "all_books";
        
        if (!_cache.TryGetValue(cacheKey, out List<Book> books))
        {
            books = await _context.Books.ToListAsync();
            _cache.Set(cacheKey, books, TimeSpan.FromMinutes(5));
        }
        
        return Ok(books);
    }
}
```

---

## **6. Performance Issues**

### 🚨 **Issue: Slow API Responses**

#### **🔧 Diagnostic Steps:**

```powershell
# Test API response times
$baseUrl = "https://$appName.azurewebsites.net"
$endpoints = @(
    "/api/health",
    "/api/Books",
    "/api/Auth/login",
    "/WeatherForecast"
)

foreach ($endpoint in $endpoints) {
    $startTime = Get-Date
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl$endpoint" -Method GET -TimeoutSec 30
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalMilliseconds
        
        Write-Host "✅ $endpoint : $([math]::Round($duration, 0))ms (Status: $($response.StatusCode))" -ForegroundColor Green
    }
    catch {
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalMilliseconds
        Write-Host "❌ $endpoint : $([math]::Round($duration, 0))ms (Error: $($_.Exception.Message))" -ForegroundColor Red
    }
}
```

**Solution 1: Database Query Optimization**
```csharp
// Use async methods and avoid N+1 queries
[HttpGet]
public async Task<IActionResult> GetBooksWithUsers()
{
    // Bad: N+1 query problem
    // var books = await _context.Books.ToListAsync();
    // foreach (var book in books)
    // {
    //     book.User = await _context.Users.FindAsync(book.UserId);
    // }
    
    // Good: Use Include for eager loading
    var books = await _context.Books
        .Include(b => b.User)
        .Take(50)  // Limit results
        .AsNoTracking()  // Read-only queries
        .ToListAsync();
        
    return Ok(books);
}
```

**Solution 2: Connection Pooling**
```csharp
// Configure connection pooling in Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
    });
}, ServiceLifetime.Scoped);

// Add connection pool settings
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(connectionString), poolSize: 128);
```

---

## **7. API Endpoint Problems**

### 🚨 **Issue: 404 Not Found for API Endpoints**

#### **Error Messages:**
```
404 Not Found
The resource you are looking for has been removed
```

#### **🔧 Solutions:**

**Solution 1: Check Route Configuration**
```csharp
// Ensure controllers have proper routing
[ApiController]
[Route("api/[controller]")]  // This creates route: api/Books
public class BooksController : ControllerBase
{
    [HttpGet]  // GET api/Books
    public async Task<IActionResult> GetBooks() { }
    
    [HttpGet("{id}")]  // GET api/Books/123
    public async Task<IActionResult> GetBook(int id) { }
}
```

**Solution 2: Test Routes Programmatically**
```powershell
# Create route testing script
$baseUrl = "https://$appName.azurewebsites.net"
$routes = @(
    @{ Method = "GET"; Path = "/api/Books"; Description = "Get all books" },
    @{ Method = "POST"; Path = "/api/Auth/login"; Description = "User login" },
    @{ Method = "GET"; Path = "/api/UserSearch/GetAllSearches"; Description = "Get searches" },
    @{ Method = "GET"; Path = "/WeatherForecast"; Description = "Weather test" }
)

Write-Host "🧪 Testing API Routes..." -ForegroundColor Green
foreach ($route in $routes) {
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl$($route.Path)" -Method $route.Method -TimeoutSec 10
        Write-Host "✅ $($route.Method) $($route.Path) - Status: $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host "🔐 $($route.Method) $($route.Path) - Requires Authentication" -ForegroundColor Yellow
        }
        else {
            Write-Host "❌ $($route.Method) $($route.Path) - Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
```

**Solution 3: Enable Swagger in Production** (for debugging)
```csharp
// In Program.cs, enable Swagger even in production
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Access Swagger at: https://yourapp.azurewebsites.net/swagger
```

---

### 🚨 **Issue: CORS Issues**

#### **Error Messages:**
```
Access to fetch at 'api/books' has been blocked by CORS policy
No 'Access-Control-Allow-Origin' header is present
```

#### **🔧 Solutions:**

**Solution: Configure CORS Properly**
```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "https://yourfrontendapp.com",
            "https://localhost:3000",  // React dev server
            "http://localhost:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
    
    // For development only - remove in production
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Apply CORS policy
app.UseCors("AllowSpecificOrigins");  // Use specific policy in production
// app.UseCors("AllowAll");  // Only for development
```

---

## **8. File Upload & Storage Issues**

### 🚨 **Issue: File Upload Fails**

#### **Error Messages:**
```
Request Entity Too Large
The request filtering module is configured to deny a request that exceeds the request content length
Maximum request length exceeded
```

#### **🔧 Solutions:**

**Solution 1: Configure File Upload Limits**
```csharp
// In Program.cs
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
});
```

**Solution 2: Add web.config for IIS**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="52428800" /> <!-- 50MB -->
      </requestFiltering>
    </security>
  </system.webServer>
</configuration>
```

**Solution 3: Test File Upload**
```powershell
# Create test script for file uploads
function Test-FileUpload {
    param(
        [string]$BaseUrl,
        [string]$FilePath
    )
    
    if (-not (Test-Path $FilePath)) {
        # Create a test file
        $testContent = "This is a test file content"
        [System.IO.File]::WriteAllText($FilePath, $testContent)
    }
    
    try {
        $form = @{
            file = Get-Item -Path $FilePath
        }
        
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/File/upload/book/test-123" -Method Post -Form $form
        Write-Host "✅ File upload successful: $($response.Url)" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ File upload failed: $($_.Exception.Message)" -ForegroundColor Red
    }
    finally {
        Remove-Item -Path $FilePath -ErrorAction SilentlyContinue
    }
}

Test-FileUpload -BaseUrl "https://$appName.azurewebsites.net" -FilePath "test.txt"
```

---

### 🚨 **Issue: Images Not Loading from wwwroot**

#### **🔧 Solutions:**

**Solution 1: Configure Static Files**
```csharp
// In Program.cs
app.UseStaticFiles(); // This should be before UseRouting()

// For custom directories
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Images")),
    RequestPath = "/Images"
});
```

**Solution 2: Check File Permissions in Azure**
```powershell
# Access Kudu console to check files
$scmUrl = "https://$appName.scm.azurewebsites.net"
Write-Host "Check files at: $scmUrl/newui"
Write-Host "Navigate to: site/wwwroot/ to verify uploaded files exist"
```

---

## **9. Authentication & JWT Issues**

### 🚨 **Issue: JWT Authentication Fails**

#### **Error Messages:**
```
401 Unauthorized
Bearer token not found
Invalid token
```

#### **🔧 Solutions:**

**Solution 1: Debug JWT Configuration**
```csharp
// Create JWT debug endpoint (development only)
[HttpGet("debug-jwt")]
public IActionResult DebugJwt()
{
    var jwtSecret = _configuration["Jwt:Secret"];
    var jwtIssuer = _configuration["Jwt:Issuer"];
    var jwtAudience = _configuration["Jwt:Audience"];
    
    return Ok(new
    {
        SecretConfigured = !string.IsNullOrEmpty(jwtSecret),
        SecretLength = jwtSecret?.Length ?? 0,
        Issuer = jwtIssuer,
        Audience = jwtAudience,
        CurrentTime = DateTime.UtcNow
    });
}
```

**Solution 2: Test JWT Token Generation**
```powershell
# Test JWT endpoint
$loginData = @{
    Email = "test@example.com"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method Post -Body $loginData -ContentType "application/json"
    Write-Host "✅ JWT Token generated successfully" -ForegroundColor Green
    Write-Host "Token: $($response.token.Substring(0, 50))..." -ForegroundColor Cyan
}
catch {
    Write-Host "❌ JWT generation failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

**Solution 3: Validate JWT Settings**
```powershell
# Check JWT configuration in Azure
$jwtSettings = az webapp config appsettings list `
    --name $appName `
    --resource-group $resourceGroup `
    --query "[?starts_with(name, 'JWT_')]" `
    --output table

Write-Host "JWT Configuration:"
$jwtSettings | Format-Table
```

---

## **10. Monitoring & Diagnostics**

### 📊 **Comprehensive Health Check Script**

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName
)

Write-Host "🔍 Comprehensive Application Health Check" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

$baseUrl = "https://$AppName.azurewebsites.net"
$healthReport = @()

# 1. Basic Connectivity
Write-Host "`n1. Testing Basic Connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $baseUrl -Method HEAD -TimeoutSec 30
    $healthReport += @{Category="Connectivity"; Test="Basic HTTP"; Status="✅ PASS"; Details="Status: $($response.StatusCode)"}
}
catch {
    $healthReport += @{Category="Connectivity"; Test="Basic HTTP"; Status="❌ FAIL"; Details=$_.Exception.Message}
}

# 2. Health Endpoint
Write-Host "2. Testing Health Endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET -TimeoutSec 30
    $healthReport += @{Category="Health"; Test="Health Endpoint"; Status="✅ PASS"; Details="Status: $($response.StatusCode)"}
}
catch {
    $healthReport += @{Category="Health"; Test="Health Endpoint"; Status="❌ FAIL"; Details=$_.Exception.Message}
}

# 3. API Endpoints
Write-Host "3. Testing API Endpoints..." -ForegroundColor Yellow
$apiTests = @(
    @{Path="/api/Books"; Method="GET"; Name="Books API"},
    @{Path="/WeatherForecast"; Method="GET"; Name="Weather API"},
    @{Path="/api/UserSearch/GetAllSearches"; Method="GET"; Name="UserSearch API"}
)

foreach ($test in $apiTests) {
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl$($test.Path)" -Method $test.Method -TimeoutSec 10
        $healthReport += @{Category="API"; Test=$test.Name; Status="✅ PASS"; Details="Status: $($response.StatusCode)"}
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            $healthReport += @{Category="API"; Test=$test.Name; Status="🔐 AUTH"; Details="Requires authentication"}
        }
        else {
            $healthReport += @{Category="API"; Test=$test.Name; Status="❌ FAIL"; Details=$_.Exception.Message}
        }
    }
}

# 4. Database Connectivity
Write-Host "4. Checking Database..." -ForegroundColor Yellow
try {
    $sqlServer = az sql server list --resource-group $ResourceGroupName --query "[0].name" --output tsv
    if ($sqlServer) {
        $healthReport += @{Category="Database"; Test="SQL Server"; Status="✅ PASS"; Details="Server: $sqlServer"}
        
        # Check firewall rules
        $firewallRules = az sql server firewall-rule list --server $sqlServer --resource-group $ResourceGroupName --query "length(@)"
        $healthReport += @{Category="Database"; Test="Firewall Rules"; Status="ℹ️ INFO"; Details="Rules count: $firewallRules"}
    }
}
catch {
    $healthReport += @{Category="Database"; Test="SQL Server"; Status="❌ FAIL"; Details=$_.Exception.Message}
}

# 5. Application Settings
Write-Host "5. Checking Application Settings..." -ForegroundColor Yellow
try {
    $appSettings = az webapp config appsettings list --name $AppName --resource-group $ResourceGroupName --output json | ConvertFrom-Json
    $criticalSettings = @("JWT_SECRET", "JWT_ISSUER", "JWT_AUDIENCE")
    
    foreach ($setting in $criticalSettings) {
        $found = $appSettings | Where-Object { $_.name -eq $setting }
        if ($found) {
            $healthReport += @{Category="Config"; Test=$setting; Status="✅ PASS"; Details="Configured"}
        }
        else {
            $healthReport += @{Category="Config"; Test=$setting; Status="❌ FAIL"; Details="Missing"}
        }
    }
}
catch {
    $healthReport += @{Category="Config"; Test="App Settings"; Status="❌ FAIL"; Details=$_.Exception.Message}
}

# 6. Application Logs
Write-Host "6. Checking Recent Logs..." -ForegroundColor Yellow
try {
    $recentLogs = az webapp log tail --name $AppName --resource-group $ResourceGroupName --provider application --timeout 5 2>&1
    if ($recentLogs -match "error|exception|fail") {
        $healthReport += @{Category="Logs"; Test="Error Check"; Status="⚠️ WARN"; Details="Errors found in logs"}
    }
    else {
        $healthReport += @{Category="Logs"; Test="Error Check"; Status="✅ PASS"; Details="No recent errors"}
    }
}
catch {
    $healthReport += @{Category="Logs"; Test="Log Access"; Status="❌ FAIL"; Details=$_.Exception.Message}
}

# Display Results
Write-Host "`n📊 Health Check Results:" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan

$groupedResults = $healthReport | Group-Object -Property Category
foreach ($group in $groupedResults) {
    Write-Host "`n$($group.Name):" -ForegroundColor White
    foreach ($result in $group.Group) {
        $color = switch -Wildcard ($result.Status) {
            "*PASS*" { "Green" }
            "*FAIL*" { "Red" }
            "*WARN*" { "Yellow" }
            "*AUTH*" { "Cyan" }
            default { "White" }
        }
        Write-Host "  $($result.Status) $($result.Test): $($result.Details)" -ForegroundColor $color
    }
}

# Summary
$passCount = ($healthReport | Where-Object { $_.Status -like "*PASS*" }).Count
$failCount = ($healthReport | Where-Object { $_.Status -like "*FAIL*" }).Count
$totalCount = $healthReport.Count

Write-Host "`n🎯 Summary: $passCount passed, $failCount failed, $($totalCount - $passCount - $failCount) other" -ForegroundColor Cyan
Write-Host "Application URL: $baseUrl" -ForegroundColor Cyan
Write-Host "Kudu Console: https://$AppName.scm.azurewebsites.net" -ForegroundColor Cyan

if ($failCount -eq 0) {
    Write-Host "`n🎉 Application is healthy!" -ForegroundColor Green
}
else {
    Write-Host "`n⚠️ Application has issues that need attention." -ForegroundColor Yellow
}
```

### 📱 **Usage:**
```powershell
# Save script as health-check.ps1 and run:
.\health-check.ps1 -AppName "YourAppName" -ResourceGroupName "ResellBook-RG"
```

---

## **🚨 Emergency Response Checklist**

### **When Production App is Down:**

1. **Immediate Actions (First 5 minutes)**
   - [ ] Check Azure Service Health: https://status.azure.com
   - [ ] Verify app is running: `az webapp show --name $appName --resource-group $resourceGroup --query state`
   - [ ] Restart app: `az webapp restart --name $appName --resource-group $resourceGroup`
   - [ ] Check basic connectivity: `curl -I https://yourapp.azurewebsites.net`

2. **Investigation (Next 10 minutes)**
   - [ ] Check application logs: `az webapp log tail --name $appName --resource-group $resourceGroup`
   - [ ] Verify database connectivity
   - [ ] Check recent deployments
   - [ ] Review application settings

3. **Resolution (Next 20 minutes)**
   - [ ] Rollback recent deployment if needed
   - [ ] Fix configuration issues
   - [ ] Scale up resources if performance issue
   - [ ] Apply emergency patches

4. **Post-Incident (After resolution)**
   - [ ] Document the incident
   - [ ] Update monitoring alerts
   - [ ] Implement preventive measures
   - [ ] Conduct post-mortem review

---

**🎯 Remember: Most issues can be resolved by checking logs first, then configuration, then connectivity. Always have a rollback plan ready!**

*This troubleshooting guide covers 90% of common issues. For specific errors not covered here, check Azure documentation or contact support.*