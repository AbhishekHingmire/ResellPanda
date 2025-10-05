# 📋 FOLDER STRUCTURE QUICK REFERENCE

## ✅ **SHOULD EXIST (Core Project)**
```
ResellBook/
├── 📁 Controllers/              ✅ API Controllers
├── 📁 Data/                     ✅ Database context
├── 📁 Developer Documentation/  ✅ Project docs  
├── 📁 Helpers/                  ✅ Utility classes
├── 📁 Migrations/               ✅ EF migrations
├── 📁 Models/                   ✅ Data models
├── 📁 Properties/               ✅ Project settings
├── 📁 Services/                 ✅ Business logic
├── 📁 Utils/                    ✅ Static utilities
├── 📁 wwwroot/                  ✅ Web files & uploads
└── 📁 AppLogs/                  ✅ Application logs
```

## 🔄 **MAY EXIST (Build/Runtime)**
```
ResellBook/
├── 📁 bin/                      🔄 Build output (auto-created)
├── 📁 obj/                      🔄 Build temp (auto-created)
├── 📁 wwwroot-backup-*/         🔄 Deployment backups (1-2 recent)
└── 📄 deploy.ps1                ✅ CRITICAL - Deployment script
```

## ❌ **SHOULD NOT EXIST (Clean Project)**
```
❌ temp-deploy-*/        # Temporary deployment folders
❌ temp-download-*/      # Temporary backup folders  
❌ publish/              # Build output (cleaned after deploy)
❌ Logs/                 # Old empty folder (use AppLogs/)
❌ *.zip                 # Temporary deployment files
❌ deploy-*.ps1          # Old dangerous deployment scripts
```

## 🚫 **NEVER COMMIT TO GIT**
```
🚫 wwwroot/uploads/      # User images (sensitive)
🚫 AppLogs/             # Log files (sensitive)
🚫 bin/obj/             # Build outputs
🚫 wwwroot-backup-*/    # Backup folders (too large)
🚫 appsettings.Development.json  # Local settings
```

## 🧹 **QUICK CLEANUP**
```powershell
# Remove temporary files
Remove-Item "temp-*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "*.zip" -Force -ErrorAction SilentlyContinue  
Remove-Item "publish/" -Recurse -Force -ErrorAction SilentlyContinue

# Or use the maintenance script
.\maintenance.ps1
```

## 🚨 **EMERGENCY CHECKS**
```powershell
# If deploy.ps1 is missing - DO NOT DEPLOY!
Test-Path "deploy.ps1"  # Must return True

# If wwwroot is missing - RESTORE FROM BACKUP!
Test-Path "wwwroot"     # Must return True

# If critical folders missing - CHECK PROJECT INTEGRITY!
Test-Path "Controllers" # Must return True
Test-Path "Models"      # Must return True
Test-Path "Utils"       # Must return True
```

---
**Remember**: Only `deploy.ps1` should be used for deployment. All other deployment scripts have been removed for safety!