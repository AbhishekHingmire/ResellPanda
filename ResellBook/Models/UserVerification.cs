using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResellBook.Models
{
    public class UserVerification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public VerificationType Type { get; set; }  // EmailVerification or PasswordReset

        [Required]
        public DateTime Expiry { get; set; }

        public bool IsUsed { get; set; } = false;
    }

    public enum VerificationType
    {
        EmailVerification,
        PasswordReset
    }
}
