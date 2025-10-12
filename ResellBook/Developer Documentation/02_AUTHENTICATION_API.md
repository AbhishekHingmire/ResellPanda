# 🔐 Authentication APIs
### 🎓 **Complete Guide to User Authentication, Security, and JWT Tokens**

> **📚 Prerequisites:** Read [Foundations & Core Concepts](00_FOUNDATIONS_AND_CONCEPTS.md) first  
> **⏰ Learning Time:** 45 minutes to understand authentication completely  
> **🎯 End Knowledge:** Professional understanding of web security and user management  

## **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/Auth`

---

## 🧠 **Authentication Fundamentals - What You Need to Know**

### **🤔 Why Do We Need Authentication?**

**The Security Problem:**
```
Without Authentication:
├── Anyone can access any user's data
├── No way to know who made changes
├── No privacy or data protection
└── Apps become unusable for real users
```

**The Authentication Solution:**
```
With Authentication System:
├── Users prove their identity (login)
├── System issues security token (JWT)
├── Token proves identity for future requests
├── Each user sees only their own data
└── System tracks who does what
```

### **🔐 How Our Authentication Flow Works**

**📋 Complete User Journey:**
```
1. Signup → 2. Email Verification → 3. Login → 4. Get JWT Token → 5. Access Protected APIs

Step 1: User Registration
├── User provides name, email, password
├── System validates data (email format, password strength)
├── Password is hashed for security (never stored in plain text)
├── Email verification code (OTP) is sent
└── User account created but not verified

Step 2: Email Verification  
├── User receives 6-digit code via email
├── User enters code in app
├── System verifies code matches and hasn't expired
├── Account becomes active and verified
└── User can now login

Step 3: Login Process
├── User provides email and password
├── System finds user by email
├── System verifies password using hash comparison
├── If valid, JWT token is generated and returned
└── User stores token for future API calls

Step 4: Protected API Access
├── User includes token in Authorization header
├── System validates token signature and expiration
├── If valid, user ID is extracted from token
├── API request proceeds with authenticated user context
└── User can access their personal data securely
```

### **🛡️ Security Measures Implemented**

**🔐 Password Security:**
- **Hashing:** Passwords encrypted using BCrypt with salt
- **One-way encryption:** Even developers can't see original passwords
- **Secure verification:** Login compares hashed versions, not plain text

**📧 Email Verification:**
- **OTP (One-Time Password):** 6-digit numeric code
- **Time-limited:** Expires after 10 minutes
- **Single use:** Code becomes invalid after successful verification
- **Database tracking:** System knows which emails are verified

**🎫 JWT Token Security:**
- **Signed tokens:** Server signature prevents tampering
- **Expiration:** Tokens automatically expire (24 hours)
- **Stateless:** No server-side session storage needed
- **User context:** Token contains user ID and permissions

---

## **API Endpoints - Complete Technical Reference**

### **1. User Signup** 
**`POST /api/Auth/signup`**

#### **🎯 Purpose & Business Logic**
Register a new user account and initiate email verification process. This is the first step in user onboarding.

#### **🔄 What Happens Behind the Scenes**
```
User Input → Validation → Password Hashing → Database Storage → OTP Generation → Email Sending

1. Data Validation:
   ├── Check all required fields are present
   ├── Validate email format (contains @ and valid domain)
   ├── Check password meets minimum requirements
   └── Verify email doesn't already exist in database

2. Security Processing:
   ├── Hash password using BCrypt (irreversible encryption)
   ├── Generate 6-digit random OTP code
   ├── Set OTP expiration time (10 minutes from now)
   └── Store all data securely in database

3. Communication:
   ├── Send OTP via email using MailKit service
   ├── Email contains verification code and instructions
   ├── Return success message to user
   └── Log signup attempt for monitoring
```

#### **📝 Request Format**
```json
{
  "Name": "John Doe",                    // Display name for user profile
  "Email": "john.doe@example.com",       // Unique identifier for login
  "Password": "password123"              // Plain text (will be hashed)
}
```

#### **🔍 Server-Side Validation Rules**
```csharp
// Validation Logic in AuthController
if (string.IsNullOrWhiteSpace(request.Name))
    return BadRequest("Name is required.");

if (string.IsNullOrWhiteSpace(request.Email))
    return BadRequest("Email is required.");

if (!IsValidEmail(request.Email))
    return BadRequest("Invalid email format.");

if (string.IsNullOrWhiteSpace(request.Password))
    return BadRequest("Password is required.");

if (request.Password.Length < 4)
    return BadRequest("Password must be at least 4 characters.");

if (await _userService.EmailExistsAsync(request.Email))
    return BadRequest("Email already exists.");
```

