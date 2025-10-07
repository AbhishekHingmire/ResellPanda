# 🤖 Android Kotlin Integration Guide

## **Complete Setup Guide for ResellPanda API Integration**

---

## **📱 Project Setup**

### **1. Dependencies (build.gradle - Module: app)**
```gradle
dependencies {
    // Core Android
    implementation 'androidx.core:core-ktx:1.12.0'
    implementation 'androidx.appcompat:appcompat:1.6.1'
    implementation 'androidx.constraintlayout:constraintlayout:2.1.4'
    
    // Architecture Components
    implementation 'androidx.lifecycle:lifecycle-viewmodel-ktx:2.7.0'
    implementation 'androidx.lifecycle:lifecycle-livedata-ktx:2.7.0'
    implementation 'androidx.activity:activity-ktx:1.8.2'
    implementation 'androidx.fragment:fragment-ktx:1.6.2'
    
    // Navigation
    implementation 'androidx.navigation:navigation-fragment-ktx:2.7.6'
    implementation 'androidx.navigation:navigation-ui-ktx:2.7.6'
    
    // Networking
    implementation 'com.squareup.retrofit2:retrofit:2.9.0'
    implementation 'com.squareup.retrofit2:converter-gson:2.9.0'
    implementation 'com.squareup.okhttp3:logging-interceptor:4.12.0'
    
    // Image Loading & Processing
    implementation 'com.github.bumptech.glide:glide:4.16.0'
    implementation 'id.zelory:compressor:3.0.1'
    
    // Location Services
    implementation 'com.google.android.gms:play-services-location:21.0.1'
    implementation 'com.google.android.gms:play-services-maps:18.2.0'
    
    // Work Manager (Background tasks)
    implementation 'androidx.work:work-runtime-ktx:2.9.0'
    
    // Security (Token storage)
    implementation 'androidx.security:security-crypto:1.1.0-alpha06'
    
    // UI Components
    implementation 'com.google.android.material:material:1.11.0'
    implementation 'androidx.swiperefreshlayout:swiperefreshlayout:1.1.0'
    implementation 'androidx.viewpager2:viewpager2:1.0.0'
    
    // Image Picker
    implementation 'com.github.dhaval2404:imagepicker:2.1'
    
    // Permission Handling
    implementation 'com.karumi:dexter:6.2.3'
}
```

### **2. Permissions (AndroidManifest.xml)**
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" 
    android:maxSdkVersion="28" />
<uses-permission android:name="android.permission.WAKE_LOCK" />

<application
    android:name=".ResellPandaApplication"
    android:allowBackup="true"
    android:icon="@mipmap/ic_launcher"
    android:label="@string/app_name"
    android:networkSecurityConfig="@xml/network_security_config"
    android:theme="@style/Theme.ResellPanda"
    android:usesCleartextTraffic="true">
    
    <!-- Activities -->
    <activity android:name=".MainActivity" />
    
    <!-- Work Manager -->
    <provider
        android:name="androidx.startup.InitializationProvider"
        android:authorities="${applicationId}.androidx-startup"
        tools:node="merge">
        <meta-data
            android:name="androidx.work.WorkManagerInitializer"
            android:value="androidx.startup" />
    </provider>
    
</application>
```

---

## **🔧 Network Layer Setup**

### **1. API Constants**
```kotlin
object ApiConstants {
    const val BASE_URL = "https://resellbook20250929183655.azurewebsites.net/"
    const val API_BASE = "api/"
    
    // Endpoints
    const val AUTH_BASE = "${API_BASE}Auth/"
    const val BOOKS_BASE = "${API_BASE}Books/"
    const val LOCATION_BASE = "${API_BASE}UserLocation/"
    
    // Image URLs
    const val IMAGE_BASE_URL = BASE_URL
    const val DEFAULT_BOOK_IMAGE = "https://via.placeholder.com/300x400?text=No+Image"
    
    // Timeouts
    const val CONNECT_TIMEOUT = 30L
    const val READ_TIMEOUT = 30L
    const val WRITE_TIMEOUT = 30L
    
    // Cache
    const val CACHE_SIZE = 10 * 1024 * 1024L // 10MB
}
```

### **2. Secure Token Manager**
```kotlin
class SecureTokenManager(context: Context) {
    private val sharedPreferences = EncryptedSharedPreferences.create(
        "secure_prefs",
        MasterKey.Builder(context)
            .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
            .build(),
        context,
        EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
        EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
    )
    
