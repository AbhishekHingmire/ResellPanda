# 🚀 Deploy.ps1 Script - Complete Technical Documentation

## 📋 Overview

The `deploy.ps1` script is a comprehensive PowerShell automation script designed for deploying the ResellBook ASP.NET Core application to Azure App Service. It implements a "bulletproof" deployment strategy that prioritizes file safety, especially for user-uploaded content in the `wwwroot` folder.

**Script Location:** `c:\Repos\ResellPanda\ResellBook\deploy.ps1`  
**Execution Context:** Must be run from the project root directory  
**Target Platform:** Azure App Service (Web Apps)  
**Deployment Method:** Azure CLI with ZIP deployment  

---

## 📁 File Locations & Operations

### **Execution Directory**
- **Script runs from:** `c:\Repos\ResellPanda\ResellBook\` (project root)
- **Working Directory:** Same as script location
- **Required files:** `ResellBook.csproj`, source code, `appsettings.json`

### **Build Source Files**
The script picks build files from the **current working directory** (project root):
```
c:\Repos\ResellPanda\ResellBook\
├── Controllers\          # API controllers
├── Models\              # Data models
├── Data\                # Database context
├── Services\            # Business logic
├── Helpers\             # Utility classes
├── ResellBook.csproj    # Project file
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── wwwroot\            # Static files (excluded from deployment)
```

### **Build Output Locations**

#### **Primary Build Output**
- **Command:** `dotnet publish -c Release -o publish`
- **Output Directory:** `c:\Repos\ResellPanda\ResellBook\publish\`
- **Contents:**
  ```
  publish\
  ├── appsettings.Development.json
  ├── appsettings.json
  ├── ResellBook.dll              # Main assembly
  ├── ResellBook.exe              # Executable
  ├── ResellBook.runtimeconfig.json
  ├── web.config                  # IIS configuration
  ├── wwwroot\                    # Static files (EXCLUDED from deployment)
  ├── runtimes\                   # Runtime dependencies
  └── Various .dll files          # Dependencies
  ```

#### **Deployment Package Creation**
- **Source:** `publish\` folder (excluding `wwwroot`)
- **Temporary Directory:** `temp-deploy-{timestamp}\`
- **Final Package:** `safe-deploy-{timestamp}.zip`
- **Excluded Files:** `wwwroot` folder and all contents

### **Azure Deployment Target**
- **Resource Group:** `$ResourceGroup` (default: "resell-panda-rg")
- **App Service:** `$AppName` (default: "ResellBook20250929183655")
- **Deployment Method:** ZIP deployment via Azure CLI
- **Target Path:** `site/wwwroot/` (but preserves existing `wwwroot` content)

---

## 🔄 Step-by-Step Process

### **Step 1: Azure Authentication & Verification**
```powershell
# Verify Azure CLI login
az account show
# Get FTP credentials for backup (if needed)
az webapp deployment list-publishing-profiles
```

**Purpose:** Ensures authenticated access to Azure resources  
**Files Accessed:** None (Azure API calls only)  
**Output:** Authentication confirmation

### **Step 2: Backup Creation (Safety Measure)**
```powershell
# Create timestamped backup folder
$backupFolder = "wwwroot-backup-$timestamp"
# Attempt FTP download of existing wwwroot
```

**Purpose:** Preserve user-uploaded images and files  
**Files Created:** `wwwroot-backup-{timestamp}\`  
**Backup Method:** FTP download (if credentials available)  
**Fallback:** Skip backup if FTP fails (deployment still proceeds)

### **Step 3: Application Build**
```powershell
# Clean previous publish directories to prevent nested folder issues
if (Test-Path "publish") {
    Write-Host "🧹 Cleaning project publish directory..." -ForegroundColor Yellow
    Remove-Item "publish" -Recurse -Force
}
if (Test-Path "bin\publish") {
    Write-Host "🧹 Cleaning bin publish directory..." -ForegroundColor Yellow
    Remove-Item "bin\publish" -Recurse -Force
}
# Build application
dotnet publish -c Release -o publish
```

**Source Files:** All project files in current directory  
**Build Configuration:** Release mode  
**Output Location:** `./publish/` (relative to script location)  
**Key Feature:** Automatic cleanup of both project and bin publish directories prevents nested folder issues

### **Step 4: Safe Deployment Package Creation**
```powershell
# Create temporary deployment directory
$tempDeploy = "temp-deploy-$timestamp"
# Copy all from publish EXCEPT wwwroot
Get-ChildItem "publish" -Exclude "wwwroot" | Copy-Item -Destination $tempDeploy -Recurse
# Create ZIP package
Compress-Archive -Path "$tempDeploy\*" -DestinationPath $deployZip
```

**Source:** `./publish/` (excluding `wwwroot`)  
**Temporary Location:** `./temp-deploy-{timestamp}/`  
**Final Package:** `./safe-deploy-{timestamp}.zip`  
**Size:** Typically 11-15 MB (varies by dependencies)

### **Step 5: Azure Deployment**
```powershell
# Deploy ZIP package
az webapp deployment source config-zip --resource-group $ResourceGroup --name $AppName --src $deployZip
```

**Deployment Method:** Azure App Service ZIP deployment  
**Target:** Replaces application files but preserves `wwwroot`  
**Duration:** 2-5 minutes depending on package size  
**Rollback:** Automatic if deployment fails

### **Step 6: Post-Deployment Testing**
```powershell
# Test basic connectivity
Invoke-WebRequest -Uri "https://$AppName.azurewebsites.net/api/Test/ping"
# Test critical endpoints
Invoke-WebRequest -Uri "https://$AppName.azurewebsites.net/api/Books/ViewAll"
```

**Tests Performed:**
- Application startup confirmation
- API endpoint availability
- Critical functionality verification

### **Step 7: Cleanup**
```powershell
# Remove temporary files
Remove-Item $tempDeploy -Recurse -Force
Remove-Item $deployZip -Force
# Preserve backup folder
```

**Cleanup Scope:** Temporary deployment files  
**Preserved:** Backup folders for safety reference

---

## 📂 Directory Structure Explanation

### **Bin Folder Structure**
The `bin\` folder contains build outputs from `dotnet build` commands:

```
bin\
├── Debug\
│   └── net8.0\           # Debug build output
│       ├── ResellBook.dll
│       ├── appsettings.json
│       └── ... (other files)
└── Release\
    └── net8.0\           # Release build output
        ├── ResellBook.dll
        ├── appsettings.json
        └── ... (other files)
