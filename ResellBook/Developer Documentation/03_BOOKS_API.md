# 📚 Books Management APIs

## **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/Books`

---

## **API Endpoints**

### **1. List New Book** 
**`POST /api/Books/ListBook`**

**Purpose:** Create a new book listing with 2-4 images

**Authentication:** 🔒 JWT Token Required

**Content-Type:** `multipart/form-data`

**Form Parameters:**
```
UserId: "123e4567-e89b-12d3-a456-426614174000" (GUID - Required)
BookName: "Data Structures and Algorithms" (String - Required)
AuthorOrPublication: "Robert Lafore" (String - Optional)
Category: "Computer Science" (String - Required)
Description: "Comprehensive guide to data structures with examples" (String - Required) ⭐ NEW
SubCategory: "Programming" (String - Optional)
SellingPrice: 299.99 (Decimal - Required)
Images: [File1, File2, File3, File4] (IFormFile[] - Required, 2-4 files)
```

**Validation Rules:**
- ✅ **UserId**: Must exist in Users database
- ✅ **BookName**: Required, not empty or whitespace
- ✅ **Category**: Required, not empty or whitespace
- ✅ **Description**: Required, not empty or whitespace ⭐ NEW FIELD
- ✅ **SellingPrice**: Required, must be positive decimal value
- ✅ **Images**: Required, minimum 2 files, maximum 4 files
- ✅ **File Types**: Image formats (jpg, png, gif, etc.)
- ✅ **File Size**: Reasonable limits for mobile upload

**Success Response (200):**
```json
{
  "Message": "Book listed successfully",
  "BookId": "789e4567-e89b-12d3-a456-426614174000"
}
```

**Error Responses:**
```json
// 404 - User not found
{"Message": "User not found"}

// 400 - Invalid image count
{"Message": "You must upload between 2 and 4 images."}

// 400 - Missing required fields (auto-validation)
// Model validation will handle individual field errors
```

**Android Kotlin Implementation:**
```kotlin
data class BookListResponse(
    val Message: String,
    val BookId: String
)

interface BooksApi {
    @Multipart
    @POST("api/Books/ListBook")
    suspend fun listBook(
        @Header("Authorization") token: String,
        @Part("UserId") userId: RequestBody,
        @Part("BookName") bookName: RequestBody,
        @Part("Category") category: RequestBody,
        @Part("Description") description: RequestBody, // ⭐ NEW REQUIRED FIELD
        @Part("SellingPrice") sellingPrice: RequestBody,
        @Part("AuthorOrPublication") authorOrPublication: RequestBody? = null,
        @Part("SubCategory") subCategory: RequestBody? = null,
        @Part images: List<MultipartBody.Part>
    ): Response<BookListResponse>
}

// Usage Example
class BookRepository {
    fun listBook(
        userId: String,
        bookName: String,
        category: String,
        sellingPrice: Double,
        authorOrPublication: String? = null,
        subCategory: String? = null,
        imageUris: List<Uri>
    ) {
        viewModelScope.launch {
            try {
                // Validate inputs
                if (imageUris.size < 2 || imageUris.size > 4) {
                    showError("Please select 2-4 images")
                    return@launch
                }
                
                // Create form parts
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
                
                // Convert images to MultipartBody.Part
                val imageParts = imageUris.mapIndexed { index, uri ->
                    createImagePart(uri, "Images", context)
                }
                
                val token = "Bearer ${getAuthToken()}"
                val response = booksApi.listBook(
                    token = token,
                    userId = userIdPart,
                    bookName = bookNamePart,
                    category = categoryPart,
                    sellingPrice = pricePart,
                    authorOrPublication = authorPart,
                    subCategory = subCategoryPart,
                    images = imageParts
                )
                
                if (response.isSuccessful) {
                    val result = response.body()
                    showSuccess("Book listed successfully! ID: ${result?.BookId}")
                } else {
                    showError(response.errorBody()?.string() ?: "Failed to list book")
                }
                
            } catch (e: Exception) {
                showError("Upload failed: ${e.message}")
            }
        }
    }
    
    // Helper function to create image parts
    private fun createImagePart(imageUri: Uri, partName: String, context: Context): MultipartBody.Part {
        val inputStream = context.contentResolver.openInputStream(imageUri)
        val file = File(context.cacheDir, "book_image_${System.currentTimeMillis()}.jpg")
        
        file.outputStream().use { outputStream ->
            inputStream?.copyTo(outputStream)
        }
        
        val requestBody = RequestBody.create(MediaType.parse("image/*"), file)
        return MultipartBody.Part.createFormData(partName, file.name, requestBody)
    }
}

// ViewModel Usage
class AddBookViewModel : ViewModel() {
    private val _uploadProgress = MutableLiveData<Boolean>()
    val uploadProgress: LiveData<Boolean> = _uploadProgress
    
    fun uploadBook(bookData: BookData, images: List<Uri>) {
        _uploadProgress.value = true
        
        bookRepository.listBook(
            userId = getCurrentUserId(),
            bookName = bookData.bookName,
            category = bookData.category,
            sellingPrice = bookData.sellingPrice,
            authorOrPublication = bookData.author,
            subCategory = bookData.subCategory,
            imageUris = images
        )
        
        _uploadProgress.value = false
    }
}
```

