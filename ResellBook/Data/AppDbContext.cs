
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
        public DbSet<UserChat> UserChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for Book.SellingPrice
            modelBuilder.Entity<Book>()
                .Property(b => b.SellingPrice)
                .HasPrecision(18, 2); // 18 total digits, 2 decimal places

            // Configure UserChat relationships
            modelBuilder.Entity<UserChat>(entity =>
            {
                entity.HasKey(uc => uc.Id);

                entity.Property(uc => uc.Message)
                      .IsRequired()
                      .HasMaxLength(5000);

                entity.Property(uc => uc.SentAt)
                      .IsRequired()
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(uc => uc.IsRead)
                      .HasDefaultValue(false);

                // Configure foreign key relationships
                entity.HasOne(uc => uc.Sender)
                      .WithMany()
                      .HasForeignKey(uc => uc.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(uc => uc.Receiver)
                      .WithMany()
                      .HasForeignKey(uc => uc.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for better performance
                entity.HasIndex(uc => new { uc.SenderId, uc.ReceiverId })
                      .HasDatabaseName("IX_UserChats_SenderReceiver");

                entity.HasIndex(uc => uc.SentAt)
                      .HasDatabaseName("IX_UserChats_SentAt");

                entity.HasIndex(uc => new { uc.ReceiverId, uc.IsRead })
                      .HasDatabaseName("IX_UserChats_ReceiverUnread");
            });
        }
    }

}