```

### **Publish Folder Issues**
The script creates a `publish\` folder in the project root, but you may see:

```
bin\
├── publish\              # ❌ ACCIDENTAL - from old builds
│   └── publish\          # ❌ NESTED - multiple runs
│       └── publish\      # ❌ DEEPER NESTING
└── ...
```

**Why This Happens:**
- `dotnet publish -o publish` publishes **into** existing `publish` folder
- Without cleanup, folders nest: `publish/publish/publish/...`
- This causes the MSB3030 error: `publish\publish\publish\ResellBook.runtimeconfig.json`

**Prevention:**
- The script now includes: `if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }`
- Always clean publish directory before building

### **WWWRoot Handling**
- **Source:** `./wwwroot/` (user-uploaded images)
- **Deployment:** EXCLUDED from deployment package
- **Preservation:** Azure keeps existing `wwwroot` during ZIP deployment
- **Safety:** Backup created before deployment

---

## ⚙️ Configuration & Parameters

### **Default Parameters**
```powershell
param(
    [string]$ResourceGroup = "resell-panda-rg",
    [string]$AppName = "ResellBook20250929183655"
)
```

### **Environment Variables Used**
- Azure CLI authentication (from `az login`)
- Project configuration from `appsettings.json`

### **External Dependencies**
- **Azure CLI:** Must be installed and authenticated
- **.NET SDK:** For building the application
- **PowerShell:** For script execution

---

## 🛡️ Safety Features

### **File Preservation Strategy**
1. **Pre-deployment Backup:** Attempts FTP download of `wwwroot`
2. **Selective Deployment:** Excludes `wwwroot` from deployment package
3. **Incremental Updates:** Only replaces application files
4. **Rollback Ready:** Azure maintains previous version if deployment fails

### **Error Handling**
- **Build Failures:** Stops deployment, shows error details
- **Deployment Failures:** Provides specific error messages
- **Network Issues:** Retries with backoff strategy
- **Authentication Issues:** Clear guidance for `az login`

---

## 📊 Performance & Timing

### **Typical Execution Times**
- **Azure Authentication:** 2-5 seconds
- **Backup Creation:** 10-30 seconds (if FTP available)
- **Application Build:** 3-8 seconds
- **Package Creation:** 5-15 seconds
- **Azure Deployment:** 2-5 minutes
- **Testing:** 10-30 seconds
- **Total:** 4-9 minutes

### **Resource Usage**
- **Disk Space:** ~50MB temporary files (cleaned up)
- **Memory:** ~200MB during build
- **Network:** ~15MB upload for deployment package

---

## 🔧 Troubleshooting

### **Common Issues**

#### **Issue 1: MSB3030 Nested Publish Error**
```
Error MSB3030: Could not copy the file "publish\publish\publish\ResellBook.runtimeconfig.json"
```
**Cause:** Multiple `dotnet publish` runs without cleanup  
**Solution:** Delete existing `publish` folder before building  
**Prevention:** Script now includes automatic cleanup

#### **Issue 2: Azure Authentication Failed**
```
Please run 'az login' to setup account.
```
**Solution:** Run `az login` in terminal  
**Verification:** `az account show`

#### **Issue 3: Build Fails**
**Common Causes:**
- Missing dependencies: Run `dotnet restore`
- Compilation errors: Check code for syntax issues
- SDK version mismatch: Ensure .NET 8.0 SDK installed

#### **Issue 4: Deployment Timeout**
**Solutions:**
- Check Azure App Service plan (Free tier has limits)
- Verify package size (< 2GB for ZIP deployment)
- Check network connectivity

#### **Issue 5: Application Won't Start**
**Checks:**
- Verify `appsettings.json` configuration
- Check database connection strings
- Review Azure Application Insights logs

---

## 📋 Deployment Checklist

### **Pre-Deployment**
- [ ] Azure CLI installed and authenticated (`az login`)
- [ ] Project builds successfully (`dotnet build`)
- [ ] Database migrations applied (`dotnet ef database update`)
- [ ] `appsettings.json` configured for production

### **During Deployment**
- [ ] Script runs from project root directory
- [ ] No existing `publish` folder conflicts
- [ ] Azure resources exist and accessible
- [ ] Sufficient Azure quota and credits

### **Post-Deployment**
- [ ] Application responds to ping endpoint
- [ ] Critical APIs functional (ViewAll, authentication)
- [ ] User-uploaded files preserved in `wwwroot`
- [ ] Logs accessible via Azure portal

---

## 🔗 Related Files & Documentation

### **Configuration Files**
- `appsettings.json` - Application configuration
- `ResellBook.csproj` - Project build settings
- `Properties/launchSettings.json` - Development settings

### **Azure Resources**
- Resource Group: `resell-panda-rg`
- App Service: `ResellBook20250929183655`
- SQL Database: `resellbook-db`
- Storage Account: For file storage (if configured)

### **Documentation Links**
- [Azure App Service Deployment](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)
- [.NET Publish Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)

---

## 📝 Change Log

### **Version 2.1 - October 17, 2025**
- ✅ Added cleanup for `bin\publish` directory to prevent nested folder issues
- ✅ Enhanced documentation with detailed folder structure explanations
- ✅ Improved troubleshooting section for publish folder problems

### **Version 2 - October 13, 2025**
- ✅ Added automatic `publish` folder cleanup to prevent MSB3030 errors
- ✅ Enhanced error handling and logging
- ✅ Improved backup strategy with FTP fallback
- ✅ Added comprehensive post-deployment testing

### **Version 1 - Initial Release**
- Basic deployment automation
- File safety measures for `wwwroot`
- Azure CLI integration

---

*This script ensures safe, automated deployment while preserving user data and providing comprehensive error handling and verification.*