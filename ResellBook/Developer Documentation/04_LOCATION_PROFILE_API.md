# 📍 User Location & Profile APIs

## **Base URL:** `https://resellbook20250929183655.azurewebsites.net/api/UserLocation`

---

## **API Endpoints**

### **1. Sync User Location** 
**`POST /api/UserLocation/SyncLocation`**

**Purpose:** Save user's current GPS location for location-based features

**Authentication:** 🔒 JWT Token Required

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "UserId": "123e4567-e89b-12d3-a456-426614174000",
  "Latitude": 40.7128,
  "Longitude": -74.0060
}
```

**Validation Rules:**
- ✅ **UserId**: Must exist in Users database (GUID format)
- ✅ **Latitude**: Required, valid range (-90.0 to 90.0)
- ✅ **Longitude**: Required, valid range (-180.0 to 180.0)
- ✅ **Precision**: Double precision for accurate GPS coordinates

**Success Response (200):**
```json
{"Message": "Location synced successfully"}
```

**Error Responses:**
```json
// 404 - User not found
{"Message": "User not found"}

// 400 - Invalid coordinates (handled by model validation)
// Automatic validation for latitude/longitude ranges
```

**Android Kotlin Implementation:**
```kotlin
data class LocationSyncRequest(
    val UserId: String,
    val Latitude: Double,
    val Longitude: Double
)

data class LocationSyncResponse(
    val Message: String
)

interface LocationApi {
    @POST("api/UserLocation/SyncLocation")
    suspend fun syncLocation(
        @Header("Authorization") token: String,
        @Body request: LocationSyncRequest
    ): Response<LocationSyncResponse>
}

// Location Service Implementation
class LocationService(private val context: Context) {
    private val fusedLocationClient = LocationServices.getFusedLocationProviderClient(context)
    private val locationApi = ApiClient.retrofit.create(LocationApi::class.java)
    
