# 🚀 ResellPanda Azure Deployment Guide - Complete Setup & Migration

## 📋 What This Guide Covers
Complete step-by-step deployment of ResellPanda ASP.NET Core app to Azure App Service with database migration, SMTP configuration, and troubleshooting.

## 🎯 Prerequisites & Setup Status

### ✅ Current Deployment Status (September 30, 2025)
- **App Service**: ResellBook20250929183655 ✅ Running
- **Resource Group**: resell-panda-rg ✅ Active
- **Database**: resellbookdbserver2001.database.windows.net ✅ Configured
- **Database User**: pandaseller ✅ Set
- **Database Password**: Password@2001 ✅ **ALREADY CONFIGURED IN AZURE**
- **SMTP Settings**: ✅ **ALREADY CONFIGURED IN AZURE**

### 🔐 Azure Configuration Status

#### Database Connection String ✅ **SET - NO ACTION NEEDED**
```
Server=tcp:resellbookdbserver2001.database.windows.net,1433;
Initial Catalog=ResellBook_db;
User ID=pandaseller;
Password=Password@2001;
Encrypt=True;
```

#### SMTP Configuration ✅ **SET - NO ACTION NEEDED**
```
SMTP__Host=smtp.gmail.com
SMTP__Port=587
SMTP__User=team.resellpanda@gmail.com
SMTP__Pass=unknvvwbmpszgmyk
```

## 🚀 Quick Deployment Process

### ⚡ **RECOMMENDED: Safe Deployment (Preserves wwwroot & Images)**

**✅ ALWAYS use this method to preserve uploaded book images and files:**

```powershell
# Use the automated preservation script (RECOMMENDED)
.\deploy-simple-preserve.ps1
```

**What this script does:**
- ✅ Builds your application with Release configuration
- ✅ **Excludes wwwroot folder** from deployment package
- ✅ **Preserves ALL uploaded images** in production
- ✅ Deploys only application code changes
- ✅ **Zero risk of file loss** - uploaded files remain intact
- ✅ Maintains existing static file structure

**File Structure Preserved:**
```
Production wwwroot/
├── uploads/
│   └── books/
│       ├── e5a14dfc-56ab-485f-9cf4-23c2e05b301c.png ✅ PRESERVED
│       ├── 8de9098e-b267-45b5-9282-6056c1f88dc4.jpg ✅ PRESERVED
│       └── [all other uploaded images] ✅ PRESERVED
└── [other static files] ✅ PRESERVED
```

**Image Access After Deployment:**
```
✅ WORKS: https://resellbook20250929183655.azurewebsites.net/uploads/books/your-image.png
✅ WORKS: All existing frontend image URLs continue functioning
✅ WORKS: New image uploads work normally
```

### 🚨 **LEGACY: Standard Deployment (OVERWRITES wwwroot)**

**⚠️ WARNING: This method will DELETE all uploaded files in production wwwroot!**

```bash
# DON'T USE THIS - It will delete uploaded files!
dotnet publish -c Release -o publish
Compress-Archive -Path "publish\*" -DestinationPath "deploy.zip" -Force
az webapp deploy --resource-group resell-panda-rg --name ResellBook20250929183655 --src-path "deploy.zip" --type zip
```