    fun saveToken(token: String) {
        sharedPreferences.edit()
            .putString("jwt_token", token)
            .putLong("token_saved_time", System.currentTimeMillis())
            .apply()
    }
    
    fun getToken(): String? {
        return sharedPreferences.getString("jwt_token", null)
    }
    
    fun clearToken() {
        sharedPreferences.edit()
            .remove("jwt_token")
            .remove("token_saved_time")
            .apply()
    }
    
    fun isTokenExpired(): Boolean {
        val savedTime = sharedPreferences.getLong("token_saved_time", 0)
        val currentTime = System.currentTimeMillis()
        val tokenAgeHours = (currentTime - savedTime) / (1000 * 60 * 60)
        return tokenAgeHours > 24 // Assume 24-hour expiry
    }
}
```

### **3. API Client with Interceptors**
```kotlin
object ApiClient {
    private var tokenManager: SecureTokenManager? = null
    
    fun initialize(context: Context) {
        tokenManager = SecureTokenManager(context)
    }
    
    private val loggingInterceptor = HttpLoggingInterceptor().apply {
        level = if (BuildConfig.DEBUG) {
            HttpLoggingInterceptor.Level.BODY
        } else {
            HttpLoggingInterceptor.Level.NONE
        }
    }
    
    private val authInterceptor = Interceptor { chain ->
        val originalRequest = chain.request()
        val token = tokenManager?.getToken()
        
        val requestBuilder = originalRequest.newBuilder()
        
        if (!token.isNullOrEmpty() && !tokenManager!!.isTokenExpired()) {
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }
        
        requestBuilder.addHeader("Content-Type", "application/json")
        requestBuilder.addHeader("Accept", "application/json")
        
        chain.proceed(requestBuilder.build())
    }
    
    private val cacheInterceptor = Interceptor { chain ->
        val response = chain.proceed(chain.request())
        val cacheControl = CacheControl.Builder()
            .maxAge(5, TimeUnit.MINUTES)
            .build()
        
        response.newBuilder()
            .header("Cache-Control", cacheControl.toString())
            .build()
    }
    
    private val okHttpClient = OkHttpClient.Builder()
        .addInterceptor(authInterceptor)
        .addInterceptor(loggingInterceptor)
        .addNetworkInterceptor(cacheInterceptor)
        .connectTimeout(ApiConstants.CONNECT_TIMEOUT, TimeUnit.SECONDS)
        .readTimeout(ApiConstants.READ_TIMEOUT, TimeUnit.SECONDS)
        .writeTimeout(ApiConstants.WRITE_TIMEOUT, TimeUnit.SECONDS)
        .retryOnConnectionFailure(true)
        .build()
    
    val retrofit: Retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(ApiConstants.BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create(createGson()))
            .build()
    }
    
    private fun createGson(): Gson {
        return GsonBuilder()
            .setDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'")
            .setLenient()
            .create()
    }
}
```

---

## **🏗️ Repository Pattern Implementation**

### **1. Base Repository**
```kotlin
abstract class BaseRepository {
    protected inline fun <T> safeApiCall(
        crossinline apiCall: suspend () -> Response<T>
    ): Flow<Resource<T>> = flow {
        emit(Resource.Loading())
        
        try {
            val response = apiCall()
            if (response.isSuccessful) {
                response.body()?.let { data ->
                    emit(Resource.Success(data))
                } ?: emit(Resource.Error("Empty response body"))
            } else {
                val errorBody = response.errorBody()?.string()
                emit(Resource.Error(errorBody ?: "Unknown error occurred"))
            }
        } catch (e: IOException) {
            emit(Resource.Error("Network connection error. Please check your internet."))
        } catch (e: HttpException) {
            emit(Resource.Error("Server error: ${e.message()}"))
        } catch (e: Exception) {
            emit(Resource.Error("Unexpected error: ${e.message}"))
        }
    }.flowOn(Dispatchers.IO)
}