---

### **2. Edit Book Listing** 
**`PUT /api/Books/EditListing/{id}`**

**Purpose:** Update existing book listing (partial updates allowed)

**Authentication:** 🔒 JWT Token Required

**Content-Type:** `multipart/form-data`

**URL Parameter:**
- `id`: Book ID (GUID) - **Required**

**Form Parameters (All Optional):**
```
BookName: "Updated Book Title" (String - Optional)
AuthorOrPublication: "Updated Author" (String - Optional)
Category: "Updated Category" (String - Optional)
SubCategory: "Updated SubCategory" (String - Optional)
SellingPrice: 399.99 (Decimal - Optional)
NewImages: [File1, File2] (IFormFile[] - Optional, new images to add)
ExistingImages: ["uploads/books/image1.jpg", "uploads/books/image2.jpg"] (String[] - Optional, existing image paths to keep)
```

**Update Logic:**
1. **Keep existing values** if parameter not provided
2. **Replace values** if parameter provided
3. **Image Management:**
   - `ExistingImages`: Paths of current images to keep
   - `NewImages`: New image files to add
   - Images not in `ExistingImages` are removed
   - New images are added to remaining count

**Validation Rules:**
- ✅ **Book ID**: Must exist in database
- ✅ **Final Image Count**: Must be 2-4 after all updates
- ✅ **Image Limit**: Cannot exceed 4 total images
- ✅ **Minimum Images**: Must have at least 2 images after update

**Success Response (200):**
```json
{"Message": "Book updated successfully"}
```

**Error Responses:**
```json
// 404 - Book not found
{"Message": "Book not found"}

// 400 - Insufficient images
{"Message": "Each book must have at least 2 images."}

// 400 - Too many images
{"Message": "Maximum 4 images allowed."}
```

**Android Kotlin Implementation:**
```kotlin
interface BooksApi {
    @Multipart
    @PUT("api/Books/EditListing/{id}")
    suspend fun editBook(
        @Header("Authorization") token: String,
        @Path("id") bookId: String,
        @Part("BookName") bookName: RequestBody? = null,
        @Part("AuthorOrPublication") authorOrPublication: RequestBody? = null,
        @Part("Category") category: RequestBody? = null,
        @Part("SubCategory") subCategory: RequestBody? = null,
        @Part("SellingPrice") sellingPrice: RequestBody? = null,
        @Part newImages: List<MultipartBody.Part>? = null,
        @PartMap existingImages: Map<String, RequestBody>? = null
    ): Response<MessageResponse>
}

// Usage Example
class BookEditRepository {
    suspend fun editBook(
        bookId: String,
        updates: BookUpdateData,
        newImageUris: List<Uri>? = null,
        existingImagePaths: List<String>? = null
    ) {
        try {
            val token = "Bearer ${getAuthToken()}"
            
            // Create optional form parts only for fields being updated
            val bookNamePart = updates.bookName?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it)
            }
            val authorPart = updates.author?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it)
            }
            val categoryPart = updates.category?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it)
            }
            val subCategoryPart = updates.subCategory?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it)
            }
            val pricePart = updates.sellingPrice?.let { 
                RequestBody.create(MediaType.parse("text/plain"), it.toString())
            }
            
            // Handle new images
            val newImageParts = newImageUris?.map { uri ->
                createImagePart(uri, "NewImages", context)
            }
            
            // Handle existing images to keep
            val existingImagesMap = existingImagePaths?.mapIndexed { index, path ->
                "ExistingImages[$index]" to RequestBody.create(MediaType.parse("text/plain"), path)
            }?.toMap()
            
            val response = booksApi.editBook(
                token = token,
                bookId = bookId,
                bookName = bookNamePart,
                authorOrPublication = authorPart,
                category = categoryPart,
                subCategory = subCategoryPart,
                sellingPrice = pricePart,
                newImages = newImageParts,
                existingImages = existingImagesMap
            )
            
            if (response.isSuccessful) {
                showSuccess("Book updated successfully")
            } else {
                showError(response.errorBody()?.string() ?: "Update failed")
            }
            
        } catch (e: Exception) {
            showError("Update failed: ${e.message}")
        }
    }
}

// Data classes for updates
data class BookUpdateData(
    val bookName: String? = null,
    val author: String? = null,
    val category: String? = null,
    val subCategory: String? = null,
    val sellingPrice: Double? = null
)

data class MessageResponse(
    val Message: String
)

// ViewModel for editing
class EditBookViewModel : ViewModel() {
    fun updateBook(
        bookId: String,
        updates: BookUpdateData,
        newImages: List<Uri>,
        keepExistingImages: List<String>
    ) {
        // Validate image count
        val totalImages = newImages.size + keepExistingImages.size
        if (totalImages < 2) {
            showError("Book must have at least 2 images")
            return
        }
        if (totalImages > 4) {
            showError("Book cannot have more than 4 images")
            return
        }
        
        viewModelScope.launch {
            bookRepository.editBook(
                bookId = bookId,
                updates = updates,
                newImageUris = newImages,
                existingImagePaths = keepExistingImages
            )
        }
    }
}
```

