using Microsoft.EntityFrameworkCore;
using StripeSample.Entities;
using System.Linq;

namespace StripeSample.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<StripeJob> StripeJob { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<User>()
                .HasIndex(b => b.EmailAddress)
                .IsUnique();

            modelBuilder.Entity<Cart>()
                .HasIndex(b => b.SessionId)
                .IsUnique();

            modelBuilder.Entity<Subscription>()
                .HasIndex(b => b.SubscriptionId)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(b => b.InvoiceId)
                .IsUnique();


            base.OnModelCreating(modelBuilder);
        }
    }
}
