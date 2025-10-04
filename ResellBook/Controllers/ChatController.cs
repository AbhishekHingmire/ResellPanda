using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResellBook.Data;
using ResellBook.Models;
using ResellBook.Helpers;

namespace ResellBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ChatController> _logger;

        public ChatController(AppDbContext context, ILogger<ChatController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Send a message to another user
        /// </summary>
        /// <param name="senderId">ID of the user sending the message</param>
        /// <param name="dto">Message details</param>
        /// <returns>Message sent confirmation</returns>
        [HttpPost("SendMessage/{senderId}")]
        public async Task<ActionResult<SendMessageResponse>> SendMessage(Guid senderId, [FromBody] SendMessageDto dto)
        {
            try
            {
                _logger.LogInformation($"SendMessage called - Sender: {senderId}, Receiver: {dto.ReceiverId}");

                // Validate sender exists
                var sender = await _context.Users.FindAsync(senderId);
                if (sender == null)
                {
                    return NotFound(new SendMessageResponse 
                    { 
                        Success = false, 
                        Message = "Sender not found" 
                    });
                }

                // Validate receiver exists
                var receiver = await _context.Users.FindAsync(dto.ReceiverId);
                if (receiver == null)
                {
                    return NotFound(new SendMessageResponse 
                    { 
                        Success = false, 
                        Message = "Receiver not found" 
                    });
                }

                // Create new chat message
                var chatMessage = new UserChat
                {
                    SenderId = senderId,
                    ReceiverId = dto.ReceiverId,
                    Message = dto.Message.Trim(),
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.UserChats.Add(chatMessage);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Message sent successfully - ID: {chatMessage.Id}");

                // Return the created message
                var response = new SendMessageResponse
                {
                    Success = true,
                    Message = "Message sent successfully",
                    ChatMessage = new ChatMessageDto
                    {
                        Id = chatMessage.Id,
                        SenderId = chatMessage.SenderId,
                        ReceiverId = chatMessage.ReceiverId,
                        SenderName = sender.Name,
                        Message = chatMessage.Message,
                        SentAt = chatMessage.SentAt,
                        IsRead = chatMessage.IsRead,
                        IsSentByMe = true
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new SendMessageResponse 
                { 
                    Success = false, 
                    Message = "Failed to send message" 
                });
            }
        }

        /// <summary>
        /// Get all chats for a user (chat list with last messages)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of chats with unread counts</returns>
        [HttpGet("GetChats/{userId}")]
        public async Task<ActionResult<ChatListResponse>> GetChats(Guid userId)
        {
            try
            {
                _logger.LogInformation($"GetChats called for user: {userId}");

                // Validate user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new ChatListResponse 
                    { 
                        Success = false, 
                        Message = "User not found" 
                    });
                }

                // Get all users that have chatted with this user
                var chatPartners = await _context.UserChats
                    .Where(c => c.SenderId == userId || c.ReceiverId == userId)
                    .Select(c => c.SenderId == userId ? c.ReceiverId : c.SenderId)
                    .Distinct()
                    .ToListAsync();

                var chatList = new List<ChatListDto>();

                foreach (var partnerId in chatPartners)
                {
                    var partner = await _context.Users.FindAsync(partnerId);
                    if (partner == null) continue;

                    // Get last message between these users
                    var lastMessage = await _context.UserChats
                        .Where(c => (c.SenderId == userId && c.ReceiverId == partnerId) ||
                                   (c.SenderId == partnerId && c.ReceiverId == userId))
                        .OrderByDescending(c => c.SentAt)
                        .FirstOrDefaultAsync();

                    // Count unread messages from this partner
                    var unreadCount = await _context.UserChats
                        .CountAsync(c => c.SenderId == partnerId && 
                                        c.ReceiverId == userId && 
                                        !c.IsRead);

                    chatList.Add(new ChatListDto
                    {
                        UserId = partnerId,
                        UserName = partner.Name,
                        LastMessage = lastMessage?.Message,
                        LastMessageTime = lastMessage?.SentAt,
                        UnreadCount = unreadCount,
                        IsOnline = false // You can implement online status later
                    });
                }

                // Sort by last message time (most recent first)
                chatList = chatList.OrderByDescending(c => c.LastMessageTime ?? DateTime.MinValue).ToList();

                _logger.LogInformation($"Retrieved {chatList.Count} chats for user {userId}");

                return Ok(new ChatListResponse
                {
                    Success = true,
                    Message = "Chats retrieved successfully",
                    Chats = chatList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chats");
                return StatusCode(500, new ChatListResponse 
                { 
                    Success = false, 
                    Message = "Failed to retrieve chats" 
                });
            }
        }

        /// <summary>
        /// Get chat messages between two users
        /// </summary>
        /// <param name="userId">Current user ID</param>
        /// <param name="otherUserId">Other user ID</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Messages per page (default 50)</param>
        /// <returns>Chat messages between the users</returns>
        [HttpGet("GetChatMessages/{userId}/{otherUserId}")]
        public async Task<ActionResult<ChatResponse>> GetChatMessages(
            Guid userId, 
            Guid otherUserId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50)
        {
            try
            {
                _logger.LogInformation($"GetChatMessages called - User: {userId}, Other: {otherUserId}");

                // Validate users exist
                var user = await _context.Users.FindAsync(userId);
                var otherUser = await _context.Users.FindAsync(otherUserId);

                if (user == null || otherUser == null)
                {
                    return NotFound(new ChatResponse 
                    { 
                        Success = false, 
                        Message = "One or both users not found" 
                    });
                }

                // Get messages between these users with pagination
                var messages = await _context.UserChats
                    .Include(c => c.Sender)
                    .Where(c => (c.SenderId == userId && c.ReceiverId == otherUserId) ||
                               (c.SenderId == otherUserId && c.ReceiverId == userId))
                    .OrderByDescending(c => c.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ChatMessageDto
                    {
                        Id = c.Id,
                        SenderId = c.SenderId,
                        ReceiverId = c.ReceiverId,
                        SenderName = c.Sender.Name,
                        Message = c.Message,
                        SentAt = c.SentAt,
                        IsRead = c.IsRead,
                        ReadAt = c.ReadAt,
                        IsSentByMe = c.SenderId == userId
                    })
                    .ToListAsync();

                // Reverse to show oldest first
                messages.Reverse();

                _logger.LogInformation($"Retrieved {messages.Count} messages between users");

                return Ok(new ChatResponse
                {
                    Success = true,
                    Message = "Messages retrieved successfully",
                    Messages = messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat messages");
                return StatusCode(500, new ChatResponse 
                { 
                    Success = false, 
                    Message = "Failed to retrieve messages" 
                });
            }
        }

        /// <summary>
        /// Mark all messages from another user as read
        /// </summary>
        /// <param name="userId">Current user ID</param>
        /// <param name="dto">Other user details</param>
        /// <returns>Success confirmation</returns>
        [HttpPut("MarkAsRead/{userId}")]
        public async Task<ActionResult> MarkAsRead(Guid userId, [FromBody] MarkAsReadDto dto)
        {
            try
            {
                _logger.LogInformation($"MarkAsRead called - User: {userId}, Other: {dto.OtherUserId}");

                // Find all unread messages from otherUser to currentUser
                var unreadMessages = await _context.UserChats
                    .Where(c => c.SenderId == dto.OtherUserId && 
                               c.ReceiverId == userId && 
                               !c.IsRead)
                    .ToListAsync();

                if (unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        message.IsRead = true;
                        message.ReadAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Marked {unreadMessages.Count} messages as read");
                }

                return Ok(new { 
                    Success = true, 
                    Message = $"Marked {unreadMessages.Count} messages as read",
                    UpdatedCount = unreadMessages.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Failed to mark messages as read" 
                });
            }
        }

        /// <summary>
        /// Get unread message count for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Total unread message count</returns>
        [HttpGet("GetUnreadCount/{userId}")]
        public async Task<ActionResult> GetUnreadCount(Guid userId)
        {
            try
            {
                var unreadCount = await _context.UserChats
                    .CountAsync(c => c.ReceiverId == userId && !c.IsRead);

                return Ok(new { 
                    Success = true, 
                    UnreadCount = unreadCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Failed to get unread count" 
                });
            }
        }

        /// <summary>
        /// Delete a specific message (only sender can delete)
        /// </summary>
        /// <param name="messageId">Message ID</param>
        /// <param name="userId">User ID requesting deletion</param>
        /// <returns>Deletion confirmation</returns>
        [HttpDelete("DeleteMessage/{messageId}/{userId}")]
        public async Task<ActionResult> DeleteMessage(Guid messageId, Guid userId)
        {
            try
            {
                var message = await _context.UserChats.FindAsync(messageId);
                
                if (message == null)
                {
                    return NotFound(new { Success = false, Message = "Message not found" });
                }

                if (message.SenderId != userId)
                {
                    return Forbid("You can only delete your own messages");
                }

                _context.UserChats.Remove(message);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Message deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                return StatusCode(500, new { Success = false, Message = "Failed to delete message" });
            }
        }

        /// <summary>
        /// Delete entire chat permanently between two users
        /// </summary>
        /// <param name="userId">Current user ID</param>
        /// <param name="otherUserId">Other user ID</param>
        /// <returns>Deletion confirmation</returns>
        [HttpDelete("DeleteChat/{userId}/{otherUserId}")]
        public async Task<ActionResult<DeleteChatResponse>> DeleteChat(Guid userId, Guid otherUserId)
        {
            try
            {
                _logger.LogInformation($"DeleteChat called - User: {userId}, Other: {otherUserId}");

                // Validate both users exist
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new DeleteChatResponse { Success = false, Message = "User not found" });
                }

                var otherUser = await _context.Users.FindAsync(otherUserId);
                if (otherUser == null)
                {
                    return NotFound(new DeleteChatResponse { Success = false, Message = "Other user not found" });
                }

                // Find all messages between these two users
                var messagesToDelete = await _context.UserChats
                    .Where(c => (c.SenderId == userId && c.ReceiverId == otherUserId) ||
                               (c.SenderId == otherUserId && c.ReceiverId == userId))
                    .ToListAsync();

                if (messagesToDelete.Count == 0)
                {
                    return NotFound(new DeleteChatResponse { Success = false, Message = "No chat found between these users" });
                }

                // Delete all messages
                _context.UserChats.RemoveRange(messagesToDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Chat deleted successfully - {messagesToDelete.Count} messages removed between users {userId} and {otherUserId}");

                return Ok(new DeleteChatResponse
                { 
                    Success = true, 
                    Message = "Chat deleted permanently", 
                    DeletedMessagesCount = messagesToDelete.Count,
                    DeletedBetween = new ChatParticipants
                    {
                        User1Name = user.Name,
                        User2Name = otherUser.Name
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting chat between users {userId} and {otherUserId}");
                return StatusCode(500, new DeleteChatResponse { Success = false, Message = "Failed to delete chat" });
            }
        }
    }
}