---

### **3. View All Books** 
**`GET /api/Books/ViewAll/{userId}`**

**Purpose:** Retrieve all book listings with distance calculation and owner information

**Authentication:** ❌ No authentication required

**URL Parameters:**
- `userId`: Current user's ID for distance calculation (GUID) - **Required**

**✨ UPDATED:** Now includes UserId and UserName of book owners for messaging integration!

**Success Response (200):**
```json
[
  {
    "Id": "789e4567-e89b-12d3-a456-426614174000",
    "UserId": "123e4567-e89b-12d3-a456-426614174000",
    "UserName": "John Doe",
    "BookName": "Data Structures and Algorithms",
    "AuthorOrPublication": "Robert Lafore",
    "Category": "Computer Science",
    "SubCategory": "Programming",
    "SellingPrice": 299.99,
    "IsSold": false,
    "Images": [
      "uploads/books/image1.jpg",
      "uploads/books/image2.jpg",
      "uploads/books/image3.jpg"
    ],
    "CreatedAt": "2025-09-30T10:30:00Z",
    "Distance": "2.5 km"
  },
  {
    "Id": "456e4567-e89b-12d3-a456-426614174000",
    "UserId": "987e4567-e89b-12d3-a456-426614174000",
    "UserName": "Jane Smith",
    "BookName": "Clean Code",
    "AuthorOrPublication": "Robert C. Martin",
    "Category": "Software Engineering",
    "SubCategory": null,
    "SellingPrice": 450.00,
    "IsSold": false,
    "Images": [
      "uploads/books/clean_code_1.jpg",
      "uploads/books/clean_code_2.jpg"
    ],
    "CreatedAt": "2025-09-29T15:20:00Z",
    "Distance": "850 m"
  }
]
```

**✅ New Fields Added:**
- **`UserId`**: ID of the book owner (for messaging integration)
- **`UserName`**: Name of the book owner (for display)
- **`IsSold`**: Book availability status
- **`Distance`**: Formatted distance from current user's location

