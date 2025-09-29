
using Microsoft.EntityFrameworkCore;
using ResellBook.Models;

namespace ResellBook.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }

}
