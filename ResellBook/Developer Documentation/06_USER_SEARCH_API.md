# 🔍 User Search APIs

## **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/UserSearch`

---

## **API Endpoints**

### **1. Log User Search** ⭐ NEW
**`POST /api/UserSearch/LogSearch`**

**Purpose:** Log user search activity for analytics and recommendations

**Authentication:** ✅ JWT Token Required

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "UserId": "123e4567-e89b-12d3-a456-426614174000",
  "SearchTerm": "data structures algorithms"
}
```

**Validation Rules:**
- ✅ **UserId**: Required, must be valid GUID
- ✅ **SearchTerm**: Required, not empty or whitespace

**Success Response (200):**
```json
{
  "Message": "Search logged successfully"
}
```

**Error Responses:**
```json
// 400 - Validation errors
{
  "Message": "Validation failed",
  "Errors": [
    "The UserId field is required.",
    "The SearchTerm field is required."
  ]
}

// 401 - Unauthorized
{"Message": "Unauthorized"}
```

---

## **📱 Android Kotlin Implementation**

### **Data Models:**
```kotlin
data class UserSearchDto(
    @SerializedName("UserId") val userId: String,
    @SerializedName("SearchTerm") val searchTerm: String
)

data class SearchLogResponse(
    @SerializedName("Message") val message: String
)
```

### **API Interface:**
```kotlin
interface UserSearchApi {
    @POST("api/UserSearch/LogSearch")
    suspend fun logSearch(
        @Header("Authorization") token: String,
        @Body request: UserSearchDto
    ): Response<SearchLogResponse>
}
```

### **Repository Implementation:**
```kotlin
class UserSearchRepository : BaseRepository() {
    private val searchApi = ApiClient.retrofit.create(UserSearchApi::class.java)
    
    suspend fun logUserSearch(userId: String, searchTerm: String): Flow<Resource<String>> = flow {
        emit(Resource.Loading())
        
        try {
            val token = "Bearer ${getAuthToken()}"
            val request = UserSearchDto(userId, searchTerm)
            val response = searchApi.logSearch(token, request)
            
            if (response.isSuccessful) {
                val message = response.body()?.message ?: "Search logged successfully"
                emit(Resource.Success(message))
            } else {
                val errorMessage = parseErrorResponse(response)
                emit(Resource.Error(errorMessage))
            }
        } catch (e: Exception) {
            emit(Resource.Error("Failed to log search: ${e.message}"))
        }
    }.flowOn(Dispatchers.IO)
}
```

### **ViewModel Integration:**
```kotlin
class BooksViewModel(
    private val booksRepository: BooksRepository,
    private val userSearchRepository: UserSearchRepository
) : BaseViewModel() {
    
    private val _searchResults = MutableLiveData<List<Book>>()
    val searchResults: LiveData<List<Book>> = _searchResults
    
    fun searchBooks(userId: String, searchQuery: String) {
        viewModelScope.launch {
            // Log the search activity
            userSearchRepository.logUserSearch(userId, searchQuery).collect { resource ->
                when (resource) {
                    is Resource.Success -> {
                        Log.d("Search", "Search logged: ${resource.data}")
                    }
                    is Resource.Error -> {
                        Log.w("Search", "Failed to log search: ${resource.message}")
                        // Continue with search even if logging fails
                    }
                    is Resource.Loading -> { /* Handle if needed */ }
                }
            }
            
            // Perform actual search
            performBookSearch(searchQuery)
        }
    }
    
    private fun performBookSearch(query: String) {
        viewModelScope.launch {
            booksRepository.searchBooks(query).collect { resource ->
                handleResource(resource) { books ->
                    _searchResults.value = books
                }
            }
        }
    }
}
```

### **UI Implementation:**
```kotlin
class SearchActivity : AppCompatActivity() {
    private lateinit var binding: ActivitySearchBinding
    private lateinit var viewModel: BooksViewModel
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = DataBindingUtil.setContentView(this, R.layout.activity_search)
        
        setupSearchView()
        observeViewModel()
    }
    
    private fun setupSearchView() {
        binding.searchView.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String?): Boolean {
                query?.let { searchQuery ->
                    val userId = getUserId() // Get current user ID
                    
                    // This will both log the search AND perform the actual search
                    viewModel.searchBooks(userId, searchQuery)
                }
                return true
            }
            
            override fun onQueryTextChange(newText: String?): Boolean {
                // Optional: Log search as user types (debounced)
                return false
            }
        })
    }
    
    private fun observeViewModel() {
        viewModel.searchResults.observe(this) { books ->
            updateSearchResults(books)
        }
        
        viewModel.error.observe(this) { error ->
            showError(error)
        }
    }
}
```

---

## **💡 Use Cases**

### **1. Search Analytics**
- Track popular search terms
- Identify trending categories
- Improve recommendation algorithms
- Understand user behavior patterns

### **2. Performance Optimization**
- Cache popular searches
- Pre-load frequently searched content
- Optimize search ranking algorithms

### **3. User Experience**
- Provide search suggestions
- Show "recently searched" items
- Personalize search results

---

## **🔧 Best Practices**

### **1. Search Logging Strategy:**
```kotlin
// Log only meaningful searches (3+ characters)
fun shouldLogSearch(query: String): Boolean {
    return query.trim().length >= 3 && 
           query.trim().isNotBlank() && 
           !query.contains(Regex("[^a-zA-Z0-9\\s]"))
}

// Debounce search logging to avoid spam
private var searchLogJob: Job? = null

fun performDebouncedSearchLog(userId: String, query: String) {
    searchLogJob?.cancel()
    searchLogJob = viewModelScope.launch {
        delay(500) // Wait 500ms before logging
        if (shouldLogSearch(query)) {
            userSearchRepository.logUserSearch(userId, query)
        }
    }
}
```

### **2. Error Handling:**
```kotlin
// Continue search functionality even if logging fails
userSearchRepository.logUserSearch(userId, searchQuery).collect { resource ->
    when (resource) {
        is Resource.Error -> {
            // Log error but don't show to user - logging is background functionality
            Log.w("SearchLog", "Search logging failed: ${resource.message}")
        }
        else -> { /* Success or loading - no action needed */ }
    }
}
```

### **3. Privacy Considerations:**
- Only log search terms, not personal data
- Consider implementing search log retention policies
- Allow users to opt-out of search logging if required

---

## **📊 Integration Summary**

✅ **What This API Provides:**
- Anonymous search activity logging
- Analytics data for improving search functionality
- User behavior insights for recommendations

✅ **What This API Doesn't Do:**
- Actual search functionality (use Books API for that)
- Store personal information beyond UserId
- Provide search results or suggestions

**This API should be called alongside your existing search functionality to provide analytics data that can improve the overall user experience.**