**Android Kotlin Implementation:**
```kotlin
data class Book(
    val Id: String,
    val BookName: String,
    val AuthorOrPublication: String?,
    val Category: String,
    val SubCategory: String?,
    val SellingPrice: Double,
    val Images: List<String>,
    val CreatedAt: String
) {
    // Helper properties for UI
    val formattedPrice: String
        get() = "₹${String.format("%.2f", SellingPrice)}"
    
    val displayAuthor: String
        get() = AuthorOrPublication ?: "Unknown Author"
    
    val displayCategory: String
        get() = if (SubCategory.isNullOrEmpty()) Category else "$Category - $SubCategory"
    
    val primaryImageUrl: String
        get() = if (Images.isNotEmpty()) 
            "${ApiConstants.BASE_URL}${Images[0]}" 
        else 
            ApiConstants.DEFAULT_BOOK_IMAGE
    
    val allImageUrls: List<String>
        get() = Images.map { "${ApiConstants.BASE_URL}$it" }
}

interface BooksApi {
    @GET("api/Books/ViewAll")
    suspend fun getAllBooks(): Response<List<Book>>
}

// Repository
class BooksRepository {
    suspend fun getAllBooks(): Result<List<Book>> {
        return try {
            val response = booksApi.getAllBooks()
            if (response.isSuccessful) {
                Result.success(response.body() ?: emptyList())
            } else {
                Result.failure(Exception("Failed to load books: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

// ViewModel
class BooksListViewModel : ViewModel() {
    private val _books = MutableLiveData<List<Book>>()
    val books: LiveData<List<Book>> = _books
    
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    private val _error = MutableLiveData<String>()
    val error: LiveData<String> = _error
    
    fun loadBooks() {
        _isLoading.value = true
        
        viewModelScope.launch {
            booksRepository.getAllBooks()
                .onSuccess { booksList ->
                    _books.value = booksList.sortedByDescending { 
                        // Sort by creation date, newest first
                        it.CreatedAt
                    }
                    _isLoading.value = false
                }
                .onFailure { exception ->
                    _error.value = exception.message
                    _isLoading.value = false
                }
        }
    }
    
    fun refreshBooks() {
        loadBooks() // Simple refresh implementation
    }
    
    fun searchBooks(query: String) {
        val allBooks = _books.value ?: return
        val filtered = allBooks.filter { book ->
            book.BookName.contains(query, ignoreCase = true) ||
            book.Category.contains(query, ignoreCase = true) ||
            book.AuthorOrPublication?.contains(query, ignoreCase = true) == true
        }
        _books.value = filtered
    }
}

// RecyclerView Adapter Example
class BooksAdapter : RecyclerView.Adapter<BooksAdapter.BookViewHolder>() {
    private var books: List<Book> = emptyList()
    
    fun updateBooks(newBooks: List<Book>) {
        books = newBooks
        notifyDataSetChanged()
    }
    
    override fun onBindViewHolder(holder: BookViewHolder, position: Int) {
        val book = books[position]
        holder.bind(book)
    }
    
    class BookViewHolder(private val binding: ItemBookBinding) : 
        RecyclerView.ViewHolder(binding.root) {
        
        fun bind(book: Book) {
            binding.apply {
                textBookName.text = book.BookName
                textAuthor.text = book.displayAuthor
                textCategory.text = book.displayCategory
                textPrice.text = book.formattedPrice
                
                // Load primary image with Glide
                Glide.with(imageBook.context)
                    .load(book.primaryImageUrl)
                    .placeholder(R.drawable.book_placeholder)
                    .error(R.drawable.book_error)
                    .into(imageBook)
                
                root.setOnClickListener {
                    // Navigate to book details
                    onBookClick(book)
                }
            }
        }
    }
}
```

---

## **🖼️ Image Handling Best Practices**

### **Image Upload Guidelines:**
```kotlin
class ImageUploadHelper {
    companion object {
        const val MAX_IMAGE_SIZE = 5 * 1024 * 1024 // 5MB
        const val MIN_IMAGES = 2
        const val MAX_IMAGES = 4
        
        fun validateImages(imageUris: List<Uri>, context: Context): ValidationResult {
            if (imageUris.size < MIN_IMAGES) {
                return ValidationResult.Error("Please select at least $MIN_IMAGES images")
            }
            if (imageUris.size > MAX_IMAGES) {
                return ValidationResult.Error("Maximum $MAX_IMAGES images allowed")
            }
            
            // Check file sizes
            imageUris.forEach { uri ->
                val size = getFileSize(uri, context)
                if (size > MAX_IMAGE_SIZE) {
                    return ValidationResult.Error("Image size must be less than 5MB")
                }
            }
            
            return ValidationResult.Success
        }
        
        fun compressImage(imageUri: Uri, context: Context): Uri {
            // Implement image compression logic
            // Use libraries like Compressor or custom compression
        }
    }
}

sealed class ValidationResult {
    object Success : ValidationResult()
    data class Error(val message: String) : ValidationResult()
}
```