sealed class Resource<T>(
    val data: T? = null,
    val message: String? = null
) {
    class Success<T>(data: T) : Resource<T>(data)
    class Error<T>(message: String, data: T? = null) : Resource<T>(data, message)
    class Loading<T> : Resource<T>()
}
```

### **2. Auth Repository**
```kotlin
class AuthRepository : BaseRepository() {
    private val authApi = ApiClient.retrofit.create(AuthApi::class.java)
    private val tokenManager = ApiClient.tokenManager!!
    
    fun signup(name: String, email: String, password: String): Flow<Resource<String>> {
        val request = SignupRequest(name, email, password)
        return safeApiCall { authApi.signup(request) }
    }
    
    fun verifyEmail(email: String, code: String): Flow<Resource<String>> {
        val request = VerifyEmailRequest(email, code)
        return safeApiCall { authApi.verifyEmail(request) }
    }
    
    fun login(email: String, password: String): Flow<Resource<LoginResponse>> = flow {
        emit(Resource.Loading())
        
        val request = LoginRequest(email, password)
        val result = safeApiCall { authApi.login(request) }
        
        result.collect { resource ->
            when (resource) {
                is Resource.Success -> {
                    resource.data?.let { loginResponse ->
                        tokenManager.saveToken(loginResponse.Token)
                        emit(Resource.Success(loginResponse))
                    }
                }
                is Resource.Error -> emit(Resource.Error(resource.message!!))
                is Resource.Loading -> emit(Resource.Loading())
            }
        }
    }
    
    fun logout() {
        tokenManager.clearToken()
    }
    
    fun isLoggedIn(): Boolean {
        val token = tokenManager.getToken()
        return !token.isNullOrEmpty() && !tokenManager.isTokenExpired()
    }
}
```

### **3. Books Repository**
```kotlin
class BooksRepository : BaseRepository() {
    private val booksApi = ApiClient.retrofit.create(BooksApi::class.java)
    
    fun getAllBooks(): Flow<Resource<List<Book>>> {
        return safeApiCall { booksApi.getAllBooks() }
    }
    
    suspend fun uploadBook(
        userId: String,
        bookName: String,
        category: String,
        sellingPrice: Double,
        authorOrPublication: String? = null,
        subCategory: String? = null,
        imageFiles: List<File>
    ): Flow<Resource<BookListResponse>> = flow {
        emit(Resource.Loading())
        
        try {
            val userIdPart = RequestBody.create(MediaType.parse("text/plain"), userId)
            val bookNamePart = RequestBody.create(MediaType.parse("text/plain"), bookName)
            val categoryPart = RequestBody.create(MediaType.parse("text/plain"), category)
            val pricePart = RequestBody.create(MediaType.parse("text/plain"), sellingPrice.toString())
            
            val authorPart = authorOrPublication?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it)
            }
            val subCategoryPart = subCategory?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it)
            }
            
            val imageParts = imageFiles.map { file ->
                val requestBody = RequestBody.create(MediaType.parse("image/*"), file)
                MultipartBody.Part.createFormData("Images", file.name, requestBody)
            }
            
            val response = booksApi.listBook(
                userId = userIdPart,
                bookName = bookNamePart,
                category = categoryPart,
                sellingPrice = pricePart,
                authorOrPublication = authorPart,
                subCategory = subCategoryPart,
                images = imageParts
            )
            
            if (response.isSuccessful) {
                response.body()?.let { data ->
                    emit(Resource.Success(data))
                } ?: emit(Resource.Error("Empty response"))
            } else {
                emit(Resource.Error(response.errorBody()?.string() ?: "Upload failed"))
            }
            
        } catch (e: Exception) {
            emit(Resource.Error("Upload failed: ${e.message}"))
        }
    }.flowOn(Dispatchers.IO)
}
```

---

## **🎨 MVVM Architecture Setup**

### **1. Base ViewModel**
```kotlin
abstract class BaseViewModel : ViewModel() {
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    private val _error = MutableLiveData<String>()
    val error: LiveData<String> = _error
    
    private val _success = MutableLiveData<String>()
    val success: LiveData<String> = _success
    
    protected fun setLoading(loading: Boolean) {
        _isLoading.value = loading
    }
    
    protected fun setError(message: String) {
        _error.value = message
        _isLoading.value = false
    }
    
    protected fun setSuccess(message: String) {
        _success.value = message
        _isLoading.value = false
    }
    