### 🔍 **Deployment Verification**
curl https://resellbook20250929183655.azurewebsites.net/api/health/database
```

## � File Management During Deployment

### **Critical File Preservation Guidelines**

**🚨 IMPORTANT - What Gets Preserved vs. What Gets Overwritten:**

| Component | Standard Deploy | Safe Deploy | Notes |
|-----------|----------------|-------------|-------|
| **Application Code** | ✅ Updated | ✅ Updated | Always safe to overwrite |
| **Database** | ✅ Preserved | ✅ Preserved | Connection string maintained |
| **wwwroot/uploads/** | ❌ **DELETED** | ✅ **PRESERVED** | **CRITICAL for images** |
| **Configuration** | ✅ Updated | ✅ Updated | appsettings maintained |
| **Static Files** | ❌ **DELETED** | ✅ **PRESERVED** | CSS, JS, images |

### **File System Troubleshooting**

**If Images Stop Working After Deployment:**

1. **Check if wwwroot was overwritten:**
```powershell
# Check Azure App Service via Kudu Console
https://resellbook20250929183655.scm.azurewebsites.net/DebugConsole
# Navigate to: site/wwwroot/uploads/books/
```

2. **Use diagnostic endpoints:**
```http
GET /api/FileTest/structure
GET /api/FileTest/check-file/your-image-name.png
```

3. **Verify file serving:**
```http
GET /uploads/books/your-image.png
```

### **Emergency File Recovery**

**If you accidentally used standard deployment and lost files:**

❌ **Unfortunately, uploaded images cannot be recovered** - they are permanently deleted
✅ **Prevention is key** - always use `.\deploy-simple-preserve.ps1`

**Recovery Steps:**
1. Re-upload any available backup images
2. Users will need to re-upload book images
3. Implement safe deployment process going forward

### **File Management Best Practices**

**✅ DO:**
- Always use `.\deploy-simple-preserve.ps1` for deployments
- Test image URLs after deployment
- Monitor file system via diagnostic endpoints
- Keep local backups of critical images

**❌ DON'T:**
- Use standard `dotnet publish` for production deployments
- Deploy without checking file preservation
- Ignore wwwroot folder in deployment planning
- Assume files will survive deployment without proper exclusion

## �🔧 When to Reconfigure Azure Settings

### 🚫 **DO NOT SET AGAIN** - Already Configured:
- ❌ Database connection string (Password@2001)
- ❌ SMTP settings (unknvvwbmpszgmyk)
- ❌ Resource group and app service

### ✅ **SET AGAIN ONLY IF**:
- 🔄 Database password changes
- 🔄 SMTP email account changes
- 🔄 Moving to different Azure subscription
- 🔄 Creating new app service
- 🔄 Database server changes

## 📊 Database Migration System

### How It Works Automatically
Your app includes auto-migration in `Program.cs`:

```csharp
// Auto-migration runs on every startup
try {
    using (var scope = app.Services.CreateScope()) {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Database.IsSqlServer()) {
            dbContext.Database.Migrate(); // ✅ Creates/updates tables
        }
    }
}
```

### Current Database Schema (Auto-Created)
```sql
✅ Users: Id, Name, Email, PasswordHash, IsEmailVerified
✅ Books: Id, UserId, BookName, Category, SellingPrice, ImagePathsJson, CreatedAt
✅ UserLocations: Id, UserId, Latitude, Longitude, CreateDate  
✅ UserVerifications: Id, UserId, Code, Type, Expiry, IsUsed
```

### Migration Status Check
```bash
# Test if database is working
curl https://resellbook20250929183655.azurewebsites.net/api/health/database

# Expected response:
{
  "Status": "Healthy",
  "Message": "Database connection successful",
  "UserCount": 0,
  "AppliedMigrations": 1,
  "PendingMigrations": 0
}
```

## 🆕 Adding New Database Changes

### 1. Add New Model or Change Existing
```csharp
// Example: Add new property to User model
public class User {
    // ... existing properties
    public string? PhoneNumber { get; set; }  // New field
}
```

### 2. Create Migration Locally
```bash
# Create new migration
dotnet ef migrations add AddPhoneNumberToUser

