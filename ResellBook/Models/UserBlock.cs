using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResellBook.Helpers;

namespace ResellBook.Models
{
    public class UserBlock
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BlockerId { get; set; } // User who initiated the block

        [Required]
        public Guid BlockedUserId { get; set; } // User who got blocked

        [Required]
        public DateTime BlockedAt { get; set; } = IndianTimeHelper.UtcNow;

        public string? Reason { get; set; } // Optional reason for blocking

        // Navigation properties
        [ForeignKey("BlockerId")]
        public virtual User Blocker { get; set; } = null!;

        [ForeignKey("BlockedUserId")]
        public virtual User BlockedUser { get; set; } = null!;
    }

    // DTO for blocking user
    public class BlockUserDto
    {
        [Required]
        public Guid UserIdToBlock { get; set; }

        public string? Reason { get; set; }
    }

    // Response DTO for blocking operations
    public class BlockUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? BlockedUserName { get; set; }
        public DateTime? BlockedAt { get; set; }
    }

    // Response DTO for unblocking operations
    public class UnblockUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UnblockedUserName { get; set; }
    }

    // DTO for blocked users list
    public class BlockedUserDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public DateTime BlockedAt { get; set; }
        public string? Reason { get; set; }
    }

    // Response DTO for getting blocked users
    public class BlockedUsersResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BlockedUserDto> BlockedUsers { get; set; } = new();
        public int TotalCount { get; set; }
    }
}