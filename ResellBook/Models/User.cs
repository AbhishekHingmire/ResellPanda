using System.ComponentModel.DataAnnotations;

namespace ResellBook.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public bool IsEmailVerified { get; set; } = false;
    }
}
