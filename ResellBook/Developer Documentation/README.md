# 📚 ResellPanda API Developer Documentation

## **Complete API Documentation Suite**

Welcome to the comprehensive developer documentation for ResellPanda API integration. This documentation is specifically designed for Android Kotlin developers building mobile applications.

---

## 📖 **Documentation Index**

### **📋 1. [Complete API Reference](API_DOCUMENTATION_COMPLETE.md)**
- **Overview:** Comprehensive reference covering all available endpoints
- **Contents:** All APIs from all controllers with detailed examples
- **Target Audience:** Developers needing complete API overview
- **Use Case:** Quick reference and complete endpoint discovery

### **🔐 2. [Authentication APIs](AUTHENTICATION_API.md)**
- **Overview:** User authentication and account management
- **Endpoints:** Signup, Login, OTP verification, Password reset
- **Features:** Email verification, JWT tokens, secure password handling
- **Security:** BCrypt hashing, OTP expiry, single-use tokens
- **Android Examples:** Complete Kotlin implementation with error handling

### **📚 3. [Books Management APIs](BOOKS_API.md)**
- **Overview:** Book listing creation, editing, and browsing
- **Endpoints:** Add books, edit listings, view all books
- **Features:** Multi-image upload, search and filtering
- **File Handling:** Multipart form data, image compression
- **Android Examples:** Image picker, compression, upload progress

### **📍 4. [Location & Profile APIs](LOCATION_AND_PROFILE_API.md)**
- **Overview:** User location tracking and profile management
- **Endpoints:** GPS sync, location history, user profiles
- **Features:** Background location sync, geofencing capabilities
- **Integration:** Google Maps, location permissions
- **Android Examples:** Fusion location provider, work manager sync

### **🤖 5. [Android Kotlin Integration Guide](ANDROID_INTEGRATION_GUIDE.md)**
- **Overview:** Complete Android project setup and architecture
- **Architecture:** MVVM pattern, Repository pattern, Clean architecture
- **Libraries:** Retrofit, Room, Work Manager, Glide, Security
- **Features:** Dependency injection, error handling, offline support
- **Production:** ProGuard rules, security configurations

### **🗄️ 6. [Database Migration Guide](DATABASE_MIGRATION_GUIDE.md)** ⭐ **ESSENTIAL**
- **Overview:** Complete database migration and model management guide
- **Purpose:** Prevent 500 errors when adding new model properties
- **Contents:** Step-by-step migration process, troubleshooting, best practices
- **Emergency:** Database reset procedures for critical issues
- **Features:** Entity Framework Core workflows, Azure database management

---

## 🚀 **Quick Start Guide**

### **For New Developers:**
1. Start with **[Android Integration Guide](ANDROID_INTEGRATION_GUIDE.md)** for project setup
2. **IMPORTANT**: Read **[Database Migration Guide](DATABASE_MIGRATION_GUIDE.md)** before making model changes
2. Review **[Authentication APIs](AUTHENTICATION_API.md)** for user management
3. Implement **[Books Management](BOOKS_API.md)** for core functionality
4. Add **[Location Services](LOCATION_AND_PROFILE_API.md)** for enhanced features

### **For API Reference:**
- Use **[Complete API Reference](API_DOCUMENTATION_COMPLETE.md)** for quick endpoint lookup
- Each category documentation provides detailed implementation examples

---

## 🔧 **API Base Information**

### **Production API URL:**
```
https://resellbook20250929183655.azurewebsites.net
```

### **API Categories:**
- **Authentication:** `/api/Auth/*`
- **Books Management:** `/api/Books/*`
- **User Location:** `/api/UserLocation/*`
- **System/Health:** `/weatherforecast`

### **Authentication Method:**
- **Type:** JWT Bearer Token
- **Header:** `Authorization: Bearer <token>`
- **Expiry:** 24 hours (recommended)

---

## 📊 **API Endpoint Summary**

| Category | Endpoint | Method | Auth Required | Purpose |
|----------|----------|---------|---------------|---------|
| **Auth** | `/api/Auth/signup` | POST | ❌ | User registration |
| **Auth** | `/api/Auth/verify-email` | POST | ❌ | Email verification |
| **Auth** | `/api/Auth/login` | POST | ❌ | User authentication |
| **Auth** | `/api/Auth/resend-otp` | POST | ❌ | Resend verification OTP |
| **Auth** | `/api/Auth/forgot-password` | POST | ❌ | Password reset request |
| **Auth** | `/api/Auth/verify-reset-otp` | POST | ❌ | Verify reset OTP |
| **Auth** | `/api/Auth/reset-password` | POST | ❌ | Complete password reset |
| **Books** | `/api/Books/ListBook` | POST | ✅ | Create book listing |
| **Books** | `/api/Books/EditListing/{id}` | PUT | ✅ | Update book listing |
| **Books** | `/api/Books/ViewAll/{userId}` | GET | ❌ | Get all books with user info |
| **Books** | `/api/Books/ViewMyListings/{userId}` | GET | ✅ | Get user's book listings |
| **Books** | `/api/Books/MarkAsSold/{bookId}` | PATCH | ✅ | Mark book as sold |
| **Books** | `/api/Books/Delete/{bookId}` | DELETE | ✅ | Delete book listing |
| **Chat** | `/api/Chat/SendMessage/{senderId}` | POST | ✅ | Send message to book owner |
| **Chat** | `/api/Chat/GetChats/{userId}` | GET | ✅ | Get user's chat conversations |
| **Chat** | `/api/Chat/GetChatMessages/{userId}/{otherUserId}` | GET | ✅ | Get messages between users |
| **Chat** | `/api/Chat/MarkAsRead/{userId}` | POST | ✅ | Mark messages as read |
| **Chat** | `/api/Chat/GetUnreadCount/{userId}` | GET | ✅ | Get unread message count |
| **Chat** | `/api/Chat/DeleteMessage/{messageId}/{userId}` | DELETE | ✅ | Delete single message |
| **Chat** | `/api/Chat/DeleteChat/{userId}/{otherUserId}` | DELETE | ✅ | Delete entire chat |
| **Chat** | `/api/Chat/GetBookForMessage/{bookId}` | GET | ✅ | Get book context for messaging |
| **Location** | `/api/UserLocation/SyncLocation` | POST | ✅ | Sync GPS location |
| **Location** | `/api/UserLocation/GetLocations/{userId}` | GET | ✅ | Get location history |
| **Profile** | `/api/UserLocation/profile/{userId}` | GET | ✅ | Get user profile |
| **System** | `/weatherforecast` | GET | ❌ | API health check |

