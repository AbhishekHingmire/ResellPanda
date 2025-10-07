# ⚠️ ResellPanda - Troubleshooting Guide

## 🚨 **Common Issues & Quick Fixes**

### **🔥 Critical Issues (Fix Immediately)**

#### **1. HTTP 500 Internal Server Error**
**Symptoms:** API calls return 500 status code
**Common Causes:**
- Database connection issues
- Model validation errors  
- Missing database migrations
- Null reference exceptions

**Quick Diagnosis:**
```bash
# Check logs immediately
curl -s "https://resellbook20250929183655.azurewebsites.net/api/Logs/GetLogsSummary" | ConvertFrom-Json
```

**Solutions:**
1. **Database Issues:** Run database migration
2. **Null References:** Check model properties for required fields
3. **Connection Timeout:** Restart Azure App Service

#### **2. ViewAll Books API Returns Empty/Error**
**Symptoms:** `/api/Books/ViewAll` returns no data or errors
**Root Cause:** UserLocation duplicate keys in GroupBy operation

**Solution Applied:** ✅ **FIXED**
- Updated ViewAll method with proper GroupBy logic
- Now handles duplicate UserLocation entries correctly

#### **3. User Images Not Loading**
**Symptoms:** Image URLs return 404 errors
**Root Cause:** wwwroot folder overwritten during deployment

**Prevention:** ✅ **SOLVED**
- Always use `.\deploy.ps1` script (preserves wwwroot)
- **Never use** old deployment methods

**Recovery:**
```bash
# Check if backup exists
ls wwwroot-backup-*

# Restore from latest backup
.\restore-images.ps1  # If backup available
```

---

## 📋 **Database Issues**

### **Migration Errors**
**Symptoms:**
- "Pending model changes" errors
- Database schema mismatch
- Missing columns/tables

**Solution:**
1. **MUST READ FIRST:** [Database Migration Guide](DATABASE_MIGRATION_GUIDE.md)
2. Run proper Entity Framework migration
3. Never skip migration steps

### **Connection Issues**
**Symptoms:** 
- "Cannot connect to database"
- Connection timeout errors

**Quick Checks:**
```bash
# Test database connection
dotnet ef database update

# Check connection string
# Verify Azure SQL firewall rules
```

---

## 🔐 **Authentication Issues**

### **JWT Token Problems**
**Symptoms:**
- 401 Unauthorized errors
- Token validation failures

**Solutions:**
```csharp
// Check token format
Authorization: Bearer <your_actual_token>

// Verify token hasn't expired (24 hour limit)
// Get new token via /api/Auth/login
```

### **OTP Not Working**
**Status:** ✅ **RESOLVED**
- SMTP configuration fixed
- OTP emails now sending properly
- 6-digit OTP with proper expiry

---

## 📱 **Android Integration Issues**

### **Network Requests Failing**
**Common Problems:**
1. **Base URL incorrect**
2. **Missing Authorization header**
3. **Wrong Content-Type**

**Correct Implementation:**
```kotlin
// Base URL
const val BASE_URL = "https://resellbook20250929183655.azurewebsites.net/"

// Headers
"Authorization" to "Bearer $token"
"Content-Type" to "application/json"
```

### **Image Upload Issues**
**Symptoms:**
- Images not uploading
- Multipart form errors

**Solution:**
```kotlin
// Use multipart/form-data for image uploads
val requestFile = RequestBody.create("image/*".toMediaType(), imageFile)
val body = MultipartBody.Part.createFormData("images", imageFile.name, requestFile)
```

---

## 🚀 **Deployment Issues**

### **HTTP 500.30 - App failed to start**
**Causes:**
- Wrong .NET version
- Missing dependencies
- Configuration errors

**Solutions:**
1. Ensure .NET 8.0 target framework
2. Run `dotnet build` locally first
3. Check appsettings.json configuration

### **Images Lost After Deployment**
**Prevention:** ✅ **SOLVED**
- Use only `.\deploy.ps1` script
- Automatically preserves wwwroot folder
- Creates backup before deployment

---

## 🛠️ **Development Tools**

### **Logging System**
**Check Application Logs:**
```bash
# Get recent logs summary
curl -s "https://resellbook20250929183655.azurewebsites.net/api/Logs/GetLogsSummary"

# Get detailed error logs  
curl -s "https://resellbook20250929183655.azurewebsites.net/api/Logs/GetCriticalLogs"
```

### **Health Checks**
```bash
# Test API connectivity
curl -s "https://resellbook20250929183655.azurewebsites.net/weatherforecast"

# Test specific endpoint
curl -X GET "https://resellbook20250929183655.azurewebsites.net/api/Books/ViewAll?userId=test-id"
```

---

## 🆘 **Emergency Procedures**

### **Complete System Down**
1. **Check Azure App Service status**
2. **Restart App Service** via Azure portal
3. **Check deployment logs**
4. **Rollback to last working deployment** if needed

### **Database Corrupted**
1. **Stop all write operations**
2. **Contact database admin**
3. **Use database backups** from Azure
4. **Run integrity checks**

### **All Images Lost**
1. **Check wwwroot-backup folders** in project
2. **Restore from latest backup**
3. **Implement emergency image placeholder system**
4. **Notify users about temporary issue**

---

## 📞 **Getting Help**

### **Log Analysis:**
- Use `/api/Logs/*` endpoints for real-time diagnosis
- Check both Normal and Critical logs
- Look for patterns in error timestamps

### **Code Issues:**
- Review recent code changes via Git
- Test locally before deployment
- Use proper Entity Framework practices

### **Infrastructure:**
- Azure portal for service status
- Database connection diagnostics
- Network connectivity tests

---

*Last Updated: October 2025*
*This guide covers the most common issues encountered during development and deployment.*