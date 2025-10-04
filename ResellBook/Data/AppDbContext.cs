
using Microsoft.EntityFrameworkCore;
using ResellBook.Models;

namespace ResellBook.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }

}
