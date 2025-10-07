# 🗑️ **Delete Chat API - Permanent Chat Deletion**

## 📋 **Overview**
New API endpoint to permanently delete entire chat conversations between two users. All messages between the specified users will be removed from the database.

---

## 🚨 **⚠️ IMPORTANT WARNING**
**This action is PERMANENT and IRREVERSIBLE!**
- ❌ **No recovery possible** - All messages are permanently deleted from database
- ❌ **Both users affected** - Chat disappears for both participants
- ❌ **No undo functionality** - Use with extreme caution

---

## 📡 **API Endpoint**

### **Delete Entire Chat**
**`DELETE /api/Chat/DeleteChat/{userId}/{otherUserId}`**

**URL Parameters:**
- `userId` (Guid) - ID of the user requesting deletion
- `otherUserId` (Guid) - ID of the other user in the chat

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Chat deleted permanently",
  "deletedMessagesCount": 25,
  "deletedBetween": {
    "user1Name": "John Doe",
    "user2Name": "Jane Smith"
  }
}
```

**Error Responses:**

**404 Not Found - User not found:**
```json
{
  "success": false,
  "message": "User not found",
  "deletedMessagesCount": 0,
  "deletedBetween": null
}
```

**404 Not Found - No chat exists:**
```json
{
  "success": false,
  "message": "No chat found between these users",
  "deletedMessagesCount": 0,
  "deletedBetween": null
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "Failed to delete chat",
  "deletedMessagesCount": 0,
  "deletedBetween": null
}
```

---

## 🔧 **Implementation Examples**

### **Android Integration**

```kotlin
// API Response Model
data class DeleteChatResponse(
    val success: Boolean,
    val message: String,
    val deletedMessagesCount: Int,
    val deletedBetween: ChatParticipants?
)

data class ChatParticipants(
    val user1Name: String,
    val user2Name: String
)

// API Interface
@DELETE("api/Chat/DeleteChat/{userId}/{otherUserId}")
suspend fun deleteChat(
    @Path("userId") userId: String,
    @Path("otherUserId") otherUserId: String
): Response<DeleteChatResponse>

// Repository Implementation
class ChatRepository {
    
    suspend fun deleteChat(userId: String, otherUserId: String): Result<DeleteChatResponse> {
        return try {
            val response = chatApi.deleteChat(userId, otherUserId)
            
            if (response.isSuccessful && response.body()?.success == true) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception(response.body()?.message ?: "Failed to delete chat"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}

// ViewModel with Confirmation Dialog
class ChatViewModel : ViewModel() {
    
    fun confirmDeleteChat(otherUserId: String) {
        // Show confirmation dialog first
        _uiState.value = _uiState.value.copy(
            showDeleteConfirmation = true,
            pendingDeleteUserId = otherUserId
        )
    }
    
    fun deleteChat(otherUserId: String) {
        viewModelScope.launch {
            try {
                _uiState.value = _uiState.value.copy(isLoading = true)
                
                val currentUserId = getCurrentUserId()
                
                val result = chatRepository.deleteChat(
                    userId = currentUserId,
                    otherUserId = otherUserId
                )
                
                result.fold(
                    onSuccess = { response ->
                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            showDeleteConfirmation = false,
                            message = "${response.deletedMessagesCount} messages deleted permanently"
                        )
                        
                        // Navigate back or refresh chat list
                        navigateBack()
                    },
                    onFailure = { error ->
                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            showDeleteConfirmation = false,
                            error = error.message ?: "Failed to delete chat"
                        )
                    }
                )
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    showDeleteConfirmation = false,
                    error = "An error occurred while deleting chat"
                )
            }
        }
    }
}

