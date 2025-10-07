# 💬 **Chat API Documentation**
*Complete messaging system for user-to-user communication*

---

## **📋 Table of Contents**
1. [Overview](#overview)
2. [API Endpoints](#api-endpoints)
3. [Data Models](#data-models)
4. [Android Implementation](#android-implementation)
5. [React/Frontend Implementation](#react-frontend-implementation)
6. [Real-time Features](#real-time-features)

---

## **🔍 Overview**

The Chat API provides a complete messaging system allowing users to:
- Send messages to other users
- View chat lists with unread counts
- Mark messages as read
- Get message history with pagination
- Delete their own messages

**Key Features:**
- ✅ User-to-user messaging
- ✅ Unread message tracking
- ✅ Message status (sent, read)
- ✅ Pagination support
- ✅ Performance optimized with database indexes

---

## **🚀 API Endpoints**

### **1. Send Message**
**`POST /api/Chat/SendMessage/{senderId}`**

**Purpose:** Send a message from one user to another

**Authentication:** Required (validate senderId matches authenticated user)

**URL Parameters:**
- `senderId`: Sender's User ID (GUID) - **Required**

**Request Body:**
```json
{
  "BookId": "123e4567-e89b-12d3-a456-426614174000",
  "Message": "Hello! Is this book still available?"
}
```

**✨ UPDATED:** Now uses `BookId` instead of `ReceiverId` - system automatically finds book owner!

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
    "Message": "Hello! Is this book still available?",
    "SentAt": "2025-10-04T13:45:00Z",
    "IsRead": false,
    "ReadAt": null,
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

**✨ ENHANCED:** Now includes `ReceiverName` and `BookContext` for better user experience!

**Error Responses:**
```json
// 404 - Sender not found
{
  "Success": false,
  "Message": "Sender not found"
}

// 404 - Receiver not found
{
  "Success": false,
  "Message": "Receiver not found"
}

// 500 - Server error
{
  "Success": false,
  "Message": "Failed to send message"
}
```

---

### **2. Get Chat List**
**`GET /api/Chat/GetChats/{userId}`**

**Purpose:** Get all chat conversations for a user with unread counts

**URL Parameters:**
- `userId`: User ID (GUID) - **Required**

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Chats retrieved successfully",
  "Chats": [
    {
      "UserId": "123e4567-e89b-12d3-a456-426614174000",
      "UserName": "Alice Smith",
      "LastMessage": "Thanks for the book!",
      "LastMessageTime": "2025-10-04T13:45:00Z",
      "UnreadCount": 2,
      "IsOnline": false
    },
    {
      "UserId": "789e4567-e89b-12d3-a456-426614174000",
      "UserName": "Bob Johnson",
      "LastMessage": "Is it still available?",
      "LastMessageTime": "2025-10-04T12:30:00Z",
      "UnreadCount": 0,
      "IsOnline": true
    }
  ]
}
```

---

### **3. Get Chat Messages**
**`GET /api/Chat/GetChatMessages/{userId}/{otherUserId}`**

**Purpose:** Get message history between two users with pagination

**URL Parameters:**
- `userId`: Current user ID (GUID) - **Required**
- `otherUserId`: Other user ID (GUID) - **Required**

**Query Parameters:**
- `page`: Page number (default: 1) - **Optional**
- `pageSize`: Messages per page (default: 50) - **Optional**

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Messages retrieved successfully",
  "Messages": [
    {
      "Id": "111e4567-e89b-12d3-a456-426614174000",
      "SenderId": "456e4567-e89b-12d3-a456-426614174000",
      "ReceiverId": "123e4567-e89b-12d3-a456-426614174000",
      "SenderName": "John Doe",
      "Message": "Hello! Is this book still available?",
      "SentAt": "2025-10-04T12:00:00Z",
      "IsRead": true,
      "ReadAt": "2025-10-04T12:05:00Z",
      "IsSentByMe": true
    },
    {
      "Id": "222e4567-e89b-12d3-a456-426614174000",
      "SenderId": "123e4567-e89b-12d3-a456-426614174000",
      "ReceiverId": "456e4567-e89b-12d3-a456-426614174000",
      "SenderName": "Alice Smith",
      "Message": "Yes, it's available for $25",
      "SentAt": "2025-10-04T12:05:00Z",
      "IsRead": false,
      "ReadAt": null,
      "IsSentByMe": false
    }
  ]
}
```

---

### **4. Mark Messages as Read**
**`PUT /api/Chat/MarkAsRead/{userId}`**

**Purpose:** Mark all messages from another user as read (call when user opens chat)

**URL Parameters:**
- `userId`: Current user ID (GUID) - **Required**

**Request Body:**
```json
{
  "OtherUserId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Marked 3 messages as read",
  "UpdatedCount": 3
}
```

---

### **5. Get Unread Count**
**`GET /api/Chat/GetUnreadCount/{userId}`**

**Purpose:** Get total unread message count for a user (for badge notifications)

**URL Parameters:**
- `userId`: User ID (GUID) - **Required**

**Success Response (200):**
```json
{
  "Success": true,
  "UnreadCount": 5
}
```

---

### **6. Delete Message**
**`DELETE /api/Chat/DeleteMessage/{messageId}/{userId}`**

**Purpose:** Delete a message (only sender can delete their own messages)

**URL Parameters:**
- `messageId`: Message ID (GUID) - **Required**
- `userId`: User ID requesting deletion (GUID) - **Required**

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Message deleted successfully"
}
```

**Error Responses:**
```json
// 404 - Message not found
{
  "Success": false,
  "Message": "Message not found"
}

// 403 - Not authorized
"You can only delete your own messages"
```

---

### **7. Delete Entire Chat (NEW)**
**`DELETE /api/Chat/DeleteChat/{userId}/{otherUserId}`**

**⚠️ WARNING:** This permanently deletes ALL messages between two users. This action is IRREVERSIBLE!

**Purpose:** Permanently delete entire chat conversation between two users

**URL Parameters:**
- `userId`: Current user ID (GUID) - **Required**
- `otherUserId`: Other user ID (GUID) - **Required**

**Success Response (200):**
```json
{
  "Success": true,
  "Message": "Chat deleted permanently",
  "DeletedMessagesCount": 25,
  "DeletedBetween": {
    "User1Name": "John Doe",
    "User2Name": "Jane Smith"
  }
}
```

**Error Responses:**
```json
// 404 - User not found
{
  "Success": false,
  "Message": "User not found",
  "DeletedMessagesCount": 0,
  "DeletedBetween": null
}

// 404 - No chat exists
{
  "Success": false,
  "Message": "No chat found between these users",
  "DeletedMessagesCount": 0,
  "DeletedBetween": null
}
```

---

### **8. Get Book for Messaging Context**
**`GET /api/Chat/GetBookForMessage/{bookId}`**

**Purpose:** Get book details for messaging context (helpful when using BookId-based messaging)

**URL Parameters:**
- `bookId`: Book ID (GUID) - **Required**

**Success Response (200):**
```json
{
  "BookId": "123e4567-e89b-12d3-a456-426614174000",
  "BookName": "Advanced Mathematics",
  "BookOwnerName": "Jane Smith",
  "SellingPrice": 450.00
}
```

**Error Response (404):**
```json
{
  "Message": "Book not found"
}
```

---

## **📱 Android Implementation**

### **Data Models:**
```kotlin
// ✨ UPDATED: Uses BookId instead of ReceiverId
data class SendMessageRequest(
    val BookId: String,        // Changed from ReceiverId
    val Message: String
)

// ✨ NEW: Book context information
data class BookContext(
    val BookId: String,
    val BookName: String,
    val BookOwnerName: String,
    val SellingPrice: Double
)

// ✨ NEW: Delete chat response
data class DeleteChatResponse(
    val Success: Boolean,
    val Message: String,
    val DeletedMessagesCount: Int,
    val DeletedBetween: ChatParticipants?
)

data class ChatParticipants(
    val User1Name: String,
    val User2Name: String
)

data class ChatMessage(
    val Id: String,
    val SenderId: String,
    val ReceiverId: String,
    val SenderName: String,
    val ReceiverName: String,    // ✨ NEW: Added receiver name
    val Message: String,
    val SentAt: String,
    val IsRead: Boolean,
    val ReadAt: String?,
    val IsSentByMe: Boolean
) {
    val formattedTime: String
        get() = try {
            val inputFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.getDefault())
            val outputFormat = SimpleDateFormat("HH:mm", Locale.getDefault())
            val date = inputFormat.parse(SentAt)
            outputFormat.format(date ?: Date())
        } catch (e: Exception) {
            SentAt
        }
}

data class ChatListItem(
    val UserId: String,
    val UserName: String,
    val LastMessage: String?,
    val LastMessageTime: String?,
    val UnreadCount: Int,
    val IsOnline: Boolean
) {
    val hasUnread: Boolean
        get() = UnreadCount > 0
        
    val displayTime: String
        get() = LastMessageTime?.let { 
            try {
                val inputFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", Locale.getDefault())
                val date = inputFormat.parse(it)
                when {
                    DateUtils.isToday(date?.time ?: 0) -> {
                        SimpleDateFormat("HH:mm", Locale.getDefault()).format(date!!)
                    }
                    DateUtils.isToday(date?.time ?: 0 + DateUtils.DAY_IN_MILLIS) -> "Yesterday"
                    else -> SimpleDateFormat("MMM dd", Locale.getDefault()).format(date!!)
                }
            } catch (e: Exception) {
                it
            }
        } ?: ""
}

data class MarkAsReadRequest(
    val OtherUserId: String
)
```

### **API Service:**
```kotlin
interface ChatApi {
    @POST("api/Chat/SendMessage/{senderId}")
    suspend fun sendMessage(
        @Path("senderId") senderId: String,
        @Body request: SendMessageRequest
    ): Response<SendMessageResponse>
    
    @GET("api/Chat/GetChats/{userId}")
    suspend fun getChats(
        @Path("userId") userId: String
    ): Response<ChatListResponse>
    
    @GET("api/Chat/GetChatMessages/{userId}/{otherUserId}")
    suspend fun getChatMessages(
        @Path("userId") userId: String,
        @Path("otherUserId") otherUserId: String,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 50
    ): Response<ChatResponse>
    
    @PUT("api/Chat/MarkAsRead/{userId}")
    suspend fun markAsRead(
        @Path("userId") userId: String,
        @Body request: MarkAsReadRequest
    ): Response<GenericResponse>
    
    @GET("api/Chat/GetUnreadCount/{userId}")
    suspend fun getUnreadCount(
        @Path("userId") userId: String
    ): Response<UnreadCountResponse>
    
    @DELETE("api/Chat/DeleteMessage/{messageId}/{userId}")
    suspend fun deleteMessage(
        @Path("messageId") messageId: String,
        @Path("userId") userId: String
    ): Response<GenericResponse>
    
    // ✨ NEW: Delete entire chat
    @DELETE("api/Chat/DeleteChat/{userId}/{otherUserId}")
    suspend fun deleteChat(
        @Path("userId") userId: String,
        @Path("otherUserId") otherUserId: String
    ): Response<DeleteChatResponse>
    
    // ✨ NEW: Get book context for messaging
    @GET("api/Chat/GetBookForMessage/{bookId}")
    suspend fun getBookForMessage(
        @Path("bookId") bookId: String
    ): Response<BookContext>
}
```

### **Repository:**
```kotlin
class ChatRepository {
    private val chatApi = ApiClient.retrofit.create(ChatApi::class.java)
    
    suspend fun sendMessage(senderId: String, receiverId: String, message: String): Result<ChatMessage> {
        return try {
            val request = SendMessageRequest(receiverId, message)
            val response = chatApi.sendMessage(senderId, request)
            
            if (response.isSuccessful && response.body()?.Success == true) {
                val chatMessage = response.body()!!.ChatMessage!!
                Result.success(chatMessage)
            } else {
                Result.failure(Exception(response.body()?.Message ?: "Failed to send message"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun getChats(userId: String): Result<List<ChatListItem>> {
        return try {
            val response = chatApi.getChats(userId)
            
            if (response.isSuccessful && response.body()?.Success == true) {
                val chats = response.body()!!.Chats
                Result.success(chats)
            } else {
                Result.failure(Exception(response.body()?.Message ?: "Failed to load chats"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun getChatMessages(userId: String, otherUserId: String, page: Int = 1): Result<List<ChatMessage>> {
        return try {
            val response = chatApi.getChatMessages(userId, otherUserId, page)
            
            if (response.isSuccessful && response.body()?.Success == true) {
                val messages = response.body()!!.Messages
                Result.success(messages)
            } else {
                Result.failure(Exception(response.body()?.Message ?: "Failed to load messages"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun markAsRead(userId: String, otherUserId: String): Result<Unit> {
        return try {
            val request = MarkAsReadRequest(otherUserId)
            val response = chatApi.markAsRead(userId, request)
            
            if (response.isSuccessful) {
                Result.success(Unit)
            } else {
                Result.failure(Exception("Failed to mark as read"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
```

### **ViewModels:**
```kotlin
class ChatListViewModel : ViewModel() {
    private val repository = ChatRepository()
    
    private val _chats = MutableLiveData<List<ChatListItem>>()
    val chats: LiveData<List<ChatListItem>> = _chats
    
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    private val _error = MutableLiveData<String>()
    val error: LiveData<String> = _error
    
    fun loadChats(userId: String) {
        _isLoading.value = true
        
        viewModelScope.launch {
            repository.getChats(userId)
                .onSuccess { chatList ->
                    _chats.value = chatList
                    _isLoading.value = false
                }
                .onFailure { exception ->
                    _error.value = exception.message
                    _isLoading.value = false
                }
        }
    }
    
    fun refreshChats(userId: String) {
        loadChats(userId)
    }
}

class ChatViewModel : ViewModel() {
    private val repository = ChatRepository()
    
    private val _messages = MutableLiveData<List<ChatMessage>>()
    val messages: LiveData<List<ChatMessage>> = _messages
    
    private val _isLoading = MutableLiveData<Boolean>()
    val isLoading: LiveData<Boolean> = _isLoading
    
    private val _isSending = MutableLiveData<Boolean>()
    val isSending: LiveData<Boolean> = _isSending
    
    fun loadMessages(userId: String, otherUserId: String) {
        _isLoading.value = true
        
        viewModelScope.launch {
            repository.getChatMessages(userId, otherUserId)
                .onSuccess { messageList ->
                    _messages.value = messageList
                    _isLoading.value = false
                    
                    // Mark messages as read
                    repository.markAsRead(userId, otherUserId)
                }
                .onFailure { exception ->
                    _isLoading.value = false
                }
        }
    }
    
    fun sendMessage(senderId: String, receiverId: String, message: String) {
        _isSending.value = true
        
        viewModelScope.launch {
            repository.sendMessage(senderId, receiverId, message)
                .onSuccess { newMessage ->
                    val currentMessages = _messages.value?.toMutableList() ?: mutableListOf()
                    currentMessages.add(newMessage)
                    _messages.value = currentMessages
                    _isSending.value = false
                }
                .onFailure { exception ->
                    _isSending.value = false
                }
        }
    }
}
```

### **UI Implementation:**
```kotlin
class ChatActivity : AppCompatActivity() {
    private lateinit var binding: ActivityChatBinding
    private lateinit var viewModel: ChatViewModel
    private lateinit var messagesAdapter: MessagesAdapter
    
    private lateinit var currentUserId: String
    private lateinit var otherUserId: String
    private lateinit var otherUserName: String
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityChatBinding.inflate(layoutInflater)
        setContentView(binding.root)
        
        // Get data from intent
        currentUserId = intent.getStringExtra("current_user_id") ?: return
        otherUserId = intent.getStringExtra("other_user_id") ?: return
        otherUserName = intent.getStringExtra("other_user_name") ?: "Chat"
        
        setupUI()
        setupRecyclerView()
        setupObservers()
        
        // Load messages and mark as read
        viewModel.loadMessages(currentUserId, otherUserId)
    }
    
    private fun setupUI() {
        binding.apply {
            toolbar.title = otherUserName
            toolbar.setNavigationOnClickListener { finish() }
            
            buttonSend.setOnClickListener {
                val message = editMessage.text.toString().trim()
                if (message.isNotEmpty()) {
                    viewModel.sendMessage(currentUserId, otherUserId, message)
                    editMessage.text.clear()
                }
            }
        }
    }
    
    private fun setupRecyclerView() {
        messagesAdapter = MessagesAdapter(currentUserId)
        binding.recyclerMessages.apply {
            adapter = messagesAdapter
            layoutManager = LinearLayoutManager(this@ChatActivity)
        }
    }
    
    private fun setupObservers() {
        viewModel.messages.observe(this) { messages ->
            messagesAdapter.updateMessages(messages)
            if (messages.isNotEmpty()) {
                binding.recyclerMessages.scrollToPosition(messages.size - 1)
            }
        }
        
        viewModel.isSending.observe(this) { isSending ->
            binding.buttonSend.isEnabled = !isSending
        }
    }
}

class MessagesAdapter(private val currentUserId: String) : 
    RecyclerView.Adapter<MessagesAdapter.MessageViewHolder>() {
    
    private var messages: List<ChatMessage> = emptyList()
    
    fun updateMessages(newMessages: List<ChatMessage>) {
        messages = newMessages
        notifyDataSetChanged()
    }
    
    override fun getItemViewType(position: Int): Int {
        return if (messages[position].IsSentByMe) VIEW_TYPE_SENT else VIEW_TYPE_RECEIVED
    }
    
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): MessageViewHolder {
        val inflater = LayoutInflater.from(parent.context)
        return if (viewType == VIEW_TYPE_SENT) {
            val binding = ItemMessageSentBinding.inflate(inflater, parent, false)
            SentMessageViewHolder(binding)
        } else {
            val binding = ItemMessageReceivedBinding.inflate(inflater, parent, false)
            ReceivedMessageViewHolder(binding)
        }
    }
    
    override fun onBindViewHolder(holder: MessageViewHolder, position: Int) {
        holder.bind(messages[position])
    }
    
    override fun getItemCount() = messages.size
    
    abstract class MessageViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        abstract fun bind(message: ChatMessage)
    }
    
    class SentMessageViewHolder(
        private val binding: ItemMessageSentBinding
    ) : MessageViewHolder(binding.root) {
        
        override fun bind(message: ChatMessage) {
            binding.apply {
                textMessage.text = message.Message
                textTime.text = message.formattedTime
                
                // Show read status
                iconRead.visibility = if (message.IsRead) View.VISIBLE else View.GONE
            }
        }
    }
    
    class ReceivedMessageViewHolder(
        private val binding: ItemMessageReceivedBinding
    ) : MessageViewHolder(binding.root) {
        
        override fun bind(message: ChatMessage) {
            binding.apply {
                textMessage.text = message.Message
                textTime.text = message.formattedTime
                textSenderName.text = message.SenderName
            }
        }
    }
    
    companion object {
        const val VIEW_TYPE_SENT = 1
        const val VIEW_TYPE_RECEIVED = 2
    }
}
```

---

## **💻 React/Frontend Implementation**

### **API Service:**
```javascript
class ChatService {
    static async sendMessage(senderId, receiverId, message) {
        const response = await fetch(`/api/Chat/SendMessage/${senderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                ReceiverId: receiverId,
                Message: message
            })
        });
        return await response.json();
    }
    
    static async getChats(userId) {
        const response = await fetch(`/api/Chat/GetChats/${userId}`);
        return await response.json();
    }
    
    static async getChatMessages(userId, otherUserId, page = 1) {
        const response = await fetch(
            `/api/Chat/GetChatMessages/${userId}/${otherUserId}?page=${page}`
        );
        return await response.json();
    }
    
    static async markAsRead(userId, otherUserId) {
        const response = await fetch(`/api/Chat/MarkAsRead/${userId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ OtherUserId: otherUserId })
        });
        return await response.json();
    }
    
    static async getUnreadCount(userId) {
        const response = await fetch(`/api/Chat/GetUnreadCount/${userId}`);
        return await response.json();
    }
}
```

### **React Components:**
```jsx
import React, { useState, useEffect, useRef } from 'react';
import ChatService from './ChatService';

const ChatList = ({ userId, onChatSelect }) => {
    const [chats, setChats] = useState([]);
    const [loading, setLoading] = useState(true);
    
    useEffect(() => {
        loadChats();
    }, [userId]);
    
    const loadChats = async () => {
        try {
            const response = await ChatService.getChats(userId);
            if (response.Success) {
                setChats(response.Chats);
            }
        } catch (error) {
            console.error('Error loading chats:', error);
        } finally {
            setLoading(false);
        }
    };
    
    if (loading) return <div>Loading chats...</div>;
    
    return (
        <div className="chat-list">
            {chats.map(chat => (
                <div 
                    key={chat.UserId} 
                    className="chat-item"
                    onClick={() => onChatSelect(chat)}
                >
                    <div className="chat-info">
                        <h4>{chat.UserName}</h4>
                        <p className="last-message">{chat.LastMessage}</p>
                    </div>
                    <div className="chat-meta">
                        <span className="time">{formatTime(chat.LastMessageTime)}</span>
                        {chat.UnreadCount > 0 && (
                            <span className="unread-badge">{chat.UnreadCount}</span>
                        )}
                    </div>
                </div>
            ))}
        </div>
    );
};

const ChatWindow = ({ currentUserId, otherUserId, otherUserName }) => {
    const [messages, setMessages] = useState([]);
    const [newMessage, setNewMessage] = useState('');
    const [loading, setLoading] = useState(true);
    const [sending, setSending] = useState(false);
    const messagesEndRef = useRef(null);
    
    useEffect(() => {
        if (otherUserId) {
            loadMessages();
            markAsRead();
        }
    }, [currentUserId, otherUserId]);
    
    useEffect(() => {
        scrollToBottom();
    }, [messages]);
    
    const loadMessages = async () => {
        try {
            const response = await ChatService.getChatMessages(currentUserId, otherUserId);
            if (response.Success) {
                setMessages(response.Messages);
            }
        } catch (error) {
            console.error('Error loading messages:', error);
        } finally {
            setLoading(false);
        }
    };
    
    const markAsRead = async () => {
        try {
            await ChatService.markAsRead(currentUserId, otherUserId);
        } catch (error) {
            console.error('Error marking as read:', error);
        }
    };
    
    const sendMessage = async (e) => {
        e.preventDefault();
        if (!newMessage.trim() || sending) return;
        
        setSending(true);
        try {
            const response = await ChatService.sendMessage(
                currentUserId, 
                otherUserId, 
                newMessage.trim()
            );
            
            if (response.Success && response.ChatMessage) {
                setMessages(prev => [...prev, response.ChatMessage]);
                setNewMessage('');
            }
        } catch (error) {
            console.error('Error sending message:', error);
        } finally {
            setSending(false);
        }
    };
    
    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };
    
    if (loading) return <div>Loading messages...</div>;
    
    return (
        <div className="chat-window">
            <div className="chat-header">
                <h3>{otherUserName}</h3>
            </div>
            
            <div className="messages-container">
                {messages.map(message => (
                    <div 
                        key={message.Id} 
                        className={`message ${message.IsSentByMe ? 'sent' : 'received'}`}
                    >
                        <div className="message-content">
                            {!message.IsSentByMe && (
                                <div className="sender-name">{message.SenderName}</div>
                            )}
                            <div className="message-text">{message.Message}</div>
                            <div className="message-time">
                                {formatTime(message.SentAt)}
                                {message.IsSentByMe && (
                                    <span className={`read-status ${message.IsRead ? 'read' : 'sent'}`}>
                                        {message.IsRead ? '✓✓' : '✓'}
                                    </span>
                                )}
                            </div>
                        </div>
                    </div>
                ))}
                <div ref={messagesEndRef} />
            </div>
            
            <form onSubmit={sendMessage} className="message-input">
                <input
                    type="text"
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                    placeholder="Type a message..."
                    disabled={sending}
                />
                <button type="submit" disabled={sending || !newMessage.trim()}>
                    {sending ? 'Sending...' : 'Send'}
                </button>
            </form>
        </div>
    );
};

const formatTime = (timestamp) => {
    const date = new Date(timestamp);
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();
    
    if (isToday) {
        return date.toLocaleTimeString('en-US', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });
    } else {
        return date.toLocaleDateString('en-US', { 
            month: 'short', 
            day: 'numeric' 
        });
    }
};
```

---

## **🔧 Usage Flow**

### **1. Typical Chat Flow:**

**For Android/React Frontend:**

1. **Load Chat List:**
   ```
   GET /api/Chat/GetChats/{userId}
   ```
   - Shows all conversations with unread counts
   - Sort by most recent message

2. **Open Specific Chat:**
   ```
   GET /api/Chat/GetChatMessages/{userId}/{otherUserId}
   PUT /api/Chat/MarkAsRead/{userId} (automatically)
   ```
   - Load message history
   - Mark messages as read when chat opens

3. **Send New Message:**
   ```
   POST /api/Chat/SendMessage/{senderId}
   ```
   - Add message to local UI immediately
   - Handle offline/retry if needed

4. **Refresh/Polling:**
   - Call GetChats periodically to check for new messages
   - Use GetUnreadCount for app badge notifications

### **2. Key Implementation Notes:**

- **Auto Mark as Read:** Call MarkAsRead API when user opens a chat
- **Pagination:** Load older messages using page parameter
- **Real-time:** Consider adding SignalR for instant message delivery
- **Offline Support:** Cache messages locally, sync when online
- **Push Notifications:** Integrate with FCM/APNS for new message alerts

---

**Chat System Complete! ✅**  
Ready for seamless user-to-user messaging with read status tracking.