---

## 🛠️ **Development Tools & Libraries**

### **Android Dependencies:**
```gradle
// Networking
implementation 'com.squareup.retrofit2:retrofit:2.9.0'
implementation 'com.squareup.retrofit2:converter-gson:2.9.0'

// Image Handling
implementation 'com.github.bumptech.glide:glide:4.16.0'
implementation 'id.zelory:compressor:3.0.1'

// Location Services
implementation 'com.google.android.gms:play-services-location:21.0.1'
implementation 'com.google.android.gms:play-services-maps:18.2.0'

// Architecture Components
implementation 'androidx.lifecycle:lifecycle-viewmodel-ktx:2.7.0'
implementation 'androidx.work:work-runtime-ktx:2.9.0'

// Security
implementation 'androidx.security:security-crypto:1.1.0-alpha06'
```

### **Key Features Supported:**
- ✅ **JWT Authentication** with secure token storage
- ✅ **Image Upload** with compression and progress tracking
- ✅ **Location Services** with background sync
- ✅ **Offline Support** with local caching
- ✅ **Error Handling** with retry mechanisms
- ✅ **MVVM Architecture** with Repository pattern
- ✅ **Background Tasks** using Work Manager

---

## 📱 **Android Implementation Highlights**

### **Authentication Flow:**
```kotlin
// Complete signup to login flow
authViewModel.signup(name, email, password)
authViewModel.verifyEmail(email, otp)
authViewModel.login(email, password)
```

### **Book Upload Flow:**
```kotlin
// Multi-image book upload with compression
booksViewModel.uploadBook(userId, bookData, compressedImages)
```

### **Location Sync:**
```kotlin
// Background location synchronization
locationManager.syncCurrentLocation(userId)
```

---

## 🔍 **Testing & Debugging**

### **API Testing Tools:**
- **Postman Collection:** Available for all endpoints
- **cURL Examples:** Provided in each documentation section
- **Android Debugging:** Comprehensive logging and error handling

### **Common Error Codes:**
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (missing/invalid token)
- `404` - Not Found (resource doesn't exist)
- `500` - Server Error (backend issues)

---

## 📈 **Performance Considerations**

### **Optimization Tips:**
1. **Image Compression:** Always compress images before upload
2. **Caching:** Implement local caching for frequently accessed data
3. **Background Sync:** Use Work Manager for non-urgent operations
4. **Token Management:** Implement automatic token refresh
5. **Error Retry:** Implement exponential backoff for network failures

### **Best Practices:**
- Use Repository pattern for data management
- Implement proper error handling with user-friendly messages
- Cache API responses locally for offline access
- Compress images before uploading to reduce bandwidth
- Implement pagination for large data sets (future enhancement)

---

## 🆘 **Support & Troubleshooting**

### **Common Issues:**
1. **500 Internal Server Error:** Usually caused by database schema mismatch - see [Database Migration Guide](DATABASE_MIGRATION_GUIDE.md)
2. **OTP Not Received:** Check email spam folder, use resend functionality
3. **Image Upload Failed:** Ensure images are under 5MB, check network
4. **Location Permission:** Request proper runtime permissions
5. **Token Expired:** Implement automatic token refresh mechanism
6. **Migration Conflicts:** Follow emergency database reset procedure in migration guide

### **Debug Information:**
- Enable network logging in debug builds
- Use proper exception handling with detailed error messages
- Implement comprehensive logging for production debugging

---

## 📝 **Version Information**

- **API Version:** v1.0
- **Documentation Version:** 1.0
- **Last Updated:** September 30, 2025
- **Android Target SDK:** 34 (Android 14)
- **Minimum SDK:** 24 (Android 7.0)

---

## 🔄 **Updates & Changelog**

### **Version 1.0 (Current)**
- ✅ Complete authentication system with OTP verification
- ✅ Books management with multi-image upload
- ✅ Location tracking and user profiles
- ✅ Comprehensive Android Kotlin integration
- ✅ Production-ready security and error handling

### **Future Enhancements:**
- 📅 Push notifications for book updates
- 📅 Advanced search and filtering
- 📅 User messaging system
- 📅 Payment integration
- 📅 Social features and ratings

---

**🎯 Ready to build amazing mobile experiences with ResellPanda!**

Choose the documentation section that matches your current development needs and start building your Android application with confidence.

---

**Contact & Support:**  
For technical questions or issues, refer to the troubleshooting sections in each documentation file or contact the development team.

**Happy Coding! 🚀**