    @SuppressLint("MissingPermission")
    suspend fun syncCurrentLocation(userId: String): Result<String> {
        return try {
            // Check location permissions
            if (!hasLocationPermission()) {
                return Result.failure(Exception("Location permission not granted"))
            }
            
            // Get last known location
            val location = suspendCoroutine<Location?> { continuation ->
                fusedLocationClient.lastLocation
                    .addOnSuccessListener { location ->
                        continuation.resume(location)
                    }
                    .addOnFailureListener { exception ->
                        continuation.resumeWithException(exception)
                    }
            }
            
            if (location == null) {
                return Result.failure(Exception("Unable to get current location"))
            }
            
            // Validate coordinates
            if (!isValidCoordinate(location.latitude, location.longitude)) {
                return Result.failure(Exception("Invalid GPS coordinates"))
            }
            
            // Sync with server
            val request = LocationSyncRequest(
                UserId = userId,
                Latitude = location.latitude,
                Longitude = location.longitude
            )
            
            val token = "Bearer ${getAuthToken()}"
            val response = locationApi.syncLocation(token, request)
            
            if (response.isSuccessful) {
                // Save locally for offline access
                saveLocationLocally(location)
                Result.success("Location synced successfully")
            } else {
                Result.failure(Exception(response.errorBody()?.string() ?: "Sync failed"))
            }
            
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private fun hasLocationPermission(): Boolean {
        return ContextCompat.checkSelfPermission(
            context,
            Manifest.permission.ACCESS_FINE_LOCATION
        ) == PackageManager.PERMISSION_GRANTED
    }
    
    private fun isValidCoordinate(latitude: Double, longitude: Double): Boolean {
        return latitude in -90.0..90.0 && longitude in -180.0..180.0
    }
    
    private fun saveLocationLocally(location: Location) {
        // Cache location locally using Room database or SharedPreferences
        val sharedPrefs = context.getSharedPreferences("user_location", Context.MODE_PRIVATE)
        sharedPrefs.edit()
            .putFloat("last_latitude", location.latitude.toFloat())
            .putFloat("last_longitude", location.longitude.toFloat())
            .putLong("last_location_time", System.currentTimeMillis())
            .apply()
    }
}

// Background Location Sync
class LocationSyncWorker(
    context: Context,
    params: WorkerParameters
) : CoroutineWorker(context, params) {
    
    override suspend fun doWork(): Result {
        val userId = inputData.getString("user_id") ?: return Result.failure()
        val locationService = LocationService(applicationContext)
        
        return when (val result = locationService.syncCurrentLocation(userId)) {
            is kotlin.Result.Success -> Result.success()
            is kotlin.Result.Failure -> {
                // Retry logic for network failures
                if (runAttemptCount < 3) {
                    Result.retry()
                } else {
                    Result.failure()
                }
            }
        }
    }
}

// ViewModel Usage
class LocationViewModel(private val locationService: LocationService) : ViewModel() {
    private val _syncStatus = MutableLiveData<LocationSyncStatus>()
    val syncStatus: LiveData<LocationSyncStatus> = _syncStatus
    
    fun syncLocation() {
        _syncStatus.value = LocationSyncStatus.Loading
        
        viewModelScope.launch {
            val userId = getCurrentUserId()
            locationService.syncCurrentLocation(userId)
                .onSuccess { message ->
                    _syncStatus.value = LocationSyncStatus.Success(message)
                }
                .onFailure { exception ->
                    _syncStatus.value = LocationSyncStatus.Error(exception.message ?: "Sync failed")
                }
        }
    }
    
    // Auto-sync on app start or periodic sync
    fun startPeriodicLocationSync() {
        val constraints = Constraints.Builder()
            .setRequiredNetworkType(NetworkType.CONNECTED)
            .build()
            
        val locationSyncRequest = PeriodicWorkRequestBuilder<LocationSyncWorker>(
            repeatInterval = 15, // Every 15 minutes
            repeatIntervalTimeUnit = TimeUnit.MINUTES
        )
            .setInputData(workDataOf("user_id" to getCurrentUserId()))
            .setConstraints(constraints)
            .build()
            
        WorkManager.getInstance(getApplication()).enqueueUniquePeriodicWork(
            "location_sync",
            ExistingPeriodicWorkPolicy.KEEP,
            locationSyncRequest
        )
    }
}

sealed class LocationSyncStatus {
    object Loading : LocationSyncStatus()
    data class Success(val message: String) : LocationSyncStatus()
    data class Error(val message: String) : LocationSyncStatus()
}
```

---

### **2. Get User Locations** 
**`GET /api/UserLocation/GetLocations/{userId}`**

**Purpose:** Retrieve location history for a specific user

**Authentication:** 🔒 JWT Token Required

**URL Parameter:**
- `userId`: User ID (GUID format) - **Required**

**Success Response (200):**
```json
[
  {
    "Id": "456e4567-e89b-12d3-a456-426614174000",
    "UserId": "123e4567-e89b-12d3-a456-426614174000",
    "Latitude": 40.7128,
    "Longitude": -74.0060,
    "CreateDate": "2025-09-30T10:30:00Z"
  },
  {
    "Id": "789e4567-e89b-12d3-a456-426614174000",
    "UserId": "123e4567-e89b-12d3-a456-426614174000",
    "Latitude": 40.7580,
    "Longitude": -73.9855,
    "CreateDate": "2025-09-30T08:15:00Z"
  }
]
```

**Error Responses:**
```json
// 404 - No locations found
{"Message": "No locations found for this user"}
```

**Android Kotlin Implementation:**
```kotlin
data class UserLocationHistory(
    val Id: String,
    val UserId: String,
    val Latitude: Double,
    val Longitude: Double,
    val CreateDate: String
) {
    // Helper properties for UI display
    val coordinates: String
        get() = "${String.format("%.6f", Latitude)}, ${String.format("%.6f", Longitude)}"
    
    val formattedDate: String
        get() = try {
            val inputFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.getDefault())
            val outputFormat = SimpleDateFormat("MMM dd, yyyy HH:mm", Locale.getDefault())
            val date = inputFormat.parse(CreateDate)
            outputFormat.format(date ?: Date())
        } catch (e: Exception) {
            CreateDate
        }
    
    val latLng: LatLng
        get() = LatLng(Latitude, Longitude)
        
    fun distanceFrom(other: UserLocationHistory): Float {
        val results = FloatArray(1)
        Location.distanceBetween(
            Latitude, Longitude,
            other.Latitude, other.Longitude,
            results
        )
        return results[0] // Distance in meters
    }
}

interface LocationApi {
    @GET("api/UserLocation/GetLocations/{userId}")
    suspend fun getUserLocations(
        @Header("Authorization") token: String,
        @Path("userId") userId: String
    ): Response<List<UserLocationHistory>>
}

// Repository
class LocationHistoryRepository {
    private val locationApi = ApiClient.retrofit.create(LocationApi::class.java)
    
