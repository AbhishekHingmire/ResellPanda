# 🚀 QUICK DEPLOYMENT CHECKLIST

## ✅ Pre-Deployment (30 seconds)
```powershell
# 1. Verify you're in the right directory
cd "c:\Repos\ResellPanda\ResellBook"

# 2. Check the deployment script exists
Test-Path "deploy.ps1"  # Should return: True

# 3. Verify Azure CLI login
az account show  # Should show your account

# 4. Quick build test
dotnet build --configuration Release  # Should succeed
```

## 🚀 Deployment (One Command)
```powershell
# THE ONLY DEPLOYMENT COMMAND:
.\deploy.ps1
```

## ✅ Post-Deployment (2 minutes)
```powershell
# 1. Test application is running
curl "https://resellbook20250929183655.azurewebsites.net/api/Test/ping"

# 2. Verify ViewAll endpoint is fixed (was 500 error)
curl "https://resellbook20250929183655.azurewebsites.net/api/Books/ViewAll?userId=test"

# 3. Check logging system works
curl "https://resellbook20250929183655.azurewebsites.net/api/Logs/TestLogging"
```

## 🔍 Manual File Verification
1. Go to: https://resellbook20250929183655.scm.azurewebsites.net/newui
2. Navigate to: `site/wwwroot/`
3. Confirm: All user folders and images are still there
4. If files missing: **ALERT TEAM IMMEDIATELY**

## 🚨 Emergency Contacts
- **If deployment fails**: Stop immediately, contact team lead
- **If files missing**: Check backup folder created by script
- **Backup location**: `wwwroot-backup-YYYYMMDD-HHMMSS/`

---
## 📋 Success Criteria
- [x] Application responds to ping
- [x] ViewAll returns 200 (not 500)
- [x] Logging endpoints work
- [x] **ALL user images still present**
- [x] No critical errors in logs

## ❌ Failure Signs
- [ ] Any user images missing
- [ ] ViewAll still returns 500 error
- [ ] Application not responding
- [ ] New critical errors appeared

---
**Remember: If `deploy.ps1` doesn't exist, DO NOT DEPLOY - contact the team!**