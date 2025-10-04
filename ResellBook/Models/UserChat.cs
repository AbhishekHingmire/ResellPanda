using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResellBook.Models
{
    public class UserChat
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        [StringLength(5000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; } = null!;
    }

    // DTO for sending messages
    public class SendMessageDto
    {
        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;
    }

    // DTO for chat list response
    public class ChatListDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; } = false;
    }

    // DTO for individual chat message
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsSentByMe { get; set; }
    }

    // DTO for marking messages as read
    public class MarkAsReadDto
    {
        [Required]
        public Guid OtherUserId { get; set; }
    }

    // Response DTOs
    public class SendMessageResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ChatMessageDto? ChatMessage { get; set; }
    }

    public class ChatResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class ChatListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ChatListDto> Chats { get; set; } = new();
    }

    public class DeleteChatResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedMessagesCount { get; set; }
        public ChatParticipants? DeletedBetween { get; set; }
    }

    public class ChatParticipants
    {
        public string User1Name { get; set; } = string.Empty;
        public string User2Name { get; set; } = string.Empty;
    }
}