    suspend fun getUserLocationHistory(userId: String): Result<List<UserLocationHistory>> {
        return try {
            val token = "Bearer ${getAuthToken()}"
            val response = locationApi.getUserLocations(token, userId)
            
            if (response.isSuccessful) {
                val locations = response.body() ?: emptyList()
                Result.success(locations.sortedByDescending { it.CreateDate })
            } else {
                Result.failure(Exception(response.errorBody()?.string() ?: "Failed to load locations"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

// ViewModel for Location History
class LocationHistoryViewModel : ViewModel() {
    private val repository = LocationHistoryRepository()
    
    private val _locations = MutableLiveData<List<UserLocationHistory>>()
    val locations: LiveData<List<UserLocationHistory>> = _locations
    
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    private val _error = MutableLiveData<String>()
    val error: LiveData<String> = _error
    
    fun loadLocationHistory(userId: String) {
        _isLoading.value = true
        
        viewModelScope.launch {
            repository.getUserLocationHistory(userId)
                .onSuccess { locationList ->
                    _locations.value = locationList
                    _isLoading.value = false
                }
                .onFailure { exception ->
                    _error.value = exception.message
                    _isLoading.value = false
                }
        }
    }
    
    fun getLocationAnalytics(locations: List<UserLocationHistory>): LocationAnalytics {
        if (locations.isEmpty()) {
            return LocationAnalytics.empty()
        }
        
        val totalDistance = locations.zipWithNext { current, next ->
            current.distanceFrom(next)
        }.sum()
        
        val mostRecentLocation = locations.firstOrNull()
        val oldestLocation = locations.lastOrNull()
        
        return LocationAnalytics(
            totalLocations = locations.size,
            totalDistanceTraveled = totalDistance,
            mostRecentLocation = mostRecentLocation,
            oldestLocation = oldestLocation,
            averageLatitude = locations.map { it.Latitude }.average(),
            averageLongitude = locations.map { it.Longitude }.average()
        )
    }
}

data class LocationAnalytics(
    val totalLocations: Int,
    val totalDistanceTraveled: Float, // in meters
    val mostRecentLocation: UserLocationHistory?,
    val oldestLocation: UserLocationHistory?,
    val averageLatitude: Double,
    val averageLongitude: Double
) {
    companion object {
        fun empty() = LocationAnalytics(0, 0f, null, null, 0.0, 0.0)
    }
    
    val totalDistanceKm: String
        get() = String.format("%.2f km", totalDistanceTraveled / 1000)
    
    val centerPoint: LatLng
        get() = LatLng(averageLatitude, averageLongitude)
}

// RecyclerView Adapter for Location History
class LocationHistoryAdapter : RecyclerView.Adapter<LocationHistoryAdapter.LocationViewHolder>() {
    private var locations: List<UserLocationHistory> = emptyList()
    var onLocationClick: ((UserLocationHistory) -> Unit)? = null
    
    fun updateLocations(newLocations: List<UserLocationHistory>) {
        locations = newLocations
        notifyDataSetChanged()
    }
    
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): LocationViewHolder {
        val binding = ItemLocationHistoryBinding.inflate(
            LayoutInflater.from(parent.context), parent, false
        )
        return LocationViewHolder(binding)
    }
    
    override fun onBindViewHolder(holder: LocationViewHolder, position: Int) {
        holder.bind(locations[position])
    }
    
    override fun getItemCount() = locations.size
    
    inner class LocationViewHolder(
        private val binding: ItemLocationHistoryBinding
    ) : RecyclerView.ViewHolder(binding.root) {
        
        fun bind(location: UserLocationHistory) {
            binding.apply {
                textCoordinates.text = location.coordinates
                textDate.text = location.formattedDate
                
                // Add distance from previous location if available
                if (adapterPosition > 0) {
                    val previousLocation = locations[adapterPosition - 1]
                    val distance = location.distanceFrom(previousLocation)
                    textDistance.text = "Moved ${String.format("%.0f", distance)}m"
                    textDistance.visibility = View.VISIBLE
                } else {
                    textDistance.visibility = View.GONE
                }
                
                root.setOnClickListener {
                    onLocationClick?.invoke(location)
                }
            }
        }
    }
}
```

---

### **3. Get User Profile** 
**`GET /api/UserLocation/profile/{userId}`**

**Purpose:** Retrieve user profile information

**Authentication:** 🔒 JWT Token Required

**URL Parameter:**
- `userId`: User ID (GUID format) - **Required**

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
// 404 - User not found
"User not found."
```

**Android Kotlin Implementation:**
```kotlin
data class UserProfile(
    val Id: String,
    val Name: String,
    val Email: String,
    val IsEmailVerified: Boolean
) {
    val displayName: String
        get() = Name.takeIf { it.isNotBlank() } ?: "Unknown User"
    
    val verificationStatus: String
        get() = if (IsEmailVerified) "Verified" else "Not Verified"
    
    val initials: String
        get() = Name.split(" ")
            .mapNotNull { it.firstOrNull()?.uppercaseChar() }
            .take(2)
            .joinToString("")
            .takeIf { it.isNotEmpty() } ?: "?"
}

interface UserApi {
    @GET("api/UserLocation/profile/{userId}")
    suspend fun getUserProfile(
        @Header("Authorization") token: String,
        @Path("userId") userId: String
    ): Response<UserProfile>
}

// Repository
class UserProfileRepository {
    private val userApi = ApiClient.retrofit.create(UserApi::class.java)
    
    suspend fun getUserProfile(userId: String): Result<UserProfile> {
        return try {
            val token = "Bearer ${getAuthToken()}"
            val response = userApi.getUserProfile(token, userId)
            
            if (response.isSuccessful) {
                val profile = response.body()
                if (profile != null) {
                    // Cache profile locally
                    cacheUserProfile(profile)
                    Result.success(profile)
                } else {
                    Result.failure(Exception("Empty response"))
                }
            } else {
                Result.failure(Exception(response.errorBody()?.string() ?: "Profile not found"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private fun cacheUserProfile(profile: UserProfile) {
        // Cache using SharedPreferences or Room database
        val sharedPrefs = context.getSharedPreferences("user_profile", Context.MODE_PRIVATE)
        val gson = Gson()
        sharedPrefs.edit()
            .putString("profile_data", gson.toJson(profile))
            .putLong("profile_cached_time", System.currentTimeMillis())
            .apply()
    }
    
    fun getCachedProfile(): UserProfile? {
        val sharedPrefs = context.getSharedPreferences("user_profile", Context.MODE_PRIVATE)
        val profileJson = sharedPrefs.getString("profile_data", null)
        val cacheTime = sharedPrefs.getLong("profile_cached_time", 0)
        
        // Return cached profile if less than 1 hour old
        return if (profileJson != null && (System.currentTimeMillis() - cacheTime) < 3600000) {
            try {
                Gson().fromJson(profileJson, UserProfile::class.java)
            } catch (e: Exception) {
                null
            }
        } else {
            null
        }
    }
}

// ViewModel
class UserProfileViewModel : ViewModel() {
    private val repository = UserProfileRepository()
    
    private val _profile = MutableLiveData<UserProfile>()
    val profile: LiveData<UserProfile> = _profile
    
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    private val _error = MutableLiveData<String>()
    val error: LiveData<String> = _error
    
    fun loadProfile(userId: String, forceRefresh: Boolean = false) {
        // Try cached profile first
        if (!forceRefresh) {
            val cachedProfile = repository.getCachedProfile()
            if (cachedProfile != null) {
                _profile.value = cachedProfile
                return
            }
        }
        
        _isLoading.value = true
        
        viewModelScope.launch {
            repository.getUserProfile(userId)
                .onSuccess { userProfile ->
                    _profile.value = userProfile
                    _isLoading.value = false
                }
                .onFailure { exception ->
                    _error.value = exception.message
                    _isLoading.value = false
                }
        }
    }
    
    fun refreshProfile(userId: String) {
        loadProfile(userId, forceRefresh = true)
    }
}

// Profile Activity/Fragment
class ProfileFragment : Fragment() {
    private lateinit var binding: FragmentProfileBinding
    private lateinit var viewModel: UserProfileViewModel
    
    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        
        setupObservers()
        
        val userId = getCurrentUserId()
        viewModel.loadProfile(userId)
        
        // Pull-to-refresh
        binding.swipeRefresh.setOnRefreshListener {
            viewModel.refreshProfile(userId)
        }
    }
    
    private fun setupObservers() {
        viewModel.profile.observe(viewLifecycleOwner) { profile ->
            updateUI(profile)
        }
        
        viewModel.isLoading.observe(viewLifecycleOwner) { isLoading ->
            binding.swipeRefresh.isRefreshing = isLoading
        }
        
        viewModel.error.observe(viewLifecycleOwner) { error ->
            if (error != null) {
                showError(error)
            }
        }
    }
    
    private fun updateUI(profile: UserProfile) {
        binding.apply {
            textName.text = profile.displayName
            textEmail.text = profile.Email
            textVerificationStatus.text = profile.verificationStatus
            
            // Set verification status color
            textVerificationStatus.setTextColor(
                ContextCompat.getColor(
                    requireContext(),
                    if (profile.IsEmailVerified) R.color.success_green else R.color.warning_orange
                )
            )
            
            // Profile picture with initials
            textInitials.text = profile.initials
            
            // Show verification badge
            iconVerified.visibility = if (profile.IsEmailVerified) View.VISIBLE else View.GONE
        }
    }
}
```

---

## **📍 Location-Based Features**

### **Google Maps Integration:**
```kotlin
class LocationMapActivity : AppCompatActivity(), OnMapReadyCallback {
    private lateinit var map: GoogleMap
    private lateinit var viewModel: LocationHistoryViewModel
    
    override fun onMapReady(googleMap: GoogleMap) {
        map = googleMap
        
        viewModel.locations.observe(this) { locations ->
            displayLocationsOnMap(locations)
        }
        
        val userId = getCurrentUserId()
        viewModel.loadLocationHistory(userId)
    }
    
    private fun displayLocationsOnMap(locations: List<UserLocationHistory>) {
        map.clear()
        
        if (locations.isEmpty()) return
        
        // Add markers for each location
        locations.forEachIndexed { index, location ->
            val marker = map.addMarker(
                MarkerOptions()
                    .position(location.latLng)
                    .title("Location ${index + 1}")
                    .snippet(location.formattedDate)
            )
        }
        
        // Draw path between locations
        if (locations.size > 1) {
            val polylineOptions = PolylineOptions()
                .color(ContextCompat.getColor(this, R.color.primary_blue))
                .width(8f)
                .geodesic(true)
            
            locations.forEach { location ->
                polylineOptions.add(location.latLng)
            }
            
            map.addPolyline(polylineOptions)
        }
        
        // Focus on most recent location
        val mostRecent = locations.first()
        map.animateCamera(
            CameraUpdateFactory.newLatLngZoom(mostRecent.latLng, 15f)
        )
    }
}
```

### **Geofencing for Location Tracking:**
```kotlin
class GeofenceManager(private val context: Context) {
    private val geofencingClient = LocationServices.getGeofencingClient(context)
    
    fun addGeofenceForUserLocation(location: UserLocationHistory, radiusMeters: Float = 100f) {
        val geofence = Geofence.Builder()
            .setRequestId("user_location_${location.Id}")
            .setCircularRegion(location.Latitude, location.Longitude, radiusMeters)
            .setExpirationDuration(Geofence.NEVER_EXPIRE)
            .setTransitionTypes(Geofence.GEOFENCE_TRANSITION_ENTER or Geofence.GEOFENCE_TRANSITION_EXIT)
            .build()
        
        val geofencingRequest = GeofencingRequest.Builder()
            .setInitialTrigger(GeofencingRequest.INITIAL_TRIGGER_ENTER)
            .addGeofence(geofence)
            .build()
        
        val geofencePendingIntent = PendingIntent.getBroadcast(
            context,
            0,
            Intent(context, GeofenceBroadcastReceiver::class.java),
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )
        
        if (ContextCompat.checkSelfPermission(
                context,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) == PackageManager.PERMISSION_GRANTED
        ) {
            geofencingClient.addGeofences(geofencingRequest, geofencePendingIntent)
        }
    }
}
```

---

## **🔄 Location Sync Strategies**

### **Periodic Background Sync:**
```kotlin
class LocationSyncManager {
    fun setupPeriodicSync(context: Context) {
        val constraints = Constraints.Builder()
            .setRequiredNetworkType(NetworkType.CONNECTED)
            .setRequiresBatteryNotLow(true)
            .build()
        
        val locationSyncWork = PeriodicWorkRequestBuilder<LocationSyncWorker>(
            repeatInterval = 15,
            repeatIntervalTimeUnit = TimeUnit.MINUTES
        )
            .setConstraints(constraints)
            .setBackoffCriteria(
                BackoffPolicy.LINEAR,
                WorkRequest.MIN_BACKOFF_MILLIS,
                TimeUnit.MILLISECONDS
            )
            .build()
        
        WorkManager.getInstance(context).enqueueUniquePeriodicWork(
            "location_sync",
            ExistingPeriodicWorkPolicy.REPLACE,
            locationSyncWork
        )
    }
}
```

### **Real-time Location Updates:**
```kotlin
class RealTimeLocationTracker(private val context: Context) {
    private val locationCallback = object : LocationCallback() {
        override fun onLocationResult(locationResult: LocationResult) {
            locationResult.lastLocation?.let { location ->
                // Sync immediately if significant movement
                val lastKnownLocation = getLastSyncedLocation()
                if (lastKnownLocation == null || 
                    location.distanceTo(lastKnownLocation) > 50) { // 50 meters
                    syncLocationImmediately(location)
                }
            }
        }
    }
    
    fun startRealTimeTracking() {
        val locationRequest = LocationRequest.create().apply {
            interval = 60000 // 1 minute
            fastestInterval = 30000 // 30 seconds
            priority = LocationRequest.PRIORITY_HIGH_ACCURACY
        }
        
        if (hasLocationPermission()) {
            LocationServices.getFusedLocationProviderClient(context)
                .requestLocationUpdates(locationRequest, locationCallback, Looper.getMainLooper())
        }
    }
}
```

---

### **4. Edit User Profile** 
**`PUT /api/UserLocation/EditUser/{id}`**

**Purpose:** Update user profile information (name, email, phone)

**Authentication:** 🔒 JWT Token Required

**URL Parameter:**
- `id`: User ID (GUID format) - **Required**

**Request Body:**
```json
{
  "Name": "Updated Name",
  "Email": "updated.email@example.com", 
  "Phone": "+1234567890"
}
```

**Field Validation:**
- `Name`: Optional string, will keep existing if null/empty
- `Email`: Optional string with email format validation, will keep existing if null/empty  
- `Phone`: Optional string, will keep existing if null/empty

**Success Response (200):**
```json
{
  "Message": "User updated successfully"
}
```

**Error Responses:**
```json
// 404 - User not found
{
  "Message": "User not found"
}

// 400 - Invalid email format
{
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

**Android Kotlin Implementation:**
```kotlin
data class EditUserRequest(
    val Name: String? = null,
    val Email: String? = null,
    val Phone: String? = null
) {
    fun isValid(): Pair<Boolean, String?> {
        // Validate email format if provided
        if (!Email.isNullOrBlank() && !android.util.Patterns.EMAIL_ADDRESS.matcher(Email).matches()) {
            return false to "Invalid email format"
        }
        
        // Validate phone format if provided
        if (!Phone.isNullOrBlank() && !isValidPhoneNumber(Phone)) {
            return false to "Invalid phone number format"
        }
        
        // At least one field should be provided
        if (Name.isNullOrBlank() && Email.isNullOrBlank() && Phone.isNullOrBlank()) {
            return false to "At least one field must be provided for update"
        }
        
        return true to null
    }
    
    private fun isValidPhoneNumber(phone: String): Boolean {
        val phonePattern = "^[+]?[1-9]\\d{1,14}\$".toRegex()
        return phonePattern.matches(phone.replace(" ", "").replace("-", ""))
    }
}

data class EditUserResponse(
    val Message: String
)

interface UserApi {
    @PUT("api/UserLocation/EditUser/{id}")
    suspend fun editUser(
        @Header("Authorization") token: String,
        @Path("id") userId: String,
        @Body request: EditUserRequest
    ): Response<EditUserResponse>
}

// Repository
class UserEditRepository {
    private val userApi = ApiClient.retrofit.create(UserApi::class.java)
    
    suspend fun updateUser(userId: String, editRequest: EditUserRequest): Result<String> {
        return try {
            // Validate request first
            val (isValid, errorMessage) = editRequest.isValid()
            if (!isValid) {
                return Result.failure(Exception(errorMessage ?: "Invalid request"))
            }
            
            val token = "Bearer ${getAuthToken()}"
            val response = userApi.editUser(token, userId, editRequest)
            
            if (response.isSuccessful) {
                val message = response.body()?.Message ?: "User updated successfully"
                
                // Clear cached profile to force refresh
                clearCachedProfile()
                
                Result.success(message)
            } else {
                val errorBody = response.errorBody()?.string()
                val errorMessage = parseErrorMessage(errorBody) ?: "Update failed"
                Result.failure(Exception(errorMessage))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private fun parseErrorMessage(errorBody: String?): String? {
        return try {
            if (errorBody != null) {
                val gson = Gson()
                val errorResponse = gson.fromJson(errorBody, JsonObject::class.java)
                
                // Handle validation errors
                if (errorResponse.has("errors")) {
                    val errors = errorResponse.getAsJsonObject("errors")
                    val errorMessages = mutableListOf<String>()
                    
                    errors.entrySet().forEach { entry ->
                        val fieldErrors = entry.value.asJsonArray
                        fieldErrors.forEach { error ->
                            errorMessages.add("${entry.key}: ${error.asString}")
                        }
                    }
                    
                    return errorMessages.joinToString("\n")
                }
                
                // Handle simple message errors
                if (errorResponse.has("Message")) {
                    return errorResponse.get("Message").asString
                }
            }
            null
        } catch (e: Exception) {
            null
        }
    }
    
    private fun clearCachedProfile() {
        val sharedPrefs = context.getSharedPreferences("user_profile", Context.MODE_PRIVATE)
        sharedPrefs.edit().remove("profile_data").apply()
    }
}

// ViewModel for Edit Profile
class EditProfileViewModel : ViewModel() {
    private val repository = UserEditRepository()
    
    private val _updateStatus = MutableLiveData<UpdateStatus>()
    val updateStatus: LiveData<UpdateStatus> = _updateStatus
    
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    fun updateUserProfile(
        userId: String, 
        name: String? = null,
        email: String? = null, 
        phone: String? = null
    ) {
        _isLoading.value = true
        
        val request = EditUserRequest(
            Name = name?.trim()?.takeIf { it.isNotEmpty() },
            Email = email?.trim()?.takeIf { it.isNotEmpty() },
            Phone = phone?.trim()?.takeIf { it.isNotEmpty() }
        )
        
        viewModelScope.launch {
            repository.updateUser(userId, request)
                .onSuccess { message ->
                    _updateStatus.value = UpdateStatus.Success(message)
                    _isLoading.value = false
                }
                .onFailure { exception ->
                    _updateStatus.value = UpdateStatus.Error(exception.message ?: "Update failed")
                    _isLoading.value = false
                }
        }
    }
    
    fun clearUpdateStatus() {
        _updateStatus.value = null
    }
}

sealed class UpdateStatus {
    data class Success(val message: String) : UpdateStatus()
    data class Error(val message: String) : UpdateStatus()
}

// Edit Profile Activity/Fragment
class EditProfileFragment : Fragment() {
    private lateinit var binding: FragmentEditProfileBinding
    private lateinit var viewModel: EditProfileViewModel
    private var currentProfile: UserProfile? = null
    
    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        
        setupUI()
        setupObservers()
        
        // Load current profile
        currentProfile = arguments?.getParcelable("user_profile")
        populateCurrentData()
    }
    
    private fun setupUI() {
        binding.apply {
            // Save button click
            buttonSave.setOnClickListener {
                saveProfile()
            }
            
            // Cancel button click
            buttonCancel.setOnClickListener {
                findNavController().navigateUp()
            }
            
            // Input validation
            setupInputValidation()
        }
    }
    
    private fun setupInputValidation() {
        binding.apply {
            // Real-time email validation
            editEmail.addTextChangedListener(object : TextWatcher {
                override fun afterTextChanged(s: Editable?) {
                    validateEmail(s.toString())
                }
                override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) {}
                override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) {}
            })
            
            // Real-time phone validation
            editPhone.addTextChangedListener(object : TextWatcher {
                override fun afterTextChanged(s: Editable?) {
                    validatePhone(s.toString())
                }
                override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) {}
                override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) {}
            })
        }
    }
    
    private fun validateEmail(email: String): Boolean {
        val isValid = email.isEmpty() || android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches()
        
        binding.textEmailError.apply {
            if (!isValid) {
                text = "Invalid email format"
                visibility = View.VISIBLE
            } else {
                visibility = View.GONE
            }
        }
        
        return isValid
    }
    
    private fun validatePhone(phone: String): Boolean {
        val isValid = phone.isEmpty() || phone.matches("^[+]?[1-9]\\d{1,14}$".toRegex())
        
        binding.textPhoneError.apply {
            if (!isValid) {
                text = "Invalid phone number format"
                visibility = View.VISIBLE
            } else {
                visibility = View.GONE
            }
        }
        
        return isValid
    }
    
    private fun populateCurrentData() {
        currentProfile?.let { profile ->
            binding.apply {
                editName.setText(profile.Name)
                editEmail.setText(profile.Email)
                // Phone not available in current profile, leave empty
            }
        }
    }
    
    private fun saveProfile() {
        binding.apply {
            val name = editName.text.toString().trim()
            val email = editEmail.text.toString().trim()
            val phone = editPhone.text.toString().trim()
            
            // Validate inputs
            val isEmailValid = validateEmail(email)
            val isPhoneValid = validatePhone(phone)
            
            if (!isEmailValid || !isPhoneValid) {
                showError("Please fix validation errors before saving")
                return
            }
            
            // Check if any changes were made
            val hasChanges = name != currentProfile?.Name ||
                           email != currentProfile?.Email ||
                           phone.isNotEmpty()
            
            if (!hasChanges) {
                showError("No changes to save")
                return
            }
            
            // Update profile
            val userId = getCurrentUserId()
            viewModel.updateUserProfile(
                userId = userId,
                name = if (name != currentProfile?.Name) name else null,
                email = if (email != currentProfile?.Email) email else null,
                phone = if (phone.isNotEmpty()) phone else null
            )
        }
    }
    
    private fun setupObservers() {
        viewModel.updateStatus.observe(viewLifecycleOwner) { status ->
            when (status) {
                is UpdateStatus.Success -> {
                    showSuccess(status.message)
                    // Navigate back after successful update
                    findNavController().navigateUp()
                }
                is UpdateStatus.Error -> {
                    showError(status.message)
                }
                null -> {} // No status
            }
        }
        
        viewModel.isLoading.observe(viewLifecycleOwner) { isLoading ->
            binding.apply {
                progressBar.visibility = if (isLoading) View.VISIBLE else View.GONE
                buttonSave.isEnabled = !isLoading
                buttonCancel.isEnabled = !isLoading
                
                // Disable input fields while loading
                editName.isEnabled = !isLoading
                editEmail.isEnabled = !isLoading
                editPhone.isEnabled = !isLoading
            }
        }
    }
    
    private fun showSuccess(message: String) {
        Snackbar.make(binding.root, message, Snackbar.LENGTH_LONG)
            .setBackgroundTint(ContextCompat.getColor(requireContext(), R.color.success_green))
            .show()
    }
    
    private fun showError(message: String) {
        Snackbar.make(binding.root, message, Snackbar.LENGTH_LONG)
            .setBackgroundTint(ContextCompat.getColor(requireContext(), R.color.error_red))
            .show()
    }
}

// Usage in Profile Screen
class ProfileFragment : Fragment() {
    private fun setupEditProfileButton() {
        binding.buttonEditProfile.setOnClickListener {
            val currentProfile = viewModel.profile.value
            if (currentProfile != null) {
                val bundle = bundleOf("user_profile" to currentProfile)
                findNavController().navigate(
                    R.id.action_profile_to_editProfile,
                    bundle
                )
            }
        }
    }
}

// Form Validation Utility
object ProfileValidationUtils {
    fun validateProfileUpdate(
        name: String?,
        email: String?, 
        phone: String?
    ): ValidationResult {
        val errors = mutableListOf<String>()
        
        // Check if at least one field has content
        if (name.isNullOrBlank() && email.isNullOrBlank() && phone.isNullOrBlank()) {
            errors.add("At least one field must be provided")
        }
        
        // Validate email format
        if (!email.isNullOrBlank() && !android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
            errors.add("Invalid email format")
        }
        
        // Validate phone format
        if (!phone.isNullOrBlank()) {
            val cleanPhone = phone.replace(" ", "").replace("-", "")
            if (!cleanPhone.matches("^[+]?[1-9]\\d{1,14}$".toRegex())) {
                errors.add("Invalid phone number format")
            }
        }
        
        return ValidationResult(
            isValid = errors.isEmpty(),
            errors = errors
        )
    }
}

data class ValidationResult(
    val isValid: Boolean,
    val errors: List<String>
)
```

---

**User Location & Profile Complete! ✅**  
Next: [Android Integration Guide →](ANDROID_INTEGRATION_GUIDE.md)