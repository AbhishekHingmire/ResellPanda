using System.ComponentModel.DataAnnotations;

namespace ResellBook.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string EmailVerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
    }
}
