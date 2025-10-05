# 📁 ResellBook Project Structure Documentation

## 🏗️ **Core Project Structure**

### **Required Folders (Never Delete)**
```
ResellBook/
├── 📁 Controllers/          # API Controllers (AuthController, BooksController, LogsController, etc.)
├── 📁 Data/                # Entity Framework DbContext and database configurations
├── 📁 Helpers/             # Utility classes (JwtHelper, IndianTimeHelper, etc.)
├── 📁 Migrations/          # Entity Framework database migrations
├── 📁 Models/              # Data models (User, Book, UserLocation, etc.)
├── 📁 Properties/          # Project properties and publish profiles
├── 📁 Services/            # Business logic services (EmailService, etc.)
├── 📁 Utils/               # Static utilities (SimpleLogger, etc.)
└── 📁 Developer Documentation/  # Project documentation
```

### **Runtime Folders (Auto-Created)**
```
ResellBook/
├── 📁 bin/                 # Compiled binaries (auto-created during build)
├── 📁 obj/                 # Build temporary files (auto-created during build)
├── 📁 publish/             # Publish output (created by dotnet publish)
├── 📁 wwwroot/             # Static web files and user uploads
│   └── 📁 uploads/         # User uploaded files (book images, etc.)
└── 📁 AppLogs/             # Application log files
    ├── 📄 normal.txt       # Normal operation logs
    └── 📄 critical.txt     # Critical error logs
```

### **Backup Folders (Created by Deployment)**
```
ResellBook/
└── 📁 wwwroot-backup-YYYYMMDD-HHMMSS/  # Timestamped wwwroot backups
    └── wwwroot-backup.zip              # Complete backup of user files
```

---

## 🚫 **Folders to NEVER Commit to Git**

### **.gitignore Should Include:**
```gitignore
# Build outputs
bin/
obj/
publish/

# User uploads (sensitive data)
wwwroot/uploads/

# Application logs (contain sensitive information)
AppLogs/
*.log

# Deployment backups (too large for git)
wwwroot-backup-*/
temp-deploy-*/
temp-download-*/
*.zip

# Temporary deployment files
deploy-*.zip
bulletproof-deploy.zip
app-logs.zip

# Local settings
appsettings.Development.json  # Contains local database connections
```

---

## 📋 **Folder Purposes & Maintenance**

### **🔧 Development Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `Controllers/` | API endpoints and request handling | ✅ Commit to git |
| `Models/` | Database entities and DTOs | ✅ Commit to git |
| `Services/` | Business logic and external integrations | ✅ Commit to git |
| `Utils/` | Static utility classes (SimpleLogger) | ✅ Commit to git |
| `Data/` | DbContext and database configuration | ✅ Commit to git |
| `Migrations/` | EF Core database migrations | ✅ Commit to git |

### **🏗️ Build Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `bin/` | Compiled application binaries | ❌ Auto-generated, ignore in git |
| `obj/` | Build intermediate files | ❌ Auto-generated, ignore in git |
| `publish/` | Deployment package output | ❌ Created by deploy script |

### **📁 Runtime Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `wwwroot/` | Static web files and uploads | 🔒 **CRITICAL** - Never delete |
| `wwwroot/uploads/` | User uploaded book images | 🔒 **PROTECTED** by deploy script |
| `AppLogs/` | Application logging files | 📊 Monitor size, rotate if needed |

### **💾 Backup Folders**
| Folder | Purpose | Maintenance |
|--------|---------|-------------|
| `wwwroot-backup-*` | Deployment safety backups | 🗂️ Keep latest 2-3, delete old ones |

---

## 🚀 **Deployment Folder Behavior**

### **What Happens During `.\deploy.ps1`:**

#### **1. Backup Phase**
```
✅ Creates: wwwroot-backup-YYYYMMDD-HHMMSS/
✅ Downloads: Current production wwwroot as backup.zip
✅ Preserves: All user uploads and static files
```

#### **2. Build Phase**
```
✅ Creates: publish/ (temporary build output)
✅ Excludes: wwwroot/ from deployment package
✅ Packages: Application code only (no user files)
```

#### **3. Deploy Phase**
```
✅ Uploads: Application code to Azure
✅ Preserves: Existing wwwroot/ on server
✅ Maintains: All user uploads and images
```

