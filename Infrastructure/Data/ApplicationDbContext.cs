using Microsoft.EntityFrameworkCore;
using StripeSample.Entities;

namespace StripeSample.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
    }
}
