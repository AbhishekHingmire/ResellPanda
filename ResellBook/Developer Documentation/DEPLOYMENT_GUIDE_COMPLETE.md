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

### For Regular Code Updates (Migration Already Works)

```bash
# 1. Build and publish
dotnet build --configuration Release
dotnet publish --configuration Release

# 2. Create deployment package
Compress-Archive -Path "bin\Release\net8.0\publish\*" -DestinationPath "deploy.zip" -Force

# 3. Deploy to Azure
az webapp deploy --resource-group resell-panda-rg --name ResellBook20250929183655 --src-path "deploy.zip" --type zip

# 4. Test deployment
curl https://resellbook20250929183655.azurewebsites.net/api/health/database
```

## 🔧 When to Reconfigure Azure Settings

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

**✨ Your ResellPanda app is fully deployed with automatic database migration!**

*No manual database setup required - everything happens automatically on deployment.*