// UI Implementation with Confirmation
@Composable
fun ChatScreen(
    otherUserId: String,
    viewModel: ChatViewModel
) {
    val uiState by viewModel.uiState.collectAsState()
    
    // Confirmation Dialog
    if (uiState.showDeleteConfirmation) {
        AlertDialog(
            onDismissRequest = { viewModel.dismissDeleteConfirmation() },
            title = { Text("Delete Chat Permanently?") },
            text = { 
                Text(
                    "This will permanently delete all messages between you and this user. " +
                    "This action cannot be undone.",
                    color = MaterialTheme.colors.error
                )
            },
            confirmButton = {
                TextButton(
                    onClick = { viewModel.deleteChat(otherUserId) },
                    colors = ButtonDefaults.textButtonColors(
                        contentColor = MaterialTheme.colors.error
                    )
                ) {
                    Text("DELETE PERMANENTLY")
                }
            },
            dismissButton = {
                TextButton(onClick = { viewModel.dismissDeleteConfirmation() }) {
                    Text("Cancel")
                }
            }
        )
    }
    
    // Chat Options Menu
    TopAppBar(
        title = { Text("Chat") },
        actions = {
            IconButton(onClick = { viewModel.confirmDeleteChat(otherUserId) }) {
                Icon(
                    Icons.Default.Delete,
                    contentDescription = "Delete Chat",
                    tint = MaterialTheme.colors.error
                )
            }
        }
    )
}
```

### **React Integration**

```javascript
// Chat Service
class ChatService {
    static async deleteChat(userId, otherUserId) {
        const response = await fetch(`/api/Chat/DeleteChat/${userId}/${otherUserId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        return await response.json();
    }
}

// React Hook
function useChatDeletion() {
    const [isDeleting, setIsDeleting] = useState(false);
    const [deleteError, setDeleteError] = useState(null);
    
    const deleteChat = async (userId, otherUserId) => {
        setIsDeleting(true);
        setDeleteError(null);
        
        try {
            const response = await ChatService.deleteChat(userId, otherUserId);
            
            if (response.success) {
                return {
                    success: true,
                    deletedCount: response.deletedMessagesCount,
                    participants: response.deletedBetween
                };
            } else {
                throw new Error(response.message);
            }
        } catch (error) {
            setDeleteError(error.message);
            return { success: false };
        } finally {
            setIsDeleting(false);
        }
    };
    
    return { deleteChat, isDeleting, deleteError };
}

// Chat Component with Confirmation
function ChatPage({ otherUserId }) {
    const [showDeleteDialog, setShowDeleteDialog] = useState(false);
    const { deleteChat, isDeleting, deleteError } = useChatDeletion();
    const currentUserId = getCurrentUserId();
    
    const handleDeleteChat = async () => {
        const result = await deleteChat(currentUserId, otherUserId);
        
        if (result.success) {
            alert(`Chat deleted! ${result.deletedCount} messages removed permanently.`);
            // Navigate away or refresh
            window.history.back();
        }
        
        setShowDeleteDialog(false);
    };
    
    return (
        <div className="chat-page">
            <header className="chat-header">
                <h2>Chat</h2>
                <button 
                    className="delete-chat-btn"
                    onClick={() => setShowDeleteDialog(true)}
                    style={{ color: 'red' }}
                >
                    🗑️ Delete Chat
                </button>
            </header>
            
            {/* Confirmation Dialog */}
            {showDeleteDialog && (
                <div className="modal">
                    <div className="modal-content">
                        <h3>⚠️ Delete Chat Permanently?</h3>
                        <p>
                            This will permanently delete all messages between you and this user.
                            <strong> This action cannot be undone.</strong>
                        </p>
                        
                        <div className="modal-actions">
                            <button 
                                onClick={() => setShowDeleteDialog(false)}
                                disabled={isDeleting}
                            >
                                Cancel
                            </button>
                            <button 
                                onClick={handleDeleteChat}
                                disabled={isDeleting}
                                style={{ 
                                    backgroundColor: 'red', 
                                    color: 'white' 
                                }}
                            >
                                {isDeleting ? 'Deleting...' : 'DELETE PERMANENTLY'}
                            </button>
                        </div>
                        
                        {deleteError && (
                            <p className="error">{deleteError}</p>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
```

---

## 🔒 **Security Features**

### **✅ Validation Checks**
1. **User Existence**: Both users must exist in database
2. **Chat Existence**: Verifies chat exists before deletion
3. **Comprehensive Logging**: All deletion attempts are logged
4. **Error Handling**: Proper error responses for all scenarios

### **🔍 Database Impact**
- **Complete Removal**: All messages between users permanently deleted
- **Transactional**: Uses database transaction for consistency
- **No Orphaned Data**: Clean removal with no leftover references

---

## 🎯 **Use Cases**

### **✅ When to Use**
- User wants to clear chat history completely
- Privacy concerns - remove all conversation traces
- Clean slate - start fresh conversation
- Account cleanup before deactivation

### **❌ When NOT to Use**
- Temporary hide (use archive feature instead)
- Accidental deletion concerns (provide clear warnings)
- Legal/compliance requirements (consider data retention policies)

---

## 🧪 **Testing**

### **Test Scenarios:**

**1. Successful Deletion**
```bash
# Delete existing chat
DELETE https://resellbook20250929183655.azurewebsites.net/api/Chat/DeleteChat/{userId}/{otherUserId}

# Expected: 200 OK with deletion confirmation
```

**2. Non-existent User**
```bash
# Try with invalid user ID
DELETE https://resellbook20250929183655.azurewebsites.net/api/Chat/DeleteChat/00000000-0000-0000-0000-000000000000/{otherUserId}

# Expected: 404 Not Found
```

**3. No Chat Exists**
```bash
# Try with users who never chatted
DELETE https://resellbook20250929183655.azurewebsites.net/api/Chat/DeleteChat/{userId}/{otherUserId}

# Expected: 404 Not Found - No chat found
```

---

## 📋 **API Summary**

| Feature | Details |
|---------|---------|
| **Endpoint** | `DELETE /api/Chat/DeleteChat/{userId}/{otherUserId}` |
| **Purpose** | Permanently delete all messages between two users |
| **Security** | User validation, existence checks |
| **Response** | Deletion confirmation with count |
| **Logging** | Full audit trail |
| **Reversible** | ❌ **NO** - Permanent deletion |

---

## 🚨 **Important Reminders**

1. **⚠️ PERMANENT**: No way to recover deleted messages
2. **👥 AFFECTS BOTH**: Both users lose access to conversation
3. **🔒 SECURE**: Proper validation and error handling
4. **📊 TRACKED**: Full logging for audit purposes
5. **⚡ INSTANT**: Immediate effect across all platforms

---

**✨ Your delete chat API is ready for secure, permanent chat removal!** 🗑️