### **Image Loading with Caching:**
```kotlin
// Using Glide for efficient image loading
class ImageLoader {
    companion object {
        fun loadBookImage(
            imageView: ImageView,
            imageUrl: String,
            placeholder: Int = R.drawable.book_placeholder
        ) {
            Glide.with(imageView.context)
                .load(imageUrl)
                .placeholder(placeholder)
                .error(R.drawable.book_error)
                .diskCacheStrategy(DiskCacheStrategy.ALL)
                .transition(DrawableTransitionOptions.withCrossFade())
                .into(imageView)
        }
        
        fun preloadImages(imageUrls: List<String>, context: Context) {
            imageUrls.forEach { url ->
                Glide.with(context).load(url).preload()
            }
        }
    }
}
```

---

## **📝 Complete Book Management Flow**

### **Add Book Flow:**
```kotlin
class AddBookActivity : AppCompatActivity() {
    private lateinit var viewModel: AddBookViewModel
    private var selectedImages: MutableList<Uri> = mutableListOf()
    
    private fun setupImageSelection() {
        val imagePickerLauncher = registerForActivityResult(
            ActivityResultContracts.GetMultipleContents()
        ) { uris ->
            if (uris.size in 2..4) {
                selectedImages.clear()
                selectedImages.addAll(uris)
                updateImagePreview()
            } else {
                showError("Please select 2-4 images")
            }
        }
        
        binding.buttonSelectImages.setOnClickListener {
            imagePickerLauncher.launch("image/*")
        }
    }
    
    private fun submitBook() {
        val bookData = BookUpdateData(
            bookName = binding.editBookName.text.toString(),
            author = binding.editAuthor.text.toString(),
            category = binding.spinnerCategory.selectedItem.toString(),
            subCategory = binding.editSubCategory.text.toString(),
            sellingPrice = binding.editPrice.text.toString().toDoubleOrNull()
        )
        
        // Validate
        if (bookData.bookName.isNullOrBlank()) {
            showError("Book name is required")
            return
        }
        
        if (selectedImages.size < 2) {
            showError("Please select at least 2 images")
            return
        }
        
        viewModel.uploadBook(bookData, selectedImages)
    }
}
```

---

## **🔧 Error Handling & Retry Logic**

```kotlin
class BooksApiHandler {
    suspend fun uploadBookWithRetry(
        bookData: BookData, 
        images: List<Uri>,
        maxRetries: Int = 3
    ): Result<BookListResponse> {
        var lastException: Exception? = null
        
        repeat(maxRetries) { attempt ->
            try {
                val response = booksApi.listBook(/* parameters */)
                if (response.isSuccessful) {
                    return Result.success(response.body()!!)
                } else if (response.code() == 413) {
                    // Payload too large - compress images
                    val compressedImages = images.map { compressImage(it) }
                    return uploadBookWithRetry(bookData, compressedImages, maxRetries - attempt)
                } else if (response.code() in 500..599 && attempt < maxRetries - 1) {
                    // Server error - retry after delay
                    delay(1000L * (attempt + 1))
                    continue
                } else {
                    return Result.failure(Exception(response.errorBody()?.string()))
                }
            } catch (e: Exception) {
                lastException = e
                if (attempt < maxRetries - 1) {
                    delay(1000L * (attempt + 1))
                }
            }
        }
        
        return Result.failure(lastException ?: Exception("Upload failed after $maxRetries attempts"))
    }
}
```

---

---

### **4. View My Book Listings** 
**`GET /api/Books/ViewMyListings/{userId}`**

**Purpose:** Get all books listed by a specific user

**Authentication:** 🔒 JWT Token Required

**URL Parameters:**
- `userId`: User GUID - **Required**

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
      "https://resellpandaimages.blob.core.windows.net/bookimages/books/image1.jpg",
      "https://resellpandaimages.blob.core.windows.net/bookimages/books/image2.jpg"
    ],
    "IsSold": false,
    "CreatedAt": "2025-10-03T10:30:00Z"
  }
]
```

**Error Responses:**
```json
// 401 - Unauthorized
{"Message": "Unauthorized"}

// Empty array if no books found
[]
```

**Android Kotlin Implementation:**
```kotlin
data class MyBookListing(
    val Id: String,
    val BookName: String,
    val AuthorOrPublication: String?,
    val Category: String,
    val SubCategory: String?,
    val SellingPrice: Double,
    val Images: List<String>,
    val IsSold: Boolean,
    val CreatedAt: String
) {
    val statusText: String
        get() = if (IsSold) "SOLD" else "AVAILABLE"
        
    val statusColor: Int
        get() = if (IsSold) R.color.red else R.color.green
}