#### **4. Cleanup Phase**
```
✅ Removes: Temporary deployment files
✅ Keeps: Latest backup for safety
✅ Cleans: Old backup folders
```

---

## 📊 **Folder Size Monitoring**

### **Expected Sizes:**
| Folder | Typical Size | Alert If Exceeds |
|--------|-------------|-----------------|
| `AppLogs/` | < 50 MB | 100 MB |
| `wwwroot/uploads/` | Varies by usage | 1 GB |
| `wwwroot-backup-*` | Same as uploads | Monitor disk space |
| `bin/` | ~50 MB | N/A (auto-managed) |

### **Maintenance Schedule:**
- **Daily**: Monitor `AppLogs/` growth
- **Weekly**: Check `wwwroot/uploads/` size
- **Monthly**: Clean old backup folders
- **After Deployment**: Verify backup creation

---

## 🔍 **Folder Health Checks**

### **Critical Folders Must Exist:**
```powershell
# Run this check after deployment
$criticalFolders = @("Controllers", "Models", "Utils", "wwwroot", "AppLogs")
foreach ($folder in $criticalFolders) {
    if (Test-Path $folder) {
        Write-Host "✅ $folder - OK" -ForegroundColor Green
    } else {
        Write-Host "❌ $folder - MISSING!" -ForegroundColor Red
    }
}
```

### **Log Files Health Check:**
```powershell
# Check logging system
if (Test-Path "AppLogs/normal.txt") {
    $normalSize = (Get-Item "AppLogs/normal.txt").Length
    Write-Host "📊 Normal logs: $([math]::Round($normalSize/1KB, 2)) KB"
}
if (Test-Path "AppLogs/critical.txt") {
    $criticalSize = (Get-Item "AppLogs/critical.txt").Length
    Write-Host "📊 Critical logs: $([math]::Round($criticalSize/1KB, 2)) KB"
}
```

---

## 🚨 **Emergency Recovery**

### **If wwwroot/ Gets Deleted:**
1. **Stop deployment immediately**
2. **Find latest backup**: `wwwroot-backup-YYYYMMDD-HHMMSS/`
3. **Extract backup.zip** to restore files
4. **Re-upload via Kudu**: https://resellbook20250929183655.scm.azurewebsites.net

### **If AppLogs/ Gets Deleted:**
1. **Restart application** (will auto-create folder)
2. **Test logging**: Call `/api/Logs/TestLogging`
3. **Verify creation**: Check `AppLogs/` folder exists

### **If Deploy Script Missing:**
1. **DO NOT DEPLOY** with any other method
2. **Contact team immediately**
3. **Restore from git** if necessary

---

## 📚 **Documentation Structure**

### **Developer Documentation/ Folder:**
```
Developer Documentation/
├── 📄 LOGS_CONTROLLER_API.md          # API documentation for logging
├── 📄 DEPLOYMENT_GUIDE_LOGGING.md     # Complete deployment guide
├── 📄 DEPLOYMENT_SAFETY_PROTOCOL.md   # Safety procedures
├── 📄 DEPLOYMENT_CHECKLIST.md         # Quick deployment steps
├── 📄 PROJECT_STRUCTURE.md            # This file
└── 📄 API_DOCUMENTATION_COMPLETE.md   # Complete API docs
```

---

## ✅ **Best Practices**

### **Do's:**
- ✅ Always use `.\deploy.ps1` for deployment
- ✅ Monitor `AppLogs/` folder size regularly
- ✅ Keep 2-3 recent backup folders
- ✅ Commit code changes to git regularly
- ✅ Test locally before deployment

### **Don'ts:**
- ❌ Never delete `wwwroot/` manually
- ❌ Never commit user uploads to git
- ❌ Never use manual deployment methods
- ❌ Never delete backup folders immediately
- ❌ Never ignore large log files

---

## 🎯 **Quick Commands**

### **Cleanup Temp Files:**
```powershell
Remove-Item "temp-*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "*.zip" -Force -ErrorAction SilentlyContinue
```

### **Check Folder Health:**
```powershell
Get-ChildItem -Directory | ForEach-Object { 
    $size = (Get-ChildItem $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum
    "$($_.Name): $([math]::Round($size/1MB, 2)) MB"
}
```

### **Monitor Logs:**
```powershell
Get-Content "AppLogs/critical.txt" -Tail 5
```

---

**Last Updated**: October 5, 2025  
**Status**: Production Structure ✅