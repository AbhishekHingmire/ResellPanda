# 🛡️ DEPLOYMENT SAFETY CHECKLIST & PROTOCOL

## ⚠️ **NEVER AGAIN: Why Images Were Lost**

### What Went Wrong:
- **`az webapp deploy --type zip`** = **REPLACES ENTIRE SITE** (including wwwroot)
- **Excluding files from zip ≠ Preserving files on server**
- **Azure App Service zip deployment is DESTRUCTIVE by design**

### The Solution:
- **ALWAYS use `deploy-bulletproof-safe.ps1`** 
- **NEVER use manual `dotnet publish` commands**
- **NEVER use `az webapp deploy --type zip`**

---

## 📋 **MANDATORY PRE-DEPLOYMENT CHECKLIST**

### ✅ Before Every Deployment:
1. [ ] **Verify THE script exists**: `deploy.ps1` (if missing, DO NOT DEPLOY)
2. [ ] **Check Azure CLI login**: `az account show` returns valid account
3. [ ] **Verify resource group access**: Can access `resell-panda-rg`
4. [ ] **Confirm no other scripts**: Only `deploy.ps1` should exist in root
5. [ ] **Build test**: `dotnet build --configuration Release` succeeds
6. [ ] **Backup awareness**: Script will auto-backup wwwroot with timestamp

### ✅ Deployment Command (ONLY THIS ONE):
```powershell
# THE SINGLE DEPLOYMENT COMMAND (ALL OTHERS REMOVED FOR SAFETY):
.\deploy.ps1

# This is the ONLY deployment script that exists now.
# All dangerous scripts have been permanently removed.
```

### 🗑️ **Dangerous Scripts Removed:**
```powershell
# ❌ THESE SCRIPTS NO LONGER EXIST (DELETED FOR SAFETY):
# deploy-simple-preserve.ps1        # DELETED - was losing user images
# deploy-advanced-preserve.ps1      # DELETED - unreliable auth  
# deploy-preserve-wwwroot.ps1       # DELETED - inconsistent results
# deploy-bulletproof-safe.ps1       # DELETED - had Kudu API issues

# ❌ MANUAL METHODS STILL DANGEROUS (DON'T USE):
# az webapp deploy --type zip              (DESTRUCTIVE)
# dotnet publish + Visual Studio Publish  (OVERWRITES wwwroot)
# Azure Portal manual upload              (INCONSISTENT)
```

---

## 🔒 **BULLETPROOF DEPLOYMENT PROCESS**

### What the Safe Script Does:
1. **🔐 Gets Kudu API credentials** (secure access)
2. **💾 Downloads entire wwwroot as backup ZIP** (before touching anything)
3. **🔨 Builds application** (with error checking)
4. **📦 Creates deployment package** (excluding wwwroot)
5. **🚀 Deploys via Kudu ZipDeploy API** (safer than az cli)
6. **⏳ Waits for deployment stabilization** (prevents race conditions)
7. **🔄 Restores wwwroot from backup** (puts everything back)
8. **🔍 Verifies files exist** (confirms success)
9. **🧪 Tests endpoints** (ensures app works)
10. **💾 Keeps backup safe** (for manual recovery if needed)

### Backup Strategy:
```
Local Backups Created:
├── wwwroot-backup-20251005-163000/
│   └── wwwroot-backup.zip          # Complete production backup
├── temp-deploy-20251005163001/     # Deployment staging (deleted)
└── bulletproof-deploy.zip          # App package (deleted)
```

---

## 🚨 **RECOVERY PROCEDURE**

### If Images Are Lost (Emergency Recovery):
1. **Stop deployment immediately**
2. **Check backup folder**: `wwwroot-backup-YYYYMMDD-HHMMSS/`
3. **Manual restore via Kudu**:
   ```powershell
   # Upload backup to restore files
   $backupPath = "wwwroot-backup-[TIMESTAMP]/wwwroot-backup.zip"
   # Use Kudu console: https://resellbook20250929183655.scm.azurewebsites.net
   ```