interface BooksApi {
    @GET("api/Books/ViewMyListings/{userId}")
    suspend fun getMyListings(
        @Header("Authorization") token: String,
        @Path("userId") userId: String
    ): Response<List<MyBookListing>>
}

// Repository
class MyBooksRepository {
    suspend fun getMyBooks(userId: String): Result<List<MyBookListing>> {
        return try {
            val token = "Bearer ${getAuthToken()}"
            val response = booksApi.getMyListings(token, userId)
            
            if (response.isSuccessful) {
                Result.success(response.body() ?: emptyList())
            } else {
                Result.failure(Exception("Failed to load your books: ${response.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

// ViewModel
class MyBooksViewModel : ViewModel() {
    private val _myBooks = MutableLiveData<List<MyBookListing>>()
    val myBooks: LiveData<List<MyBookListing>> = _myBooks
    
    fun loadMyBooks() {
        viewModelScope.launch {
            val userId = getCurrentUserId()
            myBooksRepository.getMyBooks(userId)
                .onSuccess { books ->
                    _myBooks.value = books.sortedByDescending { it.CreatedAt }
                }
                .onFailure { error ->
                    // Handle error
                }
        }
    }
    
    fun getAvailableBooks() = _myBooks.value?.filter { !it.IsSold } ?: emptyList()
    fun getSoldBooks() = _myBooks.value?.filter { it.IsSold } ?: emptyList()
}
```

---

### **5. Mark Book as Sold** 
**`PATCH /api/Books/MarkAsSold/{bookId}`**

**Purpose:** Mark a book listing as sold (cannot be undone)

**Authentication:** 🔒 JWT Token Required

**URL Parameters:**
- `bookId`: Book GUID - **Required**

**Success Response (200):**
```json
{"Message": "Book marked as sold successfully."}
```

**Error Responses:**
```json
// 404 - Book not found
{"Message": "Book not found"}

// 400 - Already sold
{"Message": "This book is already marked as sold."}

// 401 - Unauthorized
{"Message": "Unauthorized"}
```

**Android Kotlin Implementation:**
```kotlin
interface BooksApi {
    @PATCH("api/Books/MarkAsSold/{bookId}")
    suspend fun markAsSold(
        @Header("Authorization") token: String,
        @Path("bookId") bookId: String
    ): Response<MessageResponse>
}

// Repository
class BookActionsRepository {
    suspend fun markBookAsSold(bookId: String): Result<String> {
        return try {
            val token = "Bearer ${getAuthToken()}"
            val response = booksApi.markAsSold(token, bookId)
            
            if (response.isSuccessful) {
                Result.success(response.body()?.Message ?: "Book marked as sold")
            } else {
                val errorBody = response.errorBody()?.string()
                val errorMessage = try {
                    val errorResponse = Gson().fromJson(errorBody, MessageResponse::class.java)
                    errorResponse.Message
                } catch (e: Exception) {
                    "Failed to mark as sold"
                }
                Result.failure(Exception(errorMessage))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

// ViewModel
class BookDetailsViewModel : ViewModel() {
    private val _isMarkingSold = MutableLiveData<Boolean>()
    val isMarkingSold: LiveData<Boolean> = _isMarkingSold
    
    fun markAsSold(bookId: String) {
        _isMarkingSold.value = true
        
        viewModelScope.launch {
            bookActionsRepository.markBookAsSold(bookId)
                .onSuccess { message ->
                    showSuccess(message)
                    // Refresh book details or navigate back
                    refreshBookDetails(bookId)
                }
                .onFailure { error ->
                    showError(error.message ?: "Failed to mark as sold")
                }
                .also {
                    _isMarkingSold.value = false
                }
        }
    }
}

// UI Usage with Confirmation Dialog
class BookDetailsFragment {
    private fun setupMarkSoldButton() {
        binding.buttonMarkSold.setOnClickListener {
            showMarkSoldConfirmation()
        }
    }
    
    private fun showMarkSoldConfirmation() {
        AlertDialog.Builder(requireContext())
            .setTitle("Mark as Sold")
            .setMessage("Are you sure you want to mark this book as sold? This action cannot be undone.")
            .setPositiveButton("Yes, Mark as Sold") { _, _ ->
                viewModel.markAsSold(bookId)
            }
            .setNegativeButton("Cancel") { dialog, _ ->
                dialog.dismiss()
            }
            .show()
    }
}
```

---

### **6. Mark Book as Unsold** ⭐ NEW
**`PATCH /api/Books/MarkAsUnSold/{bookId}`**

**Purpose:** Mark a book listing as unsold (revert sold status)

**Authentication:** 🔒 JWT Token Required

**URL Parameters:**
- `bookId`: Book GUID - **Required**

**Success Response (200):**
```json
{"Message": "Book marked as Unsold successfully."}
```

**Error Responses:**
```json
// 404 - Book not found
{"Message": "Book not found"}

// 400 - Already unsold
{"Message": "This book is already marked as Unsold."}

// 401 - Unauthorized
{"Message": "Unauthorized"}
```

**Android Kotlin Implementation:**
```kotlin
interface BooksApi {
    @PATCH("api/Books/MarkAsUnSold/{bookId}")
    suspend fun markAsUnSold(
        @Header("Authorization") token: String,
        @Path("bookId") bookId: String
    ): Response<MessageResponse>
}

// Repository
suspend fun markBookAsUnSold(bookId: String): Result<String> {
    return try {
        val token = "Bearer ${getAuthToken()}"
        val response = booksApi.markAsUnSold(token, bookId)
        
        if (response.isSuccessful) {
            Result.success(response.body()?.Message ?: "Book marked as unsold")
        } else {
            val errorMessage = parseErrorMessage(response.errorBody()?.string())
            Result.failure(Exception(errorMessage))
        }
    } catch (e: Exception) {
        Result.failure(e)
    }
}
```

---

### **7. Delete Book Listing** 
**`DELETE /api/Books/Delete/{bookId}`**

**Purpose:** Permanently delete a book listing and its images

**Authentication:** 🔒 JWT Token Required

**URL Parameters:**
- `bookId`: Book GUID - **Required**

**Behavior:**
- ✅ Removes book from database
- ✅ Deletes associated images from Azure Blob Storage
- ✅ Permanent deletion (cannot be undone)
- ✅ Only book owner can delete

**Success Response (200):**
```json
{"Message": "Book deleted successfully"}
```

**Error Responses:**
```json
// 404 - Book not found
{"Message": "Book not found"}

// 401 - Unauthorized
{"Message": "Unauthorized"}

// 403 - Not book owner (if ownership check implemented)
{"Message": "You can only delete your own books"}
```

**Android Kotlin Implementation:**
```kotlin
interface BooksApi {
    @DELETE("api/Books/Delete/{bookId}")
    suspend fun deleteBook(
        @Header("Authorization") token: String,
        @Path("bookId") bookId: String
    ): Response<MessageResponse>
}

// Repository
class BookActionsRepository {
    suspend fun deleteBook(bookId: String): Result<String> {
        return try {
            val token = "Bearer ${getAuthToken()}"
            val response = booksApi.deleteBook(token, bookId)
            
            if (response.isSuccessful) {
                Result.success(response.body()?.Message ?: "Book deleted successfully")
            } else {
                val errorBody = response.errorBody()?.string()
                val errorMessage = try {
                    val errorResponse = Gson().fromJson(errorBody, MessageResponse::class.java)
                    errorResponse.Message
                } catch (e: Exception) {
                    "Failed to delete book"
                }
                Result.failure(Exception(errorMessage))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

// ViewModel
class MyBooksViewModel : ViewModel() {
    private val _isDeleting = MutableLiveData<Boolean>()
    val isDeleting: LiveData<Boolean> = _isDeleting
    
    fun deleteBook(bookId: String) {
        _isDeleting.value = true
        
        viewModelScope.launch {
            bookActionsRepository.deleteBook(bookId)
                .onSuccess { message ->
                    showSuccess(message)
                    // Remove from local list and refresh
                    removeBookFromList(bookId)
                    loadMyBooks() // Refresh the list
                }
                .onFailure { error ->
                    showError(error.message ?: "Failed to delete book")
                }
                .also {
                    _isDeleting.value = false
                }
        }
    }
    
    private fun removeBookFromList(bookId: String) {
        val currentBooks = _myBooks.value?.toMutableList() ?: return
        currentBooks.removeAll { it.Id == bookId }
        _myBooks.value = currentBooks
    }
}

// UI Usage with Confirmation
class MyBooksFragment {
    private fun setupBookActions(book: MyBookListing, holder: BookViewHolder) {
        holder.binding.buttonDelete.setOnClickListener {
            showDeleteConfirmation(book)
        }
    }
    
    private fun showDeleteConfirmation(book: MyBookListing) {
        AlertDialog.Builder(requireContext())
            .setTitle("Delete Book")
            .setMessage("Are you sure you want to delete '${book.BookName}'?\n\nThis will:\n• Remove the listing permanently\n• Delete all associated images\n• Cannot be undone")
            .setPositiveButton("Delete") { _, _ ->
                viewModel.deleteBook(book.Id)
            }
            .setNegativeButton("Cancel") { dialog, _ ->
                dialog.dismiss()
            }
            .setIcon(R.drawable.ic_warning)
            .show()
    }
}

// SwipeToDelete Implementation
class MyBooksAdapter : RecyclerView.Adapter<MyBooksAdapter.BookViewHolder>() {
    
    fun attachSwipeToDelete(recyclerView: RecyclerView, viewModel: MyBooksViewModel) {
        val swipeCallback = object : ItemTouchHelper.SimpleCallback(
            0, ItemTouchHelper.LEFT or ItemTouchHelper.RIGHT
        ) {
            override fun onMove(/* not implemented */) = false
            
            override fun onSwiped(viewHolder: RecyclerView.ViewHolder, direction: Int) {
                val position = viewHolder.adapterPosition
                val book = books[position]
                
                // Show confirmation before actual deletion
                showDeleteConfirmation(book) {
                    viewModel.deleteBook(book.Id)
                }
            }
            
            override fun onChildDraw(
                c: Canvas, recyclerView: RecyclerView, viewHolder: RecyclerView.ViewHolder,
                dX: Float, dY: Float, actionState: Int, isCurrentlyActive: Boolean
            ) {
                // Draw delete background and icon
                if (actionState == ItemTouchHelper.ACTION_STATE_SWIPE) {
                    val itemView = viewHolder.itemView
                    val paint = Paint().apply { color = Color.RED }
                    
                    if (dX > 0) { // Swipe right
                        c.drawRect(itemView.left.toFloat(), itemView.top.toFloat(), 
                                 dX, itemView.bottom.toFloat(), paint)
                    } else { // Swipe left
                        c.drawRect(itemView.right.toFloat() + dX, itemView.top.toFloat(),
                                 itemView.right.toFloat(), itemView.bottom.toFloat(), paint)
                    }
                }
                
                super.onChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive)
            }
        }
        
        ItemTouchHelper(swipeCallback).attachToRecyclerView(recyclerView)
    }
}
```

---

## **📊 Complete Book Management Dashboard**

```kotlin
// Dashboard showing book statistics
class BookDashboardViewModel : ViewModel() {
    private val _dashboardData = MutableLiveData<DashboardData>()
    val dashboardData: LiveData<DashboardData> = _dashboardData
    
    fun loadDashboard() {
        viewModelScope.launch {
            val userId = getCurrentUserId()
            myBooksRepository.getMyBooks(userId)
                .onSuccess { books ->
                    val dashboard = DashboardData(
                        totalBooks = books.size,
                        availableBooks = books.count { !it.IsSold },
                        soldBooks = books.count { it.IsSold },
                        totalValue = books.filter { !it.IsSold }.sumOf { it.SellingPrice },
                        recentBooks = books.take(5)
                    )
                    _dashboardData.value = dashboard
                }
        }
    }
}

data class DashboardData(
    val totalBooks: Int,
    val availableBooks: Int,
    val soldBooks: Int,
    val totalValue: Double,
    val recentBooks: List<MyBookListing>
) {
    val soldPercentage: Float
        get() = if (totalBooks > 0) (soldBooks.toFloat() / totalBooks * 100) else 0f
        
    val formattedTotalValue: String
        get() = "₹${String.format("%.2f", totalValue)}"
}
```

---

**Books Management APIs Complete! ✅**  

All 6 endpoints now documented:
- ✅ **POST** `/ListBook` - Create new book listing
- ✅ **PUT** `/EditListing/{id}` - Update existing book
- ✅ **GET** `/ViewAll/{userId}` - Browse all books  
- ✅ **GET** `/ViewMyListings/{userId}` - View user's books
- ✅ **PATCH** `/MarkAsSold/{bookId}` - Mark as sold
- ✅ **DELETE** `/Delete/{bookId}` - Delete book & images
````  
Next: [User Location APIs →](LOCATION_API.md)