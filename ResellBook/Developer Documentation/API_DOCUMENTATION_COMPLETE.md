# ResellPanda API Documentation

## 📋 **Table of Contents**

1. [Authentication APIs](#authentication-apis)
2. [Books Management APIs](#books-management-apis)
3. [Chat & Messaging APIs](#chat--messaging-apis)
4. [User Location APIs](#user-location-apis)
5. [User Profile APIs](#user-profile-apis)
6. [System APIs](#system-apis)
7. [Error Handling](#error-handling)
8. [Android Kotlin Integration Examples](#android-kotlin-integration-examples)

---

## 🔐 **Authentication APIs**

### **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/Auth`

#### **1. User Signup**
- **Endpoint:** `POST /api/Auth/signup`
- **Purpose:** Register a new user account
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "Name": "John Doe",
  "Email": "john.doe@example.com", 
  "Password": "password123"
}
```

**Validation Rules:**
- Name: Required, not empty
- Email: Required, not empty, must be unique
- Password: Required, minimum 4 characters

**Success Response (200):**
```json
"Signup successful. Check your email for OTP."
```

**Error Responses (400):**
```json
"Name is required."
"Email is required."
"Password is required."
"Password must be at least 4 characters."
"Email already exists."
```

**Android Kotlin Example:**
```kotlin
data class SignupRequest(
    val Name: String,
    val Email: String,
    val Password: String
)

// Retrofit interface
@POST("api/Auth/signup")
suspend fun signup(@Body request: SignupRequest): Response<String>

// Usage
val request = SignupRequest("John Doe", "john@example.com", "password123")
val response = authApi.signup(request)
```

---

#### **2. Verify Email**
- **Endpoint:** `POST /api/Auth/verify-email`
- **Purpose:** Verify user email using OTP
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "Email": "john.doe@example.com",
  "Code": "123456"
}
```

**Validation Rules:**
- Email: Required, not empty
- Code: Required, exactly 6 digits, numeric only

**Success Response (200):**
```json
"Email verified successfully!"
```

**Error Responses (400):**
```json
"Email is required."
"OTP code is required."
"OTP code must be 6 digits."
"OTP code must contain only digits."
"Invalid email."
"Already verified."
"Invalid OTP."
"Expired OTP."
```

**Android Kotlin Example:**
```kotlin
data class VerifyEmailRequest(
    val Email: String,
    val Code: String
)

@POST("api/Auth/verify-email")
suspend fun verifyEmail(@Body request: VerifyEmailRequest): Response<String>
```

---

#### **3. User Login**
- **Endpoint:** `POST /api/Auth/login`
- **Purpose:** Authenticate user and receive JWT token
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "Email": "john.doe@example.com",
  "Password": "password123"
}
```

**Success Response (200):**
```json
{
  "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Error Responses (400):**
```json
"Invalid credentials."
"Email not verified."
```

**Android Kotlin Example:**
```kotlin
data class LoginRequest(
    val Email: String,
    val Password: String
)

data class LoginResponse(
    val Token: String
)

@POST("api/Auth/login")
suspend fun login(@Body request: LoginRequest): Response<LoginResponse>
```

---

#### **4. Resend OTP**
- **Endpoint:** `POST /api/Auth/resend-otp`
- **Purpose:** Resend email verification OTP
- **Content-Type:** `application/json`

**Request Body:**
```json
"john.doe@example.com"
```

**Success Response (200):**
```json
"OTP resent. Check your email."
```

**Error Responses (400):**
```json
"Email is required."
"Email not found."
"Email already verified."
```

**Android Kotlin Example:**
```kotlin
@POST("api/Auth/resend-otp")
suspend fun resendOtp(@Body email: String): Response<String>
```

---

#### **5. Forgot Password**
- **Endpoint:** `POST /api/Auth/forgot-password`
- **Purpose:** Request password reset OTP
- **Content-Type:** `application/json`

**Request Body:**
```json
"john.doe@example.com"
```

**Success Response (200):**
```json
"Password reset OTP sent to your email."
```

**Error Responses (400):**
```json
"Email not found."
```

---

#### **6. Verify Reset OTP**
- **Endpoint:** `POST /api/Auth/verify-reset-otp`
- **Purpose:** Verify password reset OTP
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "Email": "john.doe@example.com",
  "Code": "123456"
}
```

**Success Response (200):**
```json
"OTP verified successfully. You can now reset your password."
```

**Error Responses (400):**
```json
"Email is required."
"OTP code is required."
"OTP code must be 6 digits."
"Invalid email."
"Invalid OTP."
"Expired OTP."
```

---

#### **7. Reset Password**
- **Endpoint:** `POST /api/Auth/reset-password`
- **Purpose:** Reset user password after OTP verification
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "Email": "john.doe@example.com",
  "NewPassword": "newPassword123"
}
```

**Validation Rules:**
- Email: Required, not empty
- NewPassword: Required, minimum 6 characters

**Success Response (200):**
```json
"Password reset successfully!"
```

**Error Responses (400):**
```json
"Email is required."
"New password is required."
"Password must be at least 6 characters."
"Invalid email."
"OTP not verified."
```

**Android Kotlin Example:**
```kotlin
data class ResetPasswordRequest(
    val Email: String,
    val NewPassword: String
)

@POST("api/Auth/reset-password")
suspend fun resetPassword(@Body request: ResetPasswordRequest): Response<String>
```

---

## 📚 **Books Management APIs**

### **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/Books`

#### **1. List New Book**
- **Endpoint:** `POST /api/Books/ListBook`
- **Purpose:** Create a new book listing with images
- **Content-Type:** `multipart/form-data`
- **Authentication:** JWT Token required

**Form Data Parameters:**
```
UserId: "123e4567-e89b-12d3-a456-426614174000" (GUID)
BookName: "Data Structures and Algorithms" (Required)
AuthorOrPublication: "Robert Lafore" (Optional)
Category: "Computer Science" (Required)
SubCategory: "Programming" (Optional)
SellingPrice: 299.99 (Required, Decimal)
Images: File[] (Required, 2-4 images, multipart files)
```

**Validation Rules:**
- UserId: Must exist in Users table
- BookName: Required, not empty
- Category: Required, not empty
- SellingPrice: Required, must be positive decimal
- Images: Required, minimum 2 files, maximum 4 files

**Success Response (200):**
```json
{
  "Message": "Book listed successfully",
  "BookId": "789e4567-e89b-12d3-a456-426614174000"
}
```

**Error Responses:**
```json
// 404
{"Message": "User not found"}

// 400
{"Message": "You must upload between 2 and 4 images."}
```

**Android Kotlin Example:**
```kotlin
// Create multipart request
val userIdPart = MultipartBody.Part.createFormData("UserId", userId)
val bookNamePart = MultipartBody.Part.createFormData("BookName", bookName)
val categoryPart = MultipartBody.Part.createFormData("Category", category)
val pricePart = MultipartBody.Part.createFormData("SellingPrice", price.toString())

// Convert images to RequestBody
val imageParts = images.map { file ->
    val requestFile = RequestBody.create(MediaType.parse("image/*"), file)
    MultipartBody.Part.createFormData("Images", file.name, requestFile)
}

@Multipart
@POST("api/Books/ListBook")
suspend fun listBook(
    @Part userId: MultipartBody.Part,
    @Part bookName: MultipartBody.Part,
    @Part category: MultipartBody.Part,
    @Part sellingPrice: MultipartBody.Part,
    @Part authorOrPublication: MultipartBody.Part? = null,
    @Part subCategory: MultipartBody.Part? = null,
    @Part images: List<MultipartBody.Part>
): Response<BookListResponse>
```

---

#### **2. Edit Book Listing**
- **Endpoint:** `PUT /api/Books/EditListing/{id}`
- **Purpose:** Update existing book listing
- **Content-Type:** `multipart/form-data`
- **Authentication:** JWT Token required

**URL Parameter:**
- `id`: Book ID (GUID)

**Form Data Parameters (All Optional):**
```
BookName: "Updated Book Name"
AuthorOrPublication: "Updated Author"
Category: "Updated Category"
SubCategory: "Updated SubCategory"
SellingPrice: 399.99
NewImages: File[] (New images to add)
ExistingImages: string[] (Paths of existing images to keep)
```

**Validation Rules:**
- Final image count must be 2-4 after update
- At least 2 images must remain after removing existing images

**Success Response (200):**
```json
{"Message": "Book updated successfully"}
```

**Error Responses:**
```json
// 404
{"Message": "Book not found"}

// 400
{"Message": "Each book must have at least 2 images."}
{"Message": "Maximum 4 images allowed."}
```

---

#### **3. View All Books**
- **Endpoint:** `GET /api/Books/ViewAll/userId`
- **Purpose:** Retrieve all book listings
- **Authentication:** None required

**Success Response (200):**
```json
[
  {
    "Id": "789e4567-e89b-12d3-a456-426614174000",
    "BookName": "Data Structures and Algorithms",
    "AuthorOrPublication": "Robert Lafore",
    "Category": "Computer Science",
    "SubCategory": "Programming",
    "SellingPrice": 299.99,
    "Images": [
      "uploads/books/image1.jpg",
      "uploads/books/image2.jpg"
    ],
    "CreatedAt": "2025-09-30T10:30:00Z",
    "distance" : "100m"
  }
]
```

**Android Kotlin Example:**
```kotlin
data class Book(
    val Id: String,
    val BookName: String,
    val AuthorOrPublication: String?,
    val Category: String,
    val SubCategory: String?,
    val SellingPrice: Double,
    val Images: List<String>,
    val CreatedAt: String,
    val Distance: Double
)

@GET("api/Books/ViewAll")
suspend fun getAllBooks(): Response<List<Book>>
```

---

## � **Chat & Messaging APIs**

### **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/Chat`

#### **1. Send Message to Book Owner**
- **Endpoint:** `POST /api/Chat/SendMessage/{senderId}`
- **Purpose:** Send message directly to book owner using BookId
- **Content-Type:** `application/json`
- **✨ Updated:** Now uses BookId instead of ReceiverId for easier integration

**URL Parameters:**
- `senderId`: ID of user sending the message (GUID)

**Request Body:**
```json
{
  "BookId": "123e4567-e89b-12d3-a456-426614174000",
  "Message": "Hi! Is this book still available?"
}
```

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Message sent successfully",
  "ChatMessage": {
    "Id": "789e4567-e89b-12d3-a456-426614174000",
    "SenderId": "456e4567-e89b-12d3-a456-426614174000",
    "ReceiverId": "123e4567-e89b-12d3-a456-426614174000",
    "SenderName": "John Doe",
    "ReceiverName": "Jane Smith",
    "Message": "Hi! Is this book still available?",
    "SentAt": "2025-10-04T13:45:00Z",
    "IsRead": false,
    "IsSentByMe": true
  },
  "BookContext": {
    "BookId": "123e4567-e89b-12d3-a456-426614174000",
    "BookName": "Advanced Mathematics",
    "BookOwnerName": "Jane Smith",
    "SellingPrice": 450.00
  }
}
```

**Android Kotlin Example:**
```kotlin
data class SendMessageRequest(
    val BookId: String,
    val Message: String
)

data class BookContext(
    val BookId: String,
    val BookName: String,
    val BookOwnerName: String,
    val SellingPrice: Double
)

data class ChatMessage(
    val Id: String,
    val SenderId: String,
    val ReceiverId: String,
    val SenderName: String,
    val ReceiverName: String,
    val Message: String,
    val SentAt: String,
    val IsRead: Boolean,
    val IsSentByMe: Boolean
)

data class SendMessageResponse(
    val Success: Boolean,
    val Message: String,
    val ChatMessage: ChatMessage?,
    val BookContext: BookContext?
)

@POST("api/Chat/SendMessage/{senderId}")
suspend fun sendMessageToBookOwner(
    @Path("senderId") senderId: String,
    @Body request: SendMessageRequest
): Response<SendMessageResponse>
```

---

#### **2. Get Chat List**
- **Endpoint:** `GET /api/Chat/GetChats/{userId}`
- **Purpose:** Get all chat conversations for user with unread counts

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Chats retrieved successfully",
  "Chats": [
    {
      "UserId": "123e4567-e89b-12d3-a456-426614174000",
      "UserName": "Jane Smith",
      "LastMessage": "Sure, it's available!",
      "LastMessageTime": "2025-10-04T14:30:00Z",
      "UnreadCount": 2,
      "IsOnline": false
    }
  ]
}
```

---

#### **3. Get Chat Messages**
- **Endpoint:** `GET /api/Chat/GetChatMessages/{userId}/{otherUserId}?page=1`
- **Purpose:** Get message history between two users

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Messages retrieved successfully",
  "Messages": [
    {
      "Id": "789e4567-e89b-12d3-a456-426614174000",
      "SenderId": "456e4567-e89b-12d3-a456-426614174000",
      "ReceiverId": "123e4567-e89b-12d3-a456-426614174000",
      "SenderName": "John Doe",
      "ReceiverName": "Jane Smith",
      "Message": "Hi! Is this book still available?",
      "SentAt": "2025-10-04T13:45:00Z",
      "IsRead": true,
      "IsSentByMe": true
    }
  ]
}
```

---

#### **4. Mark Messages as Read**
- **Endpoint:** `POST /api/Chat/MarkAsRead/{userId}`
- **Purpose:** Mark all messages from another user as read

**Request Body:**
```json
{
  "OtherUserId": "123e4567-e89b-12d3-a456-426614174000"
}
```

---

#### **5. Get Unread Count**
- **Endpoint:** `GET /api/Chat/GetUnreadCount/{userId}`
- **Purpose:** Get total unread message count for user

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Unread count retrieved successfully",
  "UnreadCount": 5
}
```

---

#### **6. Delete Single Message**
- **Endpoint:** `DELETE /api/Chat/DeleteMessage/{messageId}/{userId}`
- **Purpose:** Delete a specific message (sender only)

---

#### **7. Delete Chat from Your View (UPDATED)**
- **Endpoint:** `DELETE /api/Chat/DeleteChat/{userId}/{otherUserId}`
- **Purpose:** ✨ Delete chat from YOUR view only (not from other user's view)
- **Note:** Uses soft deletion - messages are hidden from your view but remain for the other user

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Chat deleted from your view",
  "DeletedMessagesCount": 25,
  "DeletedBetween": {
    "User1Name": "John Doe",
    "User2Name": "Jane Smith"
  }
}
```

**Android Kotlin Example:**
```kotlin
data class DeleteChatResponse(
    val Success: Boolean,
    val Message: String,
    val DeletedMessagesCount: Int,
    val DeletedBetween: ChatParticipants?
)

@DELETE("api/Chat/DeleteChat/{userId}/{otherUserId}")
suspend fun deleteChat(
    @Path("userId") userId: String,
    @Path("otherUserId") otherUserId: String
): Response<DeleteChatResponse>
```

---

#### **8. Block User (NEW)**
- **Endpoint:** `POST /api/Chat/BlockUser/{userId}`
- **Purpose:** Block a user from sending messages to you
- **Content-Type:** `application/json`

**Request Body:**
```json
{
  "UserIdToBlock": "123e4567-e89b-12d3-a456-426614174000",
  "Reason": "Spam/Inappropriate behavior" // Optional
}
```

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "User blocked successfully",
  "BlockedUserName": "John Doe",
  "BlockedAt": "2025-10-05T10:30:00Z"
}
```

**Android Kotlin Example:**
```kotlin
data class BlockUserRequest(
    val UserIdToBlock: String,
    val Reason: String? = null
)

data class BlockUserResponse(
    val Success: Boolean,
    val Message: String,
    val BlockedUserName: String?,
    val BlockedAt: String?
)

@POST("api/Chat/BlockUser/{userId}")
suspend fun blockUser(
    @Path("userId") userId: String,
    @Body request: BlockUserRequest
): Response<BlockUserResponse>
```

---

#### **9. Unblock User (NEW)**
- **Endpoint:** `DELETE /api/Chat/UnblockUser/{userId}/{blockedUserId}`
- **Purpose:** Unblock a previously blocked user

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "User unblocked successfully",
  "UnblockedUserName": "John Doe"
}
```

**Android Kotlin Example:**
```kotlin
data class UnblockUserResponse(
    val Success: Boolean,
    val Message: String,
    val UnblockedUserName: String?
)

@DELETE("api/Chat/UnblockUser/{userId}/{blockedUserId}")
suspend fun unblockUser(
    @Path("userId") userId: String,
    @Path("blockedUserId") blockedUserId: String
): Response<UnblockUserResponse>
```

---

#### **10. Get Blocked Users (NEW)**
- **Endpoint:** `GET /api/Chat/GetBlockedUsers/{userId}`
- **Purpose:** Get list of all blocked users

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Blocked users retrieved successfully",
  "BlockedUsers": [
    {
      "UserId": "123e4567-e89b-12d3-a456-426614174000",
      "UserName": "John Doe",
      "UserEmail": "john@example.com",
      "BlockedAt": "2025-10-05T10:30:00Z",
      "Reason": "Spam"
    }
  ],
  "TotalCount": 1
}
```

**Android Kotlin Example:**
```kotlin
data class BlockedUserDto(
    val UserId: String,
    val UserName: String,
    val UserEmail: String?,
    val BlockedAt: String,
    val Reason: String?
)

data class BlockedUsersResponse(
    val Success: Boolean,
    val Message: String,
    val BlockedUsers: List<BlockedUserDto>,
    val TotalCount: Int
)

@GET("api/Chat/GetBlockedUsers/{userId}")
suspend fun getBlockedUsers(
    @Path("userId") userId: String
): Response<BlockedUsersResponse>
```

---

#### **11. Get Book for Messaging Context (Helper)**
- **Endpoint:** `GET /api/Chat/GetBookForMessage/{bookId}`
- **Purpose:** Get book details to show messaging context

**Success Response (200):**
```json
{
  "BookId": "123e4567-e89b-12d3-a456-426614174000",
  "BookName": "Advanced Mathematics",
  "BookOwnerName": "Jane Smith",
  "SellingPrice": 450.00
}
```

---

## �📍 **User Location APIs**

### **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/UserLocation`

#### **1. Sync User Location**
- **Endpoint:** `POST /api/UserLocation/SyncLocation`
- **Purpose:** Save user's current location
- **Content-Type:** `application/json`
- **Authentication:** JWT Token required

**Request Body:**
```json
{
  "UserId": "123e4567-e89b-12d3-a456-426614174000",
  "Latitude": 40.7128,
  "Longitude": -74.0060
}
```

**Validation Rules:**
- UserId: Must exist in Users table
- Latitude: Required, valid double (-90 to 90)
- Longitude: Required, valid double (-180 to 180)

**Success Response (200):**
```json
{"Message": "Location synced successfully"}
```

**Error Responses:**
```json
// 404
{"Message": "User not found"}
```

**Android Kotlin Example:**
```kotlin
data class LocationRequest(
    val UserId: String,
    val Latitude: Double,
    val Longitude: Double
)

data class LocationResponse(
    val Message: String
)

@POST("api/UserLocation/SyncLocation")
suspend fun syncLocation(@Body request: LocationRequest): Response<LocationResponse>

// Usage with GPS
fun syncCurrentLocation() {
    fusedLocationClient.lastLocation.addOnSuccessListener { location ->
        if (location != null) {
            val request = LocationRequest(
                UserId = getCurrentUserId(),
                Latitude = location.latitude,
                Longitude = location.longitude
            )
            // Make API call
        }
    }
}
```

---

#### **2. Get User Locations**
- **Endpoint:** `GET /api/UserLocation/GetLocations/{userId}`
- **Purpose:** Retrieve location history for a user
- **Authentication:** JWT Token required

**URL Parameter:**
- `userId`: User ID (GUID)

**Success Response (200):**
```json
[
  {
    "Id": "456e4567-e89b-12d3-a456-426614174000",
    "UserId": "123e4567-e89b-12d3-a456-426614174000",
    "Latitude": 40.7128,
    "Longitude": -74.0060,
    "CreateDate": "2025-09-30T10:30:00Z"
  }
]
```

**Error Responses:**
```json
// 404
{"Message": "No locations found for this user"}
```

**Android Kotlin Example:**
```kotlin
data class UserLocation(
    val Id: String,
    val UserId: String,
    val Latitude: Double,
    val Longitude: Double,
    val CreateDate: String
)

@GET("api/UserLocation/GetLocations/{userId}")
suspend fun getUserLocations(@Path("userId") userId: String): Response<List<UserLocation>>
```

---

## 👤 **User Profile APIs**

#### **Get User Profile**
- **Endpoint:** `GET /api/UserLocation/profile/{userId}`
- **Purpose:** Retrieve user profile information
- **Authentication:** JWT Token required

**URL Parameter:**
- `userId`: User ID (GUID)

**Success Response (200):**
```json
{
  "Id": "123e4567-e89b-12d3-a456-426614174000",
  "Name": "John Doe",
  "Email": "john.doe@example.com",
  "IsEmailVerified": true
}
```

**Error Responses:**
```json
// 404
"User not found."
```

**Android Kotlin Example:**
```kotlin
data class UserProfile(
    val Id: String,
    val Name: String,
    val Email: String,
    val IsEmailVerified: Boolean
)

@GET("api/UserLocation/profile/{userId}")
suspend fun getUserProfile(@Path("userId") userId: String): Response<UserProfile>
```

---

## 🌤️ **System APIs**

#### **Weather Forecast (Test Endpoint)**
- **Endpoint:** `GET /weatherforecast`
- **Purpose:** Test API connectivity
- **Authentication:** None required

**Success Response (200):**
```json
[
  {
    "date": "2025-10-01",
    "temperatureC": 25,
    "temperatureF": 77,
    "summary": "Warm"
  }
]
```

---

## ⚠️ **Error Handling**

### **HTTP Status Codes:**
- `200 OK`: Request successful
- `400 Bad Request`: Invalid request data or validation errors
- `401 Unauthorized`: Missing or invalid JWT token
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server-side error

### **Error Response Format:**
```json
// Simple string message
"Error message here"

// Object format
{
  "Message": "Detailed error message"
}
```

---

## 🤖 **Android Kotlin Integration Examples**

### **1. Setup Retrofit**
```kotlin
// build.gradle (Module: app)
implementation 'com.squareup.retrofit2:retrofit:2.9.0'
implementation 'com.squareup.retrofit2:converter-gson:2.9.0'
implementation 'com.squareup.okhttp3:logging-interceptor:4.9.0'

// ApiClient.kt
object ApiClient {
    private const val BASE_URL = "https://resellbook20250929183655.azurewebsites.net/"
    
    private val okHttpClient = OkHttpClient.Builder()
        .addInterceptor(HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        })
        .addInterceptor { chain ->
            val token = getAuthToken() // Your token storage logic
            val request = chain.request().newBuilder()
                .apply {
                    if (!token.isNullOrEmpty()) {
                        addHeader("Authorization", "Bearer $token")
                    }
                }
                .build()
            chain.proceed(request)
        }
        .build()

    val retrofit: Retrofit = Retrofit.Builder()
        .baseUrl(BASE_URL)
        .client(okHttpClient)
        .addConverterFactory(GsonConverterFactory.create())
        .build()
}
```

### **2. API Interfaces**
```kotlin
// AuthApi.kt
interface AuthApi {
    @POST("api/Auth/signup")
    suspend fun signup(@Body request: SignupRequest): Response<String>
    
    @POST("api/Auth/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>
    
    @POST("api/Auth/verify-email")
    suspend fun verifyEmail(@Body request: VerifyEmailRequest): Response<String>
}

// BooksApi.kt
interface BooksApi {
    @Multipart
    @POST("api/Books/ListBook")
    suspend fun listBook(
        @Part("UserId") userId: RequestBody,
        @Part("BookName") bookName: RequestBody,
        @Part("Category") category: RequestBody,
        @Part("SellingPrice") sellingPrice: RequestBody,
        @Part images: List<MultipartBody.Part>
    ): Response<BookListResponse>
    
    @GET("api/Books/ViewAll")
    suspend fun getAllBooks(): Response<List<Book>>
}
```

### **3. Repository Pattern**
```kotlin
class AuthRepository {
    private val authApi = ApiClient.retrofit.create(AuthApi::class.java)
    
    suspend fun signup(name: String, email: String, password: String): Result<String> {
        return try {
            val request = SignupRequest(name, email, password)
            val response = authApi.signup(request)
            if (response.isSuccessful) {
                Result.success(response.body() ?: "Success")
            } else {
                Result.failure(Exception(response.errorBody()?.string() ?: "Unknown error"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
```

### **4. ViewModel Usage**
```kotlin
class AuthViewModel(private val repository: AuthRepository) : ViewModel() {
    private val _signupResult = MutableLiveData<Result<String>>()
    val signupResult: LiveData<Result<String>> = _signupResult
    
    fun signup(name: String, email: String, password: String) {
        viewModelScope.launch {
            _signupResult.value = repository.signup(name, email, password)
        }
    }
}
```

### **5. Image Upload Helper**
```kotlin
fun createImagePart(imageUri: Uri, context: Context): MultipartBody.Part {
    val inputStream = context.contentResolver.openInputStream(imageUri)
    val file = File(context.cacheDir, "temp_image_${System.currentTimeMillis()}.jpg")
    file.outputStream().use { inputStream?.copyTo(it) }
    
    val requestBody = RequestBody.create(MediaType.parse("image/*"), file)
    return MultipartBody.Part.createFormData("Images", file.name, requestBody)
}
```

---

## 📝 **Important Notes**

1. **JWT Token Management**: Store JWT tokens securely and include in Authorization header for protected endpoints
2. **Image Handling**: Books require 2-4 images, use multipart/form-data for image uploads
3. **OTP Expiry**: All OTPs expire after 10 minutes
4. **Location Precision**: Use GPS coordinates with appropriate precision for location APIs
5. **Error Handling**: Always check response status and handle errors appropriately
6. **Validation**: Client-side validation should match server-side rules for better UX

---

**Last Updated:** September 30, 2025  
**API Version:** v1.0  
**Base URL:** `https://resellbook20250929183655.azurewebsites.net`