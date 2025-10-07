# 🚀 ResellPanda Developer Documentation

## **Welcome to ResellPanda API Documentation** 

This documentation is designed for developers building Android/mobile applications that integrate with the ResellPanda backend API.

---

## 📚 **Quick Navigation**

### **🔥 For New Developers - Start Here:**
1. **[📱 Android Integration Guide](01_ANDROID_INTEGRATION_GUIDE.md)** - Complete setup
2. **[🔐 Authentication APIs](02_AUTHENTICATION_API.md)** - User login/signup
3. **[📚 Books Management APIs](03_BOOKS_API.md)** - Core functionality
4. **[📍 Location & Profile APIs](04_LOCATION_PROFILE_API.md)** - User data
5. **[🚀 Deployment Guide](05_DEPLOYMENT_GUIDE.md)** - Deploy to Azure

### **🔍 For API Reference:**
- **[📋 Complete API Reference](API_REFERENCE_COMPLETE.md)** - All endpoints in one place
- **[🛠️ Development Tools](DEVELOPMENT_TOOLS.md)** - Logging, monitoring, maintenance

### **🆘 For Troubleshooting:**
- **[🔧 Database Migration Guide](DATABASE_MIGRATION_GUIDE.md)** - **CRITICAL** for model changes
- **[⚠️ Common Issues & Solutions](TROUBLESHOOTING_GUIDE.md)** - Quick fixes

---

## 🎯 **Production API Information**

### **Base URL:**
```
https://resellbook20250929183655.azurewebsites.net
```

### **Authentication:**
- **Method:** JWT Bearer Token
- **Header:** `Authorization: Bearer <your_jwt_token>`
- **Token Validity:** 24 hours

### **Main API Categories:**
| Category | Base Path | Purpose |
|----------|-----------|---------|
| Authentication | `/api/Auth/*` | User registration, login, password reset |
| Books | `/api/Books/*` | Book listings, search, management (with Description) ⭐ |
| User Location | `/api/UserLocation/*` | GPS sync, location tracking |
| User Search | `/api/UserSearch/*` | Search analytics and behavior tracking ⭐ NEW |
| Logs | `/api/Logs/*` | System monitoring (for developers) |

---

## 🚨 **IMPORTANT: Before Making Changes**

### **⚠️ Database Changes (Critical):**
If you need to add/modify database models:
1. **MUST READ:** [Database Migration Guide](DATABASE_MIGRATION_GUIDE.md) **FIRST**
2. Follow Entity Framework migration process
3. Test locally before deploying

### **📦 Deployment (Critical):**
- **ONLY use:** `.\deploy.ps1` script in root directory
- **NEVER use** old deployment scripts
- **Always preserve** wwwroot folder (contains user images)

---

## 📞 **Quick Help**

### **I want to...**
- **Build an Android app:** Start with [Android Integration Guide](01_ANDROID_INTEGRATION_GUIDE.md)
- **Understand all APIs:** Check [Complete API Reference](API_REFERENCE_COMPLETE.md)
- **Add a new model field:** Read [Database Migration Guide](DATABASE_MIGRATION_GUIDE.md) **FIRST**
- **Deploy changes:** Use [Deployment Guide](05_DEPLOYMENT_GUIDE.md)
- **Fix API errors:** Check [Troubleshooting Guide](TROUBLESHOOTING_GUIDE.md)

### **Emergency Contacts:**
- **500 Errors:** Check logs via `/api/Logs/GetLogsSummary`
- **Database Issues:** Follow database migration rollback procedures
- **Deployment Problems:** Use backup restoration from wwwroot-backup folders

---

## 📊 **System Status**
- **API Status:** ✅ Online
- **Database:** ✅ Connected  
- **Image Storage:** ✅ Operational
- **Logging:** ✅ Active

*Last Updated: October 2025*