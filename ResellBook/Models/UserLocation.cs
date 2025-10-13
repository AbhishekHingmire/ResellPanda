
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResellBook.Helpers;

namespace ResellBook.Models
{
    public class UserLocation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public DateTime CreateDate { get; set; } = IndianTimeHelper.UtcNow;

        public virtual User? User { get; set; } = null!;  // Navigation property
    }
}
