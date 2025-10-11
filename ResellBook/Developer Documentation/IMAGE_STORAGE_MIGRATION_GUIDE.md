# 📦 Future-Proof Image Storage Migration Guide
### From Local File Storage to Azure Blob Storage

> **Target:** Migrate from wwwroot file storage to Azure Blob Storage  
> **Timeline:** 2-4 hours implementation + testing  
> **Future-Proof:** Scalable for next 2+ years  

---

## 📋 **Table of Contents**

1. [Why Migrate to Blob Storage?](#1-why-migrate-to-blob-storage)
2. [Current Implementation Analysis](#2-current-implementation-analysis)
3. [Azure Blob Storage Setup](#3-azure-blob-storage-setup)
4. [Code Migration Steps](#4-code-migration-steps)
5. [PowerShell Migration Scripts](#5-powershell-migration-scripts)
6. [Testing & Validation](#6-testing--validation)
7. [Rollback Strategy](#7-rollback-strategy)
8. [Performance Optimization](#8-performance-optimization)
9. [Cost Management](#9-cost-management)
10. [Future Enhancements](#10-future-enhancements)

---

## **1. Why Migrate to Blob Storage?**

### 🚫 **Current Limitations (File Storage)**
- **Scalability:** Limited by server disk space
- **Performance:** Slower file I/O operations
- **Backup:** Manual backup processes required
- **CDN:** No automatic global distribution
- **Costs:** Higher server storage costs
- **Reliability:** Single point of failure

### ✅ **Blob Storage Benefits**
- **Unlimited Scale:** Petabytes of storage
- **Global CDN:** Automatic worldwide distribution
- **Cost-Effective:** Pay only for what you use
- **High Availability:** 99.9% uptime SLA
- **Automatic Backup:** Built-in redundancy
- **Security:** Advanced encryption & access controls
- **Integration:** Works seamlessly with Azure services

### 💰 **Cost Comparison** (Estimated)
```
Current (App Service Files):
- 10GB storage: ~$15/month
- Bandwidth: ~$10/month
- Total: ~$25/month

Blob Storage + CDN:
- 10GB storage: ~$2/month
- Bandwidth (CDN): ~$5/month
- Total: ~$7/month (70% savings!)
```

---

## **2. Current Implementation Analysis**

### 📁 **Current File Structure**
```
wwwroot/
├── Images/
│   ├── Books/
│   │   ├── book-id-1/
│   │   │   ├── image1.jpg
│   │   │   ├── image2.jpg
│   │   └── book-id-2/
│   └── ProfilePictures/
│       ├── user-id-1.jpg
│       └── user-id-2.jpg
├── css/
├── js/
└── lib/
```

### 🔍 **Code Analysis**
Let me analyze the current file handling code:

**Current File Controller Pattern:**
```csharp
[HttpPost("upload")]
public async Task<IActionResult> UploadFile(IFormFile file, string bookId)
{
    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Books", bookId);
    Directory.CreateDirectory(uploadsFolder);
    
    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    var filePath = Path.Combine(uploadsFolder, fileName);
    
    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);
    
    return Ok(new { fileName, url = $"/Images/Books/{bookId}/{fileName}" });
}
```

**Issues with Current Approach:**
- Hard-coded file paths
- No CDN integration
- Limited to single server
- Manual backup required

---

## **3. Azure Blob Storage Setup**

### 🏗️ **Step 1: Create Storage Account**

```powershell
# Set variables
$resourceGroup = "ResellBook-RG"
$storageAccount = "resellbookstorage$(Get-Random -Maximum 9999)"
$location = "East US"
$containerName = "images"

# Create storage account
az storage account create `
    --name $storageAccount `
    --resource-group $resourceGroup `
    --location $location `
    --sku Standard_LRS `
    --kind StorageV2 `
    --access-tier Hot `
    --https-only true

# Get storage account key
$storageKey = az storage account keys list `
    --resource-group $resourceGroup `
    --account-name $storageAccount `
    --query '[0].value' `
    --output tsv

# Create blob container
az storage container create `
    --name $containerName `
    --account-name $storageAccount `
    --account-key $storageKey `
    --public-access blob

Write-Host "Storage Account: $storageAccount"
Write-Host "Storage Key: $storageKey"
Write-Host "Container: $containerName"
```

### 🌐 **Step 2: Set Up CDN (Optional but Recommended)**

```powershell
# Create CDN profile
$cdnProfile = "$storageAccount-cdn"
$cdnEndpoint = "$storageAccount-endpoint"

az cdn profile create `
    --name $cdnProfile `
    --resource-group $resourceGroup `
    --sku Standard_Microsoft

# Create CDN endpoint
az cdn endpoint create `
    --name $cdnEndpoint `
    --profile-name $cdnProfile `
    --resource-group $resourceGroup `
    --origin "$storageAccount.blob.core.windows.net" `
    --origin-host-header "$storageAccount.blob.core.windows.net"

# Get CDN endpoint URL
$cdnUrl = az cdn endpoint show `
    --name $cdnEndpoint `
    --profile-name $cdnProfile `
    --resource-group $resourceGroup `
    --query hostName `
    --output tsv

Write-Host "CDN URL: https://$cdnUrl"
```

### ⚙️ **Step 3: Configure CORS (Important!)**

```powershell
# Enable CORS for web applications
az storage cors add `
    --services b `
    --methods GET POST PUT DELETE OPTIONS `
    --origins "*" `
    --allowed-headers "*" `
    --exposed-headers "*" `
    --max-age 3600 `
    --account-name $storageAccount `
    --account-key $storageKey
```

---

## **4. Code Migration Steps**

### 📦 **Step 1: Install Required NuGet Packages**

```xml
<!-- Add to ResellBook.csproj -->
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Azure.Storage.Blobs.Batch" Version="12.16.0" />
```

```powershell
# Install packages
dotnet add package Azure.Storage.Blobs
dotnet add package Azure.Storage.Blobs.Batch
```

### 🔧 **Step 2: Create Blob Storage Service**

Create `Services/BlobStorageService.cs`:

```csharp
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ResellBook.Utils;

namespace ResellBook.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName, string fileName);
        Task<bool> DeleteFileAsync(string containerName, string fileName);
        Task<Stream> GetFileAsync(string containerName, string fileName);
        Task<List<string>> ListFilesAsync(string containerName, string prefix = "");
        string GetFileUrl(string containerName, string fileName);
        Task<bool> FileExistsAsync(string containerName, string fileName);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _cdnBaseUrl;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;
            var connectionString = configuration.GetConnectionString("BlobStorage");
            _cdnBaseUrl = configuration.GetValue<string>("CDN:BaseUrl") ?? "";
            
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName, string fileName)
        {
            try
            {
                SimpleLogger.LogNormal("BlobStorageService", "UploadFileAsync", 
                    $"Uploading file: {fileName} to container: {containerName}", "System");

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(fileName);

                // Set content type based on file extension
                var contentType = GetContentType(Path.GetExtension(fileName).ToLowerInvariant());
                
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                    Metadata = new Dictionary<string, string>
                    {
                        ["UploadedAt"] = DateTime.UtcNow.ToString("O"),
                        ["OriginalFileName"] = file.FileName,
                        ["FileSize"] = file.Length.ToString()
                    }
                };

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, uploadOptions);

                var fileUrl = GetFileUrl(containerName, fileName);
                SimpleLogger.LogNormal("BlobStorageService", "UploadFileAsync", 
                    $"File uploaded successfully: {fileUrl}", "System");

                return fileUrl;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("BlobStorageService", "UploadFileAsync", 
                    "File upload failed", ex, "System");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                var response = await blobClient.DeleteIfExistsAsync();
                
                SimpleLogger.LogNormal("BlobStorageService", "DeleteFileAsync", 
                    $"File deletion result: {response.Value} for {fileName}", "System");
                
                return response.Value;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("BlobStorageService", "DeleteFileAsync", 
                    "File deletion failed", ex, "System");
                return false;
            }
        }

        public async Task<Stream> GetFileAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                var response = await blobClient.DownloadStreamingAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("BlobStorageService", "GetFileAsync", 
                    "File download failed", ex, "System");
                throw;
            }
        }

        public async Task<List<string>> ListFilesAsync(string containerName, string prefix = "")
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobs = new List<string>();

                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    blobs.Add(blobItem.Name);
                }

                return blobs;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogCritical("BlobStorageService", "ListFilesAsync", 
                    "File listing failed", ex, "System");
                return new List<string>();
            }
        }

        public string GetFileUrl(string containerName, string fileName)
        {
            if (!string.IsNullOrEmpty(_cdnBaseUrl))
            {
                return $"{_cdnBaseUrl.TrimEnd('/')}/{containerName}/{fileName}";
            }
            
            return $"https://{_blobServiceClient.AccountName}.blob.core.windows.net/{containerName}/{fileName}";
        }

        public async Task<bool> FileExistsAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch
            {
                return false;
            }
        }

        private static string GetContentType(string fileExtension)
        {
            return fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}
```

### 🔄 **Step 3: Update File Controllers**

Update `Controllers/FileController.cs`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<FileController> _logger;

    public FileController(IBlobStorageService blobStorageService, ILogger<FileController> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    [HttpPost("upload/book/{bookId}")]
    public async Task<IActionResult> UploadBookImage(string bookId, IFormFile file)
    {
        try
        {
            SimpleLogger.LogNormal("FileController", "UploadBookImage", 
                $"Uploading image for book: {bookId}", "System");

            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            if (!IsValidImageFile(file))
                return BadRequest("Invalid file type. Only images are allowed.");

            if (file.Length > 10 * 1024 * 1024) // 10MB limit
                return BadRequest("File size exceeds 10MB limit");

            var fileName = $"books/{bookId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fileUrl = await _blobStorageService.UploadFileAsync(file, "images", fileName);

            return Ok(new
            {
                Success = true,
                FileName = fileName,
                Url = fileUrl,
                Size = file.Length,
                ContentType = file.ContentType
            });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("FileController", "UploadBookImage", 
                "Image upload failed", ex, "System");
            return StatusCode(500, "File upload failed");
        }
    }

    [HttpPost("upload/profile/{userId}")]
    public async Task<IActionResult> UploadProfilePicture(string userId, IFormFile file)
    {
        try
        {
            SimpleLogger.LogNormal("FileController", "UploadProfilePicture", 
                $"Uploading profile picture for user: {userId}", "System");

            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            if (!IsValidImageFile(file))
                return BadRequest("Invalid file type. Only images are allowed.");

            if (file.Length > 5 * 1024 * 1024) // 5MB limit for profiles
                return BadRequest("File size exceeds 5MB limit");

            // Delete existing profile picture if exists
            var existingFiles = await _blobStorageService.ListFilesAsync("images", $"profiles/{userId}/");
            foreach (var existingFile in existingFiles)
            {
                await _blobStorageService.DeleteFileAsync("images", existingFile);
            }

            var fileName = $"profiles/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fileUrl = await _blobStorageService.UploadFileAsync(file, "images", fileName);

            return Ok(new
            {
                Success = true,
                FileName = fileName,
                Url = fileUrl,
                Size = file.Length,
                ContentType = file.ContentType
            });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("FileController", "UploadProfilePicture", 
                "Profile picture upload failed", ex, "System");
            return StatusCode(500, "File upload failed");
        }
    }

    [HttpDelete("{containerName}/{*fileName}")]
    public async Task<IActionResult> DeleteFile(string containerName, string fileName)
    {
        try
        {
            var deleted = await _blobStorageService.DeleteFileAsync(containerName, fileName);
            
            if (deleted)
            {
                return Ok(new { Success = true, Message = "File deleted successfully" });
            }
            
            return NotFound("File not found");
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("FileController", "DeleteFile", 
                "File deletion failed", ex, "System");
            return StatusCode(500, "File deletion failed");
        }
    }

    [HttpGet("list/{containerName}")]
    public async Task<IActionResult> ListFiles(string containerName, string? prefix = null)
    {
        try
        {
            var files = await _blobStorageService.ListFilesAsync(containerName, prefix ?? "");
            
            var fileList = files.Select(f => new
            {
                FileName = f,
                Url = _blobStorageService.GetFileUrl(containerName, f)
            });

            return Ok(new { Success = true, Files = fileList });
        }
        catch (Exception ex)
        {
            SimpleLogger.LogCritical("FileController", "ListFiles", 
                "File listing failed", ex, "System");
            return StatusCode(500, "Failed to list files");
        }
    }

    private static bool IsValidImageFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        var allowedContentTypes = new[] 
        { 
            "image/jpeg", "image/png", "image/gif", 
            "image/webp", "image/bmp" 
        };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension) && 
               allowedContentTypes.Contains(file.ContentType);
    }
}
```

### ⚙️ **Step 4: Update Configuration**

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-connection-string",
    "BlobStorage": "your-blob-storage-connection-string"
  },
  "CDN": {
    "BaseUrl": "https://your-cdn-endpoint.azureedge.net"
  },
  "BlobStorage": {
    "ContainerName": "images",
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"]
  }
}
```

### 🔌 **Step 5: Register Services**

Update `Program.cs`:

```csharp
// Add this after builder.Services.AddDbContext
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Optional: Add health checks for blob storage
builder.Services.AddHealthChecks()
    .AddAzureBlobStorage(
        builder.Configuration.GetConnectionString("BlobStorage")!,
        containerName: "images");
```

---

## **5. PowerShell Migration Scripts**

### 📥 **Step 1: Migration Script (Existing Files to Blob)**

Create `migrate-to-blob-storage.ps1`:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$true)]
    [string]$WebAppName,
    
    [Parameter(Mandatory=$false)]
    [string]$LocalImagesPath = "wwwroot\Images"
)

Write-Host "🚀 Starting Blob Storage Migration..." -ForegroundColor Green

try {
    # Get storage account key
    $storageKey = az storage account keys list `
        --resource-group $ResourceGroupName `
        --account-name $StorageAccountName `
        --query '[0].value' `
        --output tsv

    if (!$storageKey) {
        throw "Failed to get storage account key"
    }

    Write-Host "✅ Storage account key retrieved" -ForegroundColor Green

    # Create containers
    $containers = @("images")
    foreach ($container in $containers) {
        az storage container create `
            --name $container `
            --account-name $StorageAccountName `
            --account-key $storageKey `
            --public-access blob `
            --output none
        
        Write-Host "✅ Container '$container' created/verified" -ForegroundColor Green
    }

    # Migrate existing files if they exist
    if (Test-Path $LocalImagesPath) {
        Write-Host "📦 Migrating existing files..." -ForegroundColor Yellow
        
        $imageFiles = Get-ChildItem -Path $LocalImagesPath -Recurse -File
        $totalFiles = $imageFiles.Count
        $currentFile = 0

        foreach ($file in $imageFiles) {
            $currentFile++
            $relativePath = $file.FullName.Substring((Get-Item $LocalImagesPath).FullName.Length + 1)
            $blobName = $relativePath.Replace('\', '/')
            
            Write-Progress -Activity "Migrating Files" -Status "Processing $($file.Name)" -PercentComplete (($currentFile / $totalFiles) * 100)
            
            try {
                az storage blob upload `
                    --account-name $StorageAccountName `
                    --account-key $storageKey `
                    --container-name "images" `
                    --name $blobName `
                    --file $file.FullName `
                    --overwrite `
                    --output none

                Write-Host "  ✅ Migrated: $blobName" -ForegroundColor Green
            }
            catch {
                Write-Host "  ❌ Failed to migrate: $blobName - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        
        Write-Progress -Activity "Migrating Files" -Completed
        Write-Host "📦 File migration completed: $currentFile files processed" -ForegroundColor Green
    }
    else {
        Write-Host "ℹ️ No local images folder found - skipping file migration" -ForegroundColor Yellow
    }

    # Update app settings
    Write-Host "⚙️ Updating application settings..." -ForegroundColor Yellow
    
    $blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=$StorageAccountName;AccountKey=$storageKey;EndpointSuffix=core.windows.net"
    
    az webapp config connection-string set `
        --name $WebAppName `
        --resource-group $ResourceGroupName `
        --connection-string-type Custom `
        --settings BlobStorage="$blobConnectionString"

    # Optional: Set CDN URL if you have one
    # az webapp config appsettings set `
    #     --name $WebAppName `
    #     --resource-group $ResourceGroupName `
    #     --settings "CDN__BaseUrl=https://your-cdn-endpoint.azureedge.net"

    Write-Host "✅ Application settings updated" -ForegroundColor Green

    # Restart web app
    Write-Host "🔄 Restarting web app..." -ForegroundColor Yellow
    az webapp restart --name $WebAppName --resource-group $ResourceGroupName
    
    Write-Host "🎉 Blob Storage migration completed successfully!" -ForegroundColor Green
    Write-Host "📊 Storage Account: $StorageAccountName" -ForegroundColor Cyan
    Write-Host "🌐 Blob URL: https://$StorageAccountName.blob.core.windows.net/images/" -ForegroundColor Cyan

    # Optional: Create backup of old files
    if (Test-Path $LocalImagesPath) {
        $backupPath = "Images_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        Copy-Item -Path $LocalImagesPath -Destination $backupPath -Recurse
        Write-Host "💾 Backup created at: $backupPath" -ForegroundColor Cyan
        Write-Host "⚠️ You can delete the backup after verifying the migration" -ForegroundColor Yellow
    }

} catch {
    Write-Host "❌ Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

### 🔄 **Step 2: Rollback Script**

Create `rollback-blob-migration.ps1`:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$WebAppName,
    
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "Images_Backup*"
)

Write-Host "🔄 Starting Blob Storage Rollback..." -ForegroundColor Yellow

try {
    # Find backup folder
    $backupFolder = Get-ChildItem -Path . -Directory -Name $BackupPath | Sort-Object CreationTime -Descending | Select-Object -First 1
    
    if (!$backupFolder) {
        Write-Host "❌ No backup folder found matching pattern: $BackupPath" -ForegroundColor Red
        exit 1
    }

    Write-Host "📁 Found backup folder: $backupFolder" -ForegroundColor Green

    # Restore files
    $restorePath = "wwwroot\Images"
    if (Test-Path $restorePath) {
        Remove-Item -Path $restorePath -Recurse -Force
    }
    
    Copy-Item -Path $backupFolder -Destination $restorePath -Recurse
    Write-Host "✅ Files restored to: $restorePath" -ForegroundColor Green

    # Remove blob storage connection string
    az webapp config connection-string delete `
        --name $WebAppName `
        --resource-group $ResourceGroupName `
        --setting-names "BlobStorage"

    # Remove CDN settings
    az webapp config appsettings delete `
        --name $WebAppName `
        --resource-group $ResourceGroupName `
        --setting-names "CDN__BaseUrl"

    Write-Host "✅ Application settings reverted" -ForegroundColor Green

    # Restart web app
    az webapp restart --name $WebAppName --resource-group $ResourceGroupName
    
    Write-Host "🎉 Rollback completed successfully!" -ForegroundColor Green
    Write-Host "⚠️ Remember to update your code to use local file storage" -ForegroundColor Yellow

} catch {
    Write-Host "❌ Rollback failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

---

## **6. Testing & Validation**

### 🧪 **Step 1: Automated Testing Script**

Create `test-blob-migration.ps1`:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$BaseUrl
)

Write-Host "🧪 Testing Blob Storage Implementation..." -ForegroundColor Green

$testResults = @()

try {
    # Test 1: Health Check
    Write-Host "📊 Test 1: Health Check..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl/health" -Method GET -TimeoutSec 30
        $testResults += @{Test="Health Check"; Status="✅ PASS"; Details="Status: $($response.StatusCode)"}
    }
    catch {
        $testResults += @{Test="Health Check"; Status="❌ FAIL"; Details=$_.Exception.Message}
    }

    # Test 2: File Upload
    Write-Host "📤 Test 2: File Upload..." -ForegroundColor Yellow
    try {
        # Create a test image
        $testImagePath = "test-image.jpg"
        [System.IO.File]::WriteAllBytes($testImagePath, [System.Convert]::FromBase64String("/9j/4AAQSkZJRgABAQEAAAAAAAD//gA7Q1JFQVR"))
        
        $form = @{
            file = Get-Item -Path $testImagePath
        }
        
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/File/upload/book/test-book-123" -Method Post -Form $form
        
        if ($response.Success) {
            $testResults += @{Test="File Upload"; Status="✅ PASS"; Details="URL: $($response.Url)"}
            $uploadedUrl = $response.Url
        } else {
            $testResults += @{Test="File Upload"; Status="❌ FAIL"; Details="Upload failed"}
        }
        
        Remove-Item -Path $testImagePath -ErrorAction SilentlyContinue
    }
    catch {
        $testResults += @{Test="File Upload"; Status="❌ FAIL"; Details=$_.Exception.Message}
        Remove-Item -Path $testImagePath -ErrorAction SilentlyContinue
    }

    # Test 3: File Access
    if ($uploadedUrl) {
        Write-Host "🔍 Test 3: File Access..." -ForegroundColor Yellow
        try {
            $response = Invoke-WebRequest -Uri $uploadedUrl -Method HEAD -TimeoutSec 30
            $testResults += @{Test="File Access"; Status="✅ PASS"; Details="Status: $($response.StatusCode)"}
        }
        catch {
            $testResults += @{Test="File Access"; Status="❌ FAIL"; Details=$_.Exception.Message}
        }
    }

    # Test 4: File Listing
    Write-Host "📋 Test 4: File Listing..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/File/list/images" -Method GET
        if ($response.Success) {
            $testResults += @{Test="File Listing"; Status="✅ PASS"; Details="Files found: $($response.Files.Count)"}
        } else {
            $testResults += @{Test="File Listing"; Status="❌ FAIL"; Details="Listing failed"}
        }
    }
    catch {
        $testResults += @{Test="File Listing"; Status="❌ FAIL"; Details=$_.Exception.Message}
    }

    # Display Results
    Write-Host "`n📊 Test Results:" -ForegroundColor Cyan
    Write-Host "=================" -ForegroundColor Cyan
    
    foreach ($result in $testResults) {
        Write-Host "$($result.Status) $($result.Test): $($result.Details)" -ForegroundColor $(if ($result.Status -like "*PASS*") { "Green" } else { "Red" })
    }
    
    $passCount = ($testResults | Where-Object { $_.Status -like "*PASS*" }).Count
    $totalCount = $testResults.Count
    
    Write-Host "`n🎯 Overall Result: $passCount/$totalCount tests passed" -ForegroundColor $(if ($passCount -eq $totalCount) { "Green" } else { "Yellow" })
    
    if ($passCount -eq $totalCount) {
        Write-Host "🎉 All tests passed! Blob storage migration is successful." -ForegroundColor Green
    } else {
        Write-Host "⚠️ Some tests failed. Please check the implementation." -ForegroundColor Yellow
    }

} catch {
    Write-Host "❌ Testing failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

### 📊 **Step 2: Performance Testing**

```powershell
# Performance test script
$baseUrl = "https://your-app.azurewebsites.net"
$testDuration = 60  # seconds
$concurrentUsers = 5

Write-Host "⚡ Starting Performance Test..." -ForegroundColor Green
Write-Host "Duration: $testDuration seconds" -ForegroundColor Yellow
Write-Host "Concurrent Users: $concurrentUsers" -ForegroundColor Yellow

$jobs = @()
$startTime = Get-Date

for ($i = 1; $i -le $concurrentUsers; $i++) {
    $job = Start-Job -ScriptBlock {
        param($url, $duration, $userId)
        
        $endTime = (Get-Date).AddSeconds($duration)
        $requests = 0
        $errors = 0
        
        while ((Get-Date) -lt $endTime) {
            try {
                Invoke-WebRequest -Uri "$url/api/File/list/images" -Method GET -TimeoutSec 10 | Out-Null
                $requests++
            }
            catch {
                $errors++
            }
            Start-Sleep -Milliseconds 100
        }
        
        return @{
            UserId = $userId
            Requests = $requests
            Errors = $errors
        }
    } -ArgumentList $baseUrl, $testDuration, $i
    
    $jobs += $job
}

Write-Host "⏳ Running test for $testDuration seconds..." -ForegroundColor Yellow
$jobs | Wait-Job | Out-Null

$results = $jobs | Receive-Job
$jobs | Remove-Job

$totalRequests = ($results | Measure-Object -Property Requests -Sum).Sum
$totalErrors = ($results | Measure-Object -Property Errors -Sum).Sum
$actualDuration = ((Get-Date) - $startTime).TotalSeconds

Write-Host "`n📊 Performance Test Results:" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host "Duration: $([math]::Round($actualDuration, 2)) seconds" -ForegroundColor White
Write-Host "Total Requests: $totalRequests" -ForegroundColor White
Write-Host "Total Errors: $totalErrors" -ForegroundColor $(if ($totalErrors -eq 0) { "Green" } else { "Red" })
Write-Host "Requests/Second: $([math]::Round($totalRequests / $actualDuration, 2))" -ForegroundColor White
Write-Host "Error Rate: $([math]::Round(($totalErrors / $totalRequests) * 100, 2))%" -ForegroundColor $(if ($totalErrors -eq 0) { "Green" } else { "Red" })
```

---

## **7. Rollback Strategy**

### 🔄 **Rollback Plan**

1. **Keep Original Code Branch**
   ```powershell
   git checkout -b blob-storage-migration
   # Make all changes in this branch
   # Keep main branch as rollback option
   ```

2. **Gradual Migration Approach**
   ```csharp
   // Feature flag approach in appsettings.json
   {
     "FeatureFlags": {
       "UseBlobStorage": false  // Set to true after testing
     }
   }
   ```

3. **Hybrid Implementation** (Recommended for initial rollout)
   ```csharp
   public async Task<string> UploadFileAsync(IFormFile file, string path)
   {
       var useBlobStorage = _configuration.GetValue<bool>("FeatureFlags:UseBlobStorage");
       
       if (useBlobStorage)
       {
           return await _blobStorageService.UploadFileAsync(file, "images", path);
       }
       else
       {
           return await UploadToLocalStorage(file, path);  // Original method
       }
   }
   ```

### 🚨 **Emergency Rollback Checklist**

- [ ] **Step 1:** Set `FeatureFlags:UseBlobStorage = false` in app settings
- [ ] **Step 2:** Restart application
- [ ] **Step 3:** Verify local file access working
- [ ] **Step 4:** Run rollback PowerShell script if needed
- [ ] **Step 5:** Update DNS/CDN settings if applicable

---

## **8. Performance Optimization**

### ⚡ **CDN Configuration**

```powershell
# Advanced CDN settings for optimal performance
az cdn endpoint rule add `
    --name $cdnEndpoint `
    --profile-name $cdnProfile `
    --resource-group $resourceGroup `
    --order 1 `
    --rule-name "CacheImages" `
    --match-variable "UrlFileExtension" `
    --operator "Equal" `
    --match-values "jpg,jpeg,png,gif,webp" `
    --action-name "CacheExpiration" `
    --cache-behavior "Override" `
    --cache-duration "30.00:00:00"  # 30 days

# Enable compression
az cdn endpoint update `
    --name $cdnEndpoint `
    --profile-name $cdnProfile `
    --resource-group $resourceGroup `
    --enable-compression true `
    --content-types-to-compress "image/jpeg,image/png,image/gif,image/webp"
```

### 🎯 **Image Optimization Middleware**

```csharp
public class ImageOptimizationMiddleware
{
    private readonly RequestDelegate _next;

    public ImageOptimizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add image optimization headers
        if (context.Request.Path.StartsWithSegments("/images"))
        {
            context.Response.Headers.Add("Cache-Control", "public, max-age=2592000"); // 30 days
            context.Response.Headers.Add("Expires", DateTime.UtcNow.AddDays(30).ToString("R"));
        }

        await _next(context);
    }
}

// Register in Program.cs
app.UseMiddleware<ImageOptimizationMiddleware>();
```

---

## **9. Cost Management**

### 💰 **Storage Tier Strategy**

```powershell
# Set up lifecycle management to automatically move old files to cheaper tiers
az storage account management-policy create `
    --account-name $storageAccount `
    --resource-group $resourceGroup `
    --policy '{
        "rules": [
            {
                "name": "moveToIA",
                "type": "Lifecycle",
                "definition": {
                    "filters": {
                        "blobTypes": ["blockBlob"]
                    },
                    "actions": {
                        "baseBlob": {
                            "tierToCool": {"daysAfterModificationGreaterThan": 30},
                            "tierToArchive": {"daysAfterModificationGreaterThan": 90}
                        }
                    }
                }
            }
        ]
    }'
```

### 📊 **Cost Monitoring Script**

```powershell
# Monitor storage costs
$usage = az storage account show-usage --account-name $storageAccount --resource-group $resourceGroup --query usedCapacity --output tsv
$usageGB = [math]::Round($usage / 1024 / 1024 / 1024, 2)
$estimatedCost = $usageGB * 0.0184  # Approximate cost per GB per month

Write-Host "📊 Storage Usage: $usageGB GB" -ForegroundColor Cyan
Write-Host "💰 Estimated Monthly Cost: $estimatedCost USD" -ForegroundColor Yellow
```

---

## **10. Future Enhancements**

### 🚀 **Planned Improvements (Next 2 Years)**

1. **Image Processing Pipeline**
   ```csharp
   // Auto-resize and optimize images on upload
   public async Task<string> ProcessAndUploadImageAsync(IFormFile file, ImageProcessingOptions options)
   {
       using var image = Image.Load(file.OpenReadStream());
       
       // Resize if needed
       if (options.MaxWidth > 0 && image.Width > options.MaxWidth)
       {
           image.Mutate(x => x.Resize(options.MaxWidth, 0));
       }
       
       // Convert to WebP for better compression
       using var output = new MemoryStream();
       await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = options.Quality });
       
       return await _blobStorageService.UploadStreamAsync(output, containerName, fileName);
   }
   ```

2. **AI-Powered Image Analysis**
   ```csharp
   // Integrate Azure Computer Vision for automatic tagging
   public async Task<ImageAnalysisResult> AnalyzeImageAsync(string imageUrl)
   {
       var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
       {
           Endpoint = endpoint
       };
       
       var analysis = await client.AnalyzeImageAsync(imageUrl, new List<VisualFeatureTypes?>
       {
           VisualFeatureTypes.Tags,
           VisualFeatureTypes.Description,
           VisualFeatureTypes.Objects
       });
       
       return new ImageAnalysisResult
       {
           Tags = analysis.Tags.Select(t => t.Name).ToList(),
           Description = analysis.Description.Captions.FirstOrDefault()?.Text,
           Objects = analysis.Objects.Select(o => o.ObjectProperty).ToList()
       };
   }
   ```

3. **Multi-Region CDN Setup**
   ```powershell
   # Global CDN with multiple endpoints
   $regions = @("East US", "West Europe", "Southeast Asia")
   foreach ($region in $regions) {
       $regionalEndpoint = "$storageAccount-$region".Replace(" ", "").ToLower()
       az cdn endpoint create `
           --name $regionalEndpoint `
           --profile-name $cdnProfile `
           --resource-group $resourceGroup `
           --origin "$storageAccount.blob.core.windows.net" `
           --origin-host-header "$storageAccount.blob.core.windows.net"
   }
   ```

4. **Advanced Security Features**
   ```csharp
   // Implement SAS tokens for secure file access
   public string GenerateSecureFileUrl(string containerName, string fileName, TimeSpan expiry)
   {
       var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(fileName);
       
       if (blobClient.CanGenerateSasUri)
       {
           var sasBuilder = new BlobSasBuilder
           {
               BlobContainerName = containerName,
               BlobName = fileName,
               Resource = "b",
               ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
           };
           
           sasBuilder.SetPermissions(BlobSasPermissions.Read);
           
           return blobClient.GenerateSasUri(sasBuilder).ToString();
       }
       
       return blobClient.Uri.ToString();
   }
   ```

### 📱 **Mobile Optimization**

1. **Responsive Image Serving**
   ```csharp
   public string GetOptimizedImageUrl(string originalUrl, int? width = null, int? height = null, string? format = null)
   {
       var queryParams = new List<string>();
       
       if (width.HasValue) queryParams.Add($"w={width}");
       if (height.HasValue) queryParams.Add($"h={height}");
       if (!string.IsNullOrEmpty(format)) queryParams.Add($"f={format}");
       
       var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
       return $"{originalUrl}{query}";
   }
   ```

2. **Progressive Image Loading**
   ```javascript
   // Client-side progressive loading
   function loadImageProgressively(imgElement, thumbnailUrl, fullUrl) {
       // Load low-quality thumbnail first
       imgElement.src = thumbnailUrl;
       
       // Load full quality in background
       const fullImage = new Image();
       fullImage.onload = function() {
           imgElement.src = fullUrl;
           imgElement.classList.add('loaded');
       };
       fullImage.src = fullUrl;
   }
   ```

---

## **🎯 Migration Checklist**

### **Pre-Migration**
- [ ] Review current file storage usage and costs
- [ ] Create Azure storage account and CDN
- [ ] Test blob storage service in development
- [ ] Backup existing files
- [ ] Update application configuration

### **Migration Day**
- [ ] Run migration script during low traffic
- [ ] Deploy updated application code
- [ ] Test all file operations
- [ ] Verify CDN propagation
- [ ] Update DNS if needed

### **Post-Migration**
- [ ] Monitor application performance
- [ ] Check storage costs and usage
- [ ] Set up automated backups
- [ ] Configure lifecycle policies
- [ ] Document new procedures

### **Optimization (Week 2+)**
- [ ] Enable image compression
- [ ] Set up advanced caching rules
- [ ] Implement image resizing
- [ ] Add performance monitoring
- [ ] Plan future enhancements

---

**🎉 This guide provides a complete roadmap for migrating to Azure Blob Storage with future-proof architecture that will serve your needs for the next 2+ years!**

*Remember: Always test in a development environment first, and have a rollback plan ready before migrating production data.*