#### **🔐 Password Hashing Process**
```csharp
// How passwords are secured (never stored in plain text)
var plainTextPassword = "password123";                    // User input
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
// Result: "$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy"

// Later during login verification:
var loginPassword = "password123";                        // User login input
var storedHashedPassword = "$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";
var isValid = BCrypt.Net.BCrypt.Verify(loginPassword, storedHashedPassword); // true
```

#### **📊 Success Response (200)**
```json
"Signup successful. Check your email for OTP."
```
**What this means:**
- ✅ User account created in database
- ✅ Password securely hashed and stored
- ✅ OTP generated and emailed
- ✅ Account is pending email verification
- ⏳ User must verify email before login

#### **❌ Error Responses (400 Bad Request)**
```json
// Missing or invalid data
"Name is required."                    // Empty or null name
"Email is required."                   // Empty or null email
"Invalid email format."                // Email doesn't contain @ or valid domain
"Password is required."                // Empty or null password
"Password must be at least 4 characters." // Password too short
"Email already exists."                // Another user already registered with this email
```

#### **🎓 Understanding Error Handling**
```csharp
// Why we validate on server even if mobile app validates too:

// 1. Security: Never trust client-side validation
// 2. API can be called from anywhere (not just your mobile app)
// 3. Different clients might have different validation rules
// 4. Server is the authoritative source of business rules
```

**Android Kotlin:**
```kotlin
data class SignupRequest(
    val Name: String,
    val Email: String,
    val Password: String
)

@POST("api/Auth/signup")
suspend fun signup(@Body request: SignupRequest): Response<String>

// Usage
val request = SignupRequest("John Doe", "john@example.com", "password123")
try {
    val response = authApi.signup(request)
    if (response.isSuccessful) {
        // Navigate to OTP verification screen
        showMessage(response.body())
    } else {
        showError(response.errorBody()?.string())
    }
} catch (e: Exception) {
    showError("Network error: ${e.message}")
}
```

---

### **2. Verify Email** 
**`POST /api/Auth/verify-email`**

**Purpose:** Verify user email using 6-digit OTP sent to email

**Request:**
```json
{
  "Email": "john.doe@example.com",
  "Code": "123456"
}
```

**Validation:**
- ✅ Email: Required, not empty
- ✅ Code: Required, exactly 6 digits, numeric only
- ✅ OTP expires after 10 minutes

**Success Response (200):**
```json
"Email verified successfully!"
```

**Error Responses (400):**
- `"Email is required."`
- `"OTP code is required."`
- `"OTP code must be 6 digits."`
- `"OTP code must contain only digits."`
- `"Invalid email."`
- `"Already verified."`
- `"Invalid OTP."`
- `"Expired OTP."`

**Android Kotlin:**
```kotlin
data class VerifyEmailRequest(
    val Email: String,
    val Code: String
)

@POST("api/Auth/verify-email")
suspend fun verifyEmail(@Body request: VerifyEmailRequest): Response<String>

// Usage with input validation
fun verifyOtp(email: String, otp: String) {
    if (otp.length != 6 || !otp.all { it.isDigit() }) {
        showError("OTP must be exactly 6 digits")
        return
    }
    
    viewModelScope.launch {
        try {
            val request = VerifyEmailRequest(email, otp)
            val response = authApi.verifyEmail(request)
            if (response.isSuccessful) {
                // Email verified, proceed to login or main app
                navigateToLogin()
            } else {
                showError(response.errorBody()?.string())
            }
        } catch (e: Exception) {
            showError("Verification failed: ${e.message}")
        }
    }
}
```

---

### **3. User Login** 
**`POST /api/Auth/login`**

**Purpose:** Authenticate user and receive JWT token for API access

**Request:**
```json
{
  "Email": "john.doe@example.com",
  "Password": "password123"
}
```

**Success Response (200):**
```json
{
  "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwidXNlcklkIjoiMTIzZTQ1NjctZTg5Yi0xMmQzLWE0NTYtNDI2NjE0MTc0MDAwIiwibmJmIjoxNjk2MDc0MDAwLCJleHAiOjE2OTYxNjA0MDAsImlhdCI6MTY5NjA3NDAwMH0.signature"
}
```

**Error Responses (400):**
- `"Invalid credentials."` (Wrong email/password)
- `"Email not verified."` (Must verify email first)

**Android Kotlin:**
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

// Usage with token storage
fun login(email: String, password: String) {
    viewModelScope.launch {
        try {
            val request = LoginRequest(email, password)
            val response = authApi.login(request)
            if (response.isSuccessful) {
                val token = response.body()?.Token
                if (!token.isNullOrEmpty()) {
                    // Store token securely
                    saveAuthToken(token)
                    navigateToMainActivity()
                } else {
                    showError("Invalid response format")
                }
            } else {
                showError(response.errorBody()?.string())
            }
        } catch (e: Exception) {
            showError("Login failed: ${e.message}")
        }
    }
}

// Token storage helper
private fun saveAuthToken(token: String) {
    val sharedPrefs = getSharedPreferences("auth", Context.MODE_PRIVATE)
    sharedPrefs.edit().putString("jwt_token", token).apply()
}
```

---

### **4. Resend OTP** 
**`POST /api/Auth/resend-otp`**

**Purpose:** Resend email verification OTP (invalidates previous OTP)

**Request:**
```json
"john.doe@example.com"
```

**Success Response (200):**
```json
"OTP resent. Check your email."
```

**Error Responses (400):**
- `"Email is required."`
- `"Email not found."`
- `"Email already verified."`

**Android Kotlin:**
```kotlin
@POST("api/Auth/resend-otp")
suspend fun resendOtp(@Body email: String): Response<String>

// Usage with cooldown timer
fun resendOtp(email: String) {
    if (isResendOnCooldown()) {
        showError("Please wait before requesting another OTP")
        return
    }
    
    viewModelScope.launch {
        try {
            val response = authApi.resendOtp(email)
            if (response.isSuccessful) {
                showMessage(response.body())
                startResendCooldown() // 30-60 seconds cooldown
            } else {
                showError(response.errorBody()?.string())
            }
        } catch (e: Exception) {
            showError("Failed to resend OTP: ${e.message}")
        }
    }
}
```

---

### **5. Forgot Password** 
**`POST /api/Auth/forgot-password`**

**Purpose:** Request password reset OTP sent to email

**Request:**
```json
"john.doe@example.com"
```

**Success Response (200):**
```json
"Password reset OTP sent to your email."
```

**Error Responses (400):**
- `"Email not found."`

**Android Kotlin:**
```kotlin
@POST("api/Auth/forgot-password")
suspend fun forgotPassword(@Body email: String): Response<String>

// Usage
fun initiatePasswordReset(email: String) {
    viewModelScope.launch {
        try {
            val response = authApi.forgotPassword(email)
            if (response.isSuccessful) {
                showMessage(response.body())
                navigateToResetOtpScreen(email)
            } else {
                showError(response.errorBody()?.string())
            }
        } catch (e: Exception) {
            showError("Request failed: ${e.message}")
        }
    }
}
```

---

### **6. Verify Reset OTP** 
**`POST /api/Auth/verify-reset-otp`**

**Purpose:** Verify password reset OTP before allowing password change

**Request:**
```json
{
  "Email": "john.doe@example.com",
  "Code": "123456"
}
```

**Validation:**
- ✅ Same validation as verify-email
- ✅ OTP expires after 10 minutes

**Success Response (200):**
```json
"OTP verified successfully. You can now reset your password."
```

**Error Responses (400):**
- Same errors as verify-email endpoint

**Android Kotlin:**
```kotlin
@POST("api/Auth/verify-reset-otp")
suspend fun verifyResetOtp(@Body request: VerifyEmailRequest): Response<String>

// Usage - same pattern as email verification
fun verifyResetOtp(email: String, otp: String) {
    // Same validation and error handling as verifyOtp()
    // On success, navigate to new password screen
}
```

---

### **7. Reset Password** 
**`POST /api/Auth/reset-password`**

**Purpose:** Set new password after OTP verification

**Request:**
```json
{
  "Email": "john.doe@example.com",
  "NewPassword": "newSecurePassword123"
}
```

**Validation:**
- ✅ Email: Required, not empty
- ✅ NewPassword: Required, minimum 6 characters
- ✅ Must have verified reset OTP first

**Success Response (200):**
```json
"Password reset successfully!"
```

**Error Responses (400):**
- `"Email is required."`
- `"New password is required."`
- `"Password must be at least 6 characters."`
- `"Invalid email."`
- `"OTP not verified."`

**Android Kotlin:**
```kotlin
data class ResetPasswordRequest(
    val Email: String,
    val NewPassword: String
)

@POST("api/Auth/reset-password")
suspend fun resetPassword(@Body request: ResetPasswordRequest): Response<String>

// Usage with password strength validation
fun resetPassword(email: String, newPassword: String, confirmPassword: String) {
    // Client-side validation
    if (newPassword.length < 6) {
        showError("Password must be at least 6 characters")
        return
    }
    if (newPassword != confirmPassword) {
        showError("Passwords do not match")
        return
    }
    
    viewModelScope.launch {
        try {
            val request = ResetPasswordRequest(email, newPassword)
            val response = authApi.resetPassword(request)
            if (response.isSuccessful) {
                showMessage("Password reset successfully")
                navigateToLogin()
            } else {
                showError(response.errorBody()?.string())
            }
        } catch (e: Exception) {
            showError("Password reset failed: ${e.message}")
        }
    }
}
```

---

## **🔧 Authentication Flow Examples**

### **Complete Signup Flow:**
```kotlin
class AuthViewModel : ViewModel() {
    fun completeSignupFlow(name: String, email: String, password: String) {
        viewModelScope.launch {
            try {
                // Step 1: Signup
                val signupResponse = authApi.signup(SignupRequest(name, email, password))
                if (!signupResponse.isSuccessful) {
                    showError("Signup failed")
                    return@launch
                }
                
                // Step 2: Navigate to OTP screen
                _navigationEvent.value = NavigationEvent.ToOtpVerification(email)
                
            } catch (e: Exception) {
                showError("Signup failed: ${e.message}")
            }
        }
    }
    
    fun verifyAndLogin(email: String, otp: String) {
        viewModelScope.launch {
            try {
                // Step 3: Verify OTP
                val verifyResponse = authApi.verifyEmail(VerifyEmailRequest(email, otp))
                if (!verifyResponse.isSuccessful) {
                    showError("OTP verification failed")
                    return@launch
                }
                
                // Step 4: Auto-login after verification (optional)
                // You might want to ask user to login manually
                _navigationEvent.value = NavigationEvent.ToLogin
                
            } catch (e: Exception) {
                showError("Verification failed: ${e.message}")
            }
        }
    }
}
```

### **Password Reset Flow:**
```kotlin
fun completePasswordResetFlow(email: String) {
    viewModelScope.launch {
        try {
            // Step 1: Request reset OTP
            val forgotResponse = authApi.forgotPassword(email)
            if (!forgotResponse.isSuccessful) {
                showError("Failed to send reset OTP")
                return@launch
            }
            
            _currentResetEmail.value = email
            _navigationEvent.value = NavigationEvent.ToResetOtpScreen(email)
            
        } catch (e: Exception) {
            showError("Reset request failed: ${e.message}")
        }
    }
}

fun completePasswordReset(email: String, otp: String, newPassword: String) {
    viewModelScope.launch {
        try {
            // Step 2: Verify reset OTP
            val verifyResponse = authApi.verifyResetOtp(VerifyEmailRequest(email, otp))
            if (!verifyResponse.isSuccessful) {
                showError("Invalid OTP")
                return@launch
            }
            
            // Step 3: Set new password
            val resetResponse = authApi.resetPassword(ResetPasswordRequest(email, newPassword))
            if (resetResponse.isSuccessful) {
                showMessage("Password reset successfully")
                _navigationEvent.value = NavigationEvent.ToLogin
            } else {
                showError("Failed to reset password")
            }
            
        } catch (e: Exception) {
            showError("Password reset failed: ${e.message}")
        }
    }
}
```

---

## **🛡️ Security Notes**

1. **JWT Token Storage**: Store JWT tokens securely using Android Keystore or EncryptedSharedPreferences
2. **OTP Security**: 
   - OTPs expire after 10 minutes
   - Old OTPs are automatically invalidated when new ones are generated
   - Single-use OTPs (cannot be reused)
3. **Password Requirements**: Minimum 4 characters for signup, 6 characters for reset
4. **Rate Limiting**: Implement client-side cooldowns for OTP resend requests
5. **Network Security**: Always use HTTPS endpoints

---

**Authentication Complete! ✅**  
Next: [Books Management APIs →](BOOKS_API.md)