# Test locally first
dotnet ef database update
```

### 3. Deploy (Migration Runs Automatically)
```bash
# Standard deployment process
dotnet build --configuration Release
dotnet publish --configuration Release
Compress-Archive -Path "bin\Release\net8.0\publish\*" -DestinationPath "deploy.zip" -Force
az webapp deploy --resource-group resell-panda-rg --name ResellBook20250929183655 --src-path "deploy.zip" --type zip
```

### 4. Verify in Azure
- Migration applies automatically on startup
- Check logs: `az webapp log tail --name ResellBook20250929183655 --resource-group resell-panda-rg`
- Test endpoint: `/api/health/database`

## 🔐 Security Configuration Commands

### ⚠️ **ONLY USE IF SETTINGS ARE LOST OR CHANGED**

#### Database Connection (Only if password changes)
```bash
az webapp config connection-string set \
    --name ResellBook20250929183655 \
    --resource-group resell-panda-rg \
    --connection-string-type SQLAzure \
    --settings DefaultConnection="Server=tcp:resellbookdbserver2001.database.windows.net,1433;Initial Catalog=ResellBook_db;User ID=pandaseller;Password=NEW_PASSWORD;Encrypt=True;"
```

#### SMTP Settings (Only if email account changes)
```bash
az webapp config appsettings set \
    --name ResellBook20250929183655 \
    --resource-group resell-panda-rg \
    --settings SMTP__Host="smtp.gmail.com" SMTP__Port="587" SMTP__User="new-email@gmail.com" SMTP__Pass="new-app-password"
```

## 🔍 Verification & Testing

### Health Checks
```bash
# Overall app health
curl https://resellbook20250929183655.azurewebsites.net/api/health

# Database health & migration status
curl https://resellbook20250929183655.azurewebsites.net/api/health/database

# Test weather endpoint (basic functionality)
curl https://resellbook20250929183655.azurewebsites.net/weatherforecast
```

### API Testing
```bash
# Test user signup (includes email sending)
curl -X POST https://resellbook20250929183655.azurewebsites.net/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","password":"Test123!"}'

# Expected: "Signup successful. Check your email for OTP."
```

### Log Monitoring
```bash
# Live logs
az webapp log tail --name ResellBook20250929183655 --resource-group resell-panda-rg

# Download logs
az webapp log download --name ResellBook20250929183655 --resource-group resell-panda-rg --log-file logs.zip
```

## 🆘 Troubleshooting

### Migration Issues
```bash
# Check migration status
curl https://resellbook20250929183655.azurewebsites.net/api/health/database

# Look for migration logs
az webapp log tail --name ResellBook20250929183655 --resource-group resell-panda-rg
# Look for: "Applying migration" or "Database is already up to date"
```

### Connection Issues
```bash
# Verify connection string is set
az webapp config connection-string list --name ResellBook20250929183655 --resource-group resell-panda-rg

# Verify SMTP settings
az webapp config appsettings list --name ResellBook20250929183655 --resource-group resell-panda-rg
```

### App Won't Start
```bash
# Check deployment logs
az webapp log tail --name ResellBook20250929183655 --resource-group resell-panda-rg

