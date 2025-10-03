using System.ComponentModel.DataAnnotations;

namespace ResellBook.Models
{
    public class Book
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [Required] public Guid UserId { get; set; }  // FK to User
        [Required] public required string BookName { get; set; }
        public string? AuthorOrPublication { get; set; }
        [Required] public required string Category { get; set; }
        public string? SubCategory { get; set; }
        [Required] public decimal SellingPrice { get; set; }
        public bool IsSold { get; set; } = false;
        public string? ImagePathsJson { get; set; } // Store multiple image paths as JSON
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
    }
}