### Prevention Measures:
- **NEVER** use any deployment method except `deploy-bulletproof-safe.ps1`
- **ALWAYS** verify backup creation before deployment
- **ALWAYS** check file count after deployment

---

## 📊 **DEPLOYMENT VERIFICATION PROTOCOL**

### Required Post-Deployment Checks:
```powershell
# 1. Check application works
curl https://resellbook20250929183655.azurewebsites.net/api/Test/ping

# 2. Verify logging system
curl https://resellbook20250929183655.azurewebsites.net/api/Logs/TestLogging

# 3. Test ViewAll endpoint (was broken)
curl "https://resellbook20250929183655.azurewebsites.net/api/Books/ViewAll?userId=test"

# 4. Verify file preservation (manual check via Kudu)
# Go to: https://resellbook20250929183655.scm.azurewebsites.net/newui
# Navigate to: site/wwwroot/
# Confirm: Images and folders still exist
```

---

## 🔧 **TEAM DEPLOYMENT RULES**

### Authorized Deployment Methods:
1. **✅ ONLY**: `.\deploy.ps1` (the single remaining script)
2. **❌ NEVER**: Visual Studio Publish (overwrites wwwroot)
3. **❌ NEVER**: `az webapp deploy` (destructive to files)
4. **❌ NEVER**: Manual file upload (inconsistent)
5. **❌ NEVER**: `dotnet publish` direct (no file protection)
6. **❌ NEVER**: Any other .ps1 script (all removed for safety)

### Who Can Deploy:
- **Primary**: Lead Developer (with backup verification)
- **Secondary**: Team Lead (emergency only)
- **Process**: Always announce in team chat before deployment

### Deployment Schedule:
- **Preferred**: Off-peak hours (minimize user impact)
- **Required**: Advance notice to team
- **Backup**: Always keep previous version ready

---

## 📝 **DEPLOYMENT LOG TEMPLATE**

```
DEPLOYMENT LOG
==============
Date: [YYYY-MM-DD HH:MM]
Deployer: [Name]
Branch: [branch-name]
Commit: [commit-hash]

Pre-deployment:
[ ] Build successful
[ ] Backup script ready
[ ] Team notified

Deployment:
[ ] Backup created: [backup-folder-name]
[ ] Application deployed
[ ] Files restored
[ ] Endpoints tested

Post-deployment:
[ ] Images verified present
[ ] Logging system working
[ ] ViewAll endpoint fixed
[ ] No critical errors

Backup Location: [folder-path]
Status: SUCCESS / FAILED
Notes: [any issues or observations]
```

---

## 🎯 **SUCCESS CRITERIA**

### Deployment is SUCCESSFUL only when:
- [x] Application responds to ping
- [x] ViewAll endpoint returns 200 (not 500)
- [x] Logging endpoints work
- [x] **ALL user images still present**
- [x] **NO files lost in wwwroot**
- [x] Backup created and preserved
- [x] No new critical errors

### Deployment is FAILED if:
- [ ] Any user images missing
- [ ] ViewAll still returns 500
- [ ] Logging system not working
- [ ] Application not responding
- [ ] Database connection issues

---

## 📞 **EMERGENCY CONTACTS**

### If Deployment Fails:
1. **Immediate**: Stop deployment process
2. **Check**: Backup folder for manual recovery
3. **Notify**: Team lead immediately
4. **Rollback**: Use previous backup if needed

### Recovery Resources:
- **Kudu Console**: https://resellbook20250929183655.scm.azurewebsites.net/newui
- **Azure Portal**: ResellBook20250929183655 App Service
- **Backup Location**: Local `wwwroot-backup-*` folders

---

## 🔄 **CONTINUOUS IMPROVEMENT**

### After Each Deployment:
1. **Document** any issues encountered
2. **Update** this checklist if needed  
3. **Improve** the bulletproof script if gaps found
4. **Train** team members on safe practices

### Monthly Review:
- Review all deployment logs
- Check backup strategy effectiveness
- Update procedures based on lessons learned
- Verify team compliance with protocols

---

**Remember: USER DATA IS SACRED. NEVER DEPLOY WITHOUT BULLETPROOF BACKUP!** 🛡️