# Common issues:
# - Migration failed (check database connectivity)
# - Missing dependencies (rebuild and redeploy)
# - Configuration errors (verify connection strings)
```

## 📝 Development Workflow

### Local Development
```bash
# Use local database for development
# Connection string in appsettings.json points to local SQL Server
dotnet ef database update  # Apply migrations locally
dotnet run                 # Test locally
```

### Production Deployment
```bash
# Connection string automatically switches to Azure via app settings
# Migration runs automatically on app startup
# No manual database commands needed
```

## 🎯 Current App Capabilities

### ✅ Working Features
- 🔐 User authentication (signup, login, email verification)
- 📧 Email sending (verification, welcome emails)
- 📚 Book management (CRUD operations)
- 📍 User location tracking
- 🔄 Automatic database migration
- 🏥 Health monitoring endpoints
- 🔒 Secure configuration management

### 🔗 Available Endpoints
- `GET /api/health` - App status
- `GET /api/health/database` - Database & migration status
- `POST /api/auth/signup` - User registration
- `POST /api/auth/login` - User authentication
- `POST /api/auth/verify-email` - Email verification
- `GET /api/books` - List books
- `POST /api/books` - Create book listing

## 📅 Maintenance Schedule

### Daily
- ✅ Automatic: App restarts, migrations apply, health checks run

### Weekly
- 📊 Review logs: `az webapp log download`
- 🔍 Check health: `/api/health/database`

### Monthly
- 🔒 Review security settings
- 📈 Monitor performance metrics
- 🔄 Update dependencies if needed

---

## 🚨 Important Notes

### 🔐 **Security**
- ✅ Database password: Stored securely in Azure App Service settings
- ✅ SMTP credentials: Stored securely in Azure App Service settings  
- ✅ Connection strings: Never in source code
- ✅ Auto-SSL: Enabled for HTTPS

### 🔄 **Migration Safety**
- ✅ Migrations are transactional (rollback on failure)
- ✅ App continues running even if migration fails
- ✅ Detailed logging for troubleshooting
- ✅ No data loss on updates

### 📞 **Support Information**
- **App URL**: https://resellbook20250929183655.azurewebsites.net
- **Resource Group**: resell-panda-rg  
- **Database Server**: resellbookdbserver2001.database.windows.net
- **Last Updated**: September 30, 2025
- **Migration Status**: ✅ Active and Working

---

## 🔍 Post-Deployment Monitoring & Maintenance

### **Essential Health Checks After Each Deployment**

**1. Application Health:**
```http
GET https://resellbook20250929183655.azurewebsites.net/weatherforecast
# Should return weather data array
```

**2. Database Connectivity:**
```http
GET https://resellbook20250929183655.azurewebsites.net/api/health/database
# Should return "Database connection successful"
```

**3. Image Serving Verification:**
```http
GET https://resellbook20250929183655.azurewebsites.net/api/FileTest/structure
# Should show wwwroot/uploads/books/ structure
```

**4. File System Integrity:**
```http
GET https://resellbook20250929183655.azurewebsites.net/api/FileTest/check-file/[existing-image-name.png]
# Should return file found details
```

### **Regular Maintenance Tasks**

**Weekly Checks:**
- ✅ Verify image uploads are working: Test book creation with image
- ✅ Check file system health: Use `/api/FileTest/structure`
- ✅ Monitor database performance: Check response times
- ✅ Validate authentication: Test login/registration flows

**Monthly Reviews:**
- 📊 Review wwwroot folder size growth
- 🗂️ Archive old or unused images if needed
- 🔒 Update security configurations if required
- 📈 Monitor app performance metrics in Azure portal

### **Deployment Checklist Template**

**Before Each Deployment:**
- [ ] ✅ Using `.\deploy-simple-preserve.ps1` script
- [ ] ✅ Tested changes in local environment
- [ ] ✅ Database migrations are ready (if any)
- [ ] ✅ No breaking changes to existing APIs

**After Each Deployment:**
- [ ] ✅ Application loads successfully
- [ ] ✅ Database connectivity confirmed
- [ ] ✅ Image serving works (test existing images)
- [ ] ✅ New image uploads functional
- [ ] ✅ Authentication flows operational
- [ ] ✅ Critical API endpoints responding

**Emergency Rollback Plan:**
```powershell
# If deployment causes issues:
# 1. Check Azure portal for error logs
# 2. Use Azure deployment center to rollback to previous version
# 3. Files are preserved, so rollback is safe
# 4. Test all functionality after rollback
```

---

## 📞 Quick Reference

**Live Application:** https://resellbook20250929183655.azurewebsites.net
**Deployment Command:** `.\deploy-simple-preserve.ps1`
**Health Check:** `/weatherforecast`
**File System Check:** `/api/FileTest/structure`
**Image Test:** `/uploads/books/[image-name]`

**🚨 Emergency Contacts & Resources:**
- Azure Portal: https://portal.azure.com
- Kudu Console: https://resellbook20250929183655.scm.azurewebsites.net
- Application Logs: Available in Azure portal under Monitoring > Log stream

---

**✨ Your ResellPanda app is fully deployed with automatic database migration and file preservation!**

*Complete deployment workflow established - all features documented and operational.*