    protected fun <T> handleResource(
        resource: Resource<T>,
        onSuccess: (T) -> Unit
    ) {
        when (resource) {
            is Resource.Loading -> setLoading(true)
            is Resource.Success -> {
                setLoading(false)
                resource.data?.let(onSuccess)
            }
            is Resource.Error -> {
                setError(resource.message ?: "Unknown error")
            }
        }
    }
}
```

### **2. Auth ViewModel**
```kotlin
class AuthViewModel(private val authRepository: AuthRepository) : BaseViewModel() {
    
    private val _signupResult = MutableLiveData<Boolean>()
    val signupResult: LiveData<Boolean> = _signupResult
    
    private val _loginResult = MutableLiveData<LoginResponse>()
    val loginResult: LiveData<LoginResponse> = _loginResult
    
    fun signup(name: String, email: String, password: String) {
        viewModelScope.launch {
            authRepository.signup(name, email, password).collect { resource ->
                handleResource(resource) { message ->
                    setSuccess(message)
                    _signupResult.value = true
                }
            }
        }
    }
    
    fun verifyEmail(email: String, code: String) {
        viewModelScope.launch {
            authRepository.verifyEmail(email, code).collect { resource ->
                handleResource(resource) { message ->
                    setSuccess(message)
                }
            }
        }
    }
    
    fun login(email: String, password: String) {
        viewModelScope.launch {
            authRepository.login(email, password).collect { resource ->
                handleResource(resource) { loginResponse ->
                    _loginResult.value = loginResponse
                    setSuccess("Login successful")
                }
            }
        }
    }
    
    fun logout() {
        authRepository.logout()
    }
    
    fun isLoggedIn(): Boolean {
        return authRepository.isLoggedIn()
    }
}
```

### **3. Books ViewModel**
```kotlin
class BooksViewModel(private val booksRepository: BooksRepository) : BaseViewModel() {
    
    private val _books = MutableLiveData<List<Book>>()
    val books: LiveData<List<Book>> = _books
    
    private val _uploadResult = MutableLiveData<BookListResponse>()
    val uploadResult: LiveData<BookListResponse> = _uploadResult
    
    fun loadBooks() {
        viewModelScope.launch {
            booksRepository.getAllBooks().collect { resource ->
                handleResource(resource) { booksList ->
                    _books.value = booksList
                }
            }
        }
    }
    
    fun uploadBook(
        userId: String,
        bookData: BookData,
        imageFiles: List<File>
    ) {
        viewModelScope.launch {
            booksRepository.uploadBook(
                userId = userId,
                bookName = bookData.bookName,
                category = bookData.category,
                sellingPrice = bookData.sellingPrice,
                authorOrPublication = bookData.author,
                subCategory = bookData.subCategory,
                imageFiles = imageFiles
            ).collect { resource ->
                handleResource(resource) { response ->
                    _uploadResult.value = response
                    setSuccess("Book uploaded successfully")
                    loadBooks() // Refresh list
                }
            }
        }
    }
    
    fun searchBooks(query: String) {
        val currentBooks = _books.value ?: return
        val filteredBooks = currentBooks.filter { book ->
            book.BookName.contains(query, ignoreCase = true) ||
            book.Category.contains(query, ignoreCase = true) ||
            book.AuthorOrPublication?.contains(query, ignoreCase = true) == true
        }
        _books.value = filteredBooks
    }
}
```

---

## **🖼️ Image Handling Utilities**

### **1. Image Picker Helper**
```kotlin
class ImagePickerHelper(private val activity: AppCompatActivity) {
    
    private var onImagesSelected: ((List<Uri>) -> Unit)? = null
    
    private val imagePickerLauncher = activity.registerForActivityResult(
        ActivityResultContracts.GetMultipleContents()
    ) { uris ->
        if (uris.isNotEmpty()) {
            onImagesSelected?.invoke(uris)
        }
    }
    
    private val cameraLauncher = activity.registerForActivityResult(
        ActivityResultContracts.TakePicture()
    ) { success ->
        if (success) {
            currentPhotoUri?.let { uri ->
                onImagesSelected?.invoke(listOf(uri))
            }
        }
    }
    
    private var currentPhotoUri: Uri? = null
    
    fun pickImages(
        maxImages: Int = 4,
        onSelected: (List<Uri>) -> Unit
    ) {
        onImagesSelected = { uris ->
            if (uris.size <= maxImages) {
                onSelected(uris)
            } else {
                Toast.makeText(
                    activity, 
                    "Maximum $maxImages images allowed", 
                    Toast.LENGTH_SHORT
                ).show()
            }
        }
        
        showImageSourceDialog()
    }
    
    private fun showImageSourceDialog() {
        val options = arrayOf("Camera", "Gallery", "Cancel")
        AlertDialog.Builder(activity)
            .setTitle("Select Image Source")
            .setItems(options) { _, which ->
                when (which) {
                    0 -> openCamera()
                    1 -> openGallery()
                }
            }
            .show()
    }
    
    private fun openCamera() {
        val photoFile = createImageFile()
        currentPhotoUri = FileProvider.getUriForFile(
            activity,
            "${activity.packageName}.fileprovider",
            photoFile
        )
        cameraLauncher.launch(currentPhotoUri)
    }
    
    private fun openGallery() {
        imagePickerLauncher.launch("image/*")
    }
    
    private fun createImageFile(): File {
        val timeStamp = SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(Date())
        val imageFileName = "JPEG_${timeStamp}_"
        val storageDir = activity.getExternalFilesDir(Environment.DIRECTORY_PICTURES)
        return File.createTempFile(imageFileName, ".jpg", storageDir)
    }
}
```

### **2. Image Compression Utility**
```kotlin
class ImageCompressionHelper(private val context: Context) {
    
    suspend fun compressImages(imageUris: List<Uri>): List<File> = withContext(Dispatchers.IO) {
        imageUris.map { uri ->
            compressImage(uri)
        }
    }
    
    private suspend fun compressImage(imageUri: Uri): File = withContext(Dispatchers.IO) {
        val inputStream = context.contentResolver.openInputStream(imageUri)
        val originalFile = File(context.cacheDir, "temp_${System.currentTimeMillis()}.jpg")
        
        originalFile.outputStream().use { outputStream ->
            inputStream?.copyTo(outputStream)
        }
        
        Compressor.compress(context, originalFile) {
            resolution(1080, 1080)
            quality(80)
            format(Bitmap.CompressFormat.JPEG)
            size(1_000_000) // 1MB
        }
    }
}
```

---

## **📍 Location Services Integration**

### **1. Location Manager**
```kotlin
class LocationManager(private val context: Context) {
    private val fusedLocationClient = LocationServices.getFusedLocationProviderClient(context)
    private val locationRepository = LocationRepository()
    
    @SuppressLint("MissingPermission")
    suspend fun getCurrentLocation(): Location? = suspendCoroutine { continuation ->
        if (!hasLocationPermission()) {
            continuation.resume(null)
            return@suspendCoroutine
        }
        
        fusedLocationClient.lastLocation
            .addOnSuccessListener { location ->
                continuation.resume(location)
            }
            .addOnFailureListener {
                continuation.resume(null)
            }
    }
    
    suspend fun syncLocation(userId: String): Result<String> {
        val location = getCurrentLocation()
        return if (location != null) {
            locationRepository.syncLocation(userId, location.latitude, location.longitude)
        } else {
            Result.failure(Exception("Unable to get current location"))
        }
    }
    
    private fun hasLocationPermission(): Boolean {
        return ContextCompat.checkSelfPermission(
            context,
            Manifest.permission.ACCESS_FINE_LOCATION
        ) == PackageManager.PERMISSION_GRANTED
    }
    
    fun requestLocationPermission(activity: AppCompatActivity, requestCode: Int) {
        ActivityCompat.requestPermissions(
            activity,
            arrayOf(
                Manifest.permission.ACCESS_FINE_LOCATION,
                Manifest.permission.ACCESS_COARSE_LOCATION
            ),
            requestCode
        )
    }
}
```

---

## **🎯 Complete Activity Examples**

### **1. Login Activity**
```kotlin
class LoginActivity : AppCompatActivity() {
    private lateinit var binding: ActivityLoginBinding
    private lateinit var authViewModel: AuthViewModel
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityLoginBinding.inflate(layoutInflater)
        setContentView(binding.root)
        
        setupViewModel()
        setupObservers()
        setupClickListeners()
    }
    
    private fun setupViewModel() {
        val authRepository = AuthRepository()
        authViewModel = ViewModelProvider(
            this,
            ViewModelFactory { AuthViewModel(authRepository) }
        )[AuthViewModel::class.java]
    }
    
    private fun setupObservers() {
        authViewModel.isLoading.observe(this) { isLoading ->
            binding.progressBar.visibility = if (isLoading) View.VISIBLE else View.GONE
            binding.buttonLogin.isEnabled = !isLoading
        }
        
        authViewModel.error.observe(this) { error ->
            if (error != null) {
                showError(error)
            }
        }
        
        authViewModel.loginResult.observe(this) { loginResponse ->
            if (loginResponse != null) {
                navigateToMainActivity()
            }
        }
    }
    
    private fun setupClickListeners() {
        binding.buttonLogin.setOnClickListener {
            val email = binding.editEmail.text.toString().trim()
            val password = binding.editPassword.text.toString().trim()
            
            if (validateInputs(email, password)) {
                authViewModel.login(email, password)
            }
        }
        
        binding.textSignup.setOnClickListener {
            startActivity(Intent(this, SignupActivity::class.java))
        }
    }
    
    private fun validateInputs(email: String, password: String): Boolean {
        if (email.isEmpty()) {
            binding.editEmail.error = "Email is required"
            return false
        }
        if (password.isEmpty()) {
            binding.editPassword.error = "Password is required"
            return false
        }
        return true
    }
    
    private fun showError(message: String) {
        Snackbar.make(binding.root, message, Snackbar.LENGTH_LONG).show()
    }
    
    private fun navigateToMainActivity() {
        startActivity(Intent(this, MainActivity::class.java))
        finish()
    }
}
```

### **2. Book Upload Activity**
```kotlin
class AddBookActivity : AppCompatActivity() {
    private lateinit var binding: ActivityAddBookBinding
    private lateinit var booksViewModel: BooksViewModel
    private lateinit var imagePickerHelper: ImagePickerHelper
    private lateinit var imageCompressionHelper: ImageCompressionHelper
    
    private var selectedImageUris: MutableList<Uri> = mutableListOf()
    private var compressedImageFiles: MutableList<File> = mutableListOf()
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityAddBookBinding.inflate(layoutInflater)
        setContentView(binding.root)
        
        setupViewModels()
        setupImagePicker()
        setupObservers()
        setupClickListeners()
    }
    
    private fun setupViewModels() {
        val booksRepository = BooksRepository()
        booksViewModel = ViewModelProvider(
            this,
            ViewModelFactory { BooksViewModel(booksRepository) }
        )[BooksViewModel::class.java]
        
        imageCompressionHelper = ImageCompressionHelper(this)
    }
    
    private fun setupImagePicker() {
        imagePickerHelper = ImagePickerHelper(this)
    }
    
    private fun setupObservers() {
        booksViewModel.isLoading.observe(this) { isLoading ->
            binding.progressBar.visibility = if (isLoading) View.VISIBLE else View.GONE
            binding.buttonUpload.isEnabled = !isLoading
        }
        
        booksViewModel.uploadResult.observe(this) { result ->
            if (result != null) {
                showSuccess("Book uploaded successfully!")
                finish()
            }
        }
        
        booksViewModel.error.observe(this) { error ->
            if (error != null) {
                showError(error)
            }
        }
    }
    
    private fun setupClickListeners() {
        binding.buttonSelectImages.setOnClickListener {
            imagePickerHelper.pickImages(maxImages = 4) { uris ->
                selectedImageUris.clear()
                selectedImageUris.addAll(uris)
                updateImagePreview()
                compressImagesAsync()
            }
        }
        
        binding.buttonUpload.setOnClickListener {
            uploadBook()
        }
    }
    
    private fun updateImagePreview() {
        binding.textSelectedImages.text = "${selectedImageUris.size} images selected"
        // Update RecyclerView or ImageView preview here
    }
    
    private fun compressImagesAsync() {
        lifecycleScope.launch {
            try {
                compressedImageFiles.clear()
                compressedImageFiles.addAll(
                    imageCompressionHelper.compressImages(selectedImageUris)
                )
            } catch (e: Exception) {
                showError("Image compression failed: ${e.message}")
            }
        }
    }
    
    private fun uploadBook() {
        val bookName = binding.editBookName.text.toString().trim()
        val author = binding.editAuthor.text.toString().trim()
        val category = binding.spinnerCategory.selectedItem.toString()
        val priceText = binding.editPrice.text.toString().trim()
        
        if (!validateInputs(bookName, category, priceText)) return
        
        val price = priceText.toDoubleOrNull()
        if (price == null || price <= 0) {
            binding.editPrice.error = "Enter valid price"
            return
        }
        
        if (compressedImageFiles.size < 2) {
            showError("Please select at least 2 images")
            return
        }
        
        val bookData = BookData(
            bookName = bookName,
            author = author.takeIf { it.isNotEmpty() },
            category = category,
            sellingPrice = price
        )
        
        val userId = getCurrentUserId()
        booksViewModel.uploadBook(userId, bookData, compressedImageFiles)
    }
    
    private fun validateInputs(bookName: String, category: String, price: String): Boolean {
        var isValid = true
        
        if (bookName.isEmpty()) {
            binding.editBookName.error = "Book name is required"
            isValid = false
        }
        
        if (category.isEmpty()) {
            showError("Please select a category")
            isValid = false
        }
        
        if (price.isEmpty()) {
            binding.editPrice.error = "Price is required"
            isValid = false
        }
        
        return isValid
    }
    
    private fun getCurrentUserId(): String {
        // Get from SharedPreferences or user session
        return "your-user-id"
    }
    
    private fun showError(message: String) {
        Snackbar.make(binding.root, message, Snackbar.LENGTH_LONG).show()
    }
    
    private fun showSuccess(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
    }
}
```

---

## **🚀 Application Class Setup**

### **ResellPandaApplication.kt**
```kotlin
class ResellPandaApplication : Application() {
    override fun onCreate() {
        super.onCreate()
        
        // Initialize API Client
        ApiClient.initialize(this)
        
        // Initialize Work Manager for background tasks
        setupWorkManager()
        
        // Initialize other libraries
        if (BuildConfig.DEBUG) {
            Timber.plant(Timber.DebugTree())
        }
    }
    
    private fun setupWorkManager() {
        val configuration = Configuration.Builder()
            .setMinimumLoggingLevel(if (BuildConfig.DEBUG) Log.DEBUG else Log.ERROR)
            .build()
        
        WorkManager.initialize(this, configuration)
    }
}
```

---

## **🔄 Background Sync Setup**

### **Location Sync Worker**
```kotlin
class LocationSyncWorker(
    context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {
    
    override suspend fun doWork(): Result {
        return try {
            val userId = inputData.getString("user_id") ?: return Result.failure()
            val locationManager = LocationManager(applicationContext)
            
            when (val result = locationManager.syncLocation(userId)) {
                is kotlin.Result.Success -> {
                    Timber.d("Location synced successfully")
                    Result.success()
                }
                is kotlin.Result.Failure -> {
                    Timber.e("Location sync failed: ${result.exception.message}")
                    if (runAttemptCount < 3) Result.retry() else Result.failure()
                }
            }
        } catch (e: Exception) {
            Timber.e("Location sync error: ${e.message}")
            Result.failure()
        }
    }
}
```

---

## **📱 Production Considerations**

### **1. ProGuard Rules (proguard-rules.pro)**
```proguard
# Retrofit
-dontwarn retrofit2.**
-keep class retrofit2.** { *; }
-keepattributes Signature
-keepattributes Exceptions

# Gson
-keepattributes Signature
-keepattributes *Annotation*
-dontwarn sun.misc.**
-keep class * implements com.google.gson.TypeAdapter
-keep class * implements com.google.gson.TypeAdapterFactory
-keep class * implements com.google.gson.JsonSerializer
-keep class * implements com.google.gson.JsonDeserializer

# Your API models
-keep class com.yourpackage.models.** { *; }
```

### **2. Network Security Config (res/xml/network_security_config.xml)**
```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <domain-config cleartextTrafficPermitted="true">
        <domain includeSubdomains="true">resellbook20250929183655.azurewebsites.net</domain>
    </domain-config>
</network-security-config>
```

---

**🎉 Complete Android Integration Setup Done!**

This comprehensive guide provides everything needed to integrate ResellPanda APIs in your Android Kotlin application with proper architecture, error handling, and production-ready features.