using Microsoft.EntityFrameworkCore;
using StripeSample.Domain;
using StripeSample.Infrastructure.Data.EntityConfigurations;

namespace StripeSample.Infrastructure.Data
{
    public class SubscriptionsContext : DbContext
    {
        public const string DEFAULT_SCHEMA = "subs";
        public const string MIGRATIONS_TABLE = "__SubscriptionsMigrationsHistory";

        public DbSet<ApplicationUser> ApplicationUser { get; set; } // This would live in a separate, application dbcontext

        public DbSet<Currency> Currency { get; set; }
        public DbSet<CardType> CardType { get; set; }
        public DbSet<BillingInterval> BillingInterval { get; set; }
        public DbSet<InvoiceStatus> InvoiceStatus { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatus { get; set; }

        public DbSet<Cart> Cart { get; set; }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<Plan> Plan { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Subscription> Subscription { get; set; }


        public SubscriptionsContext(DbContextOptions<SubscriptionsContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema(DEFAULT_SCHEMA);

            // TODO: https://github.com/ardalis/EFCore.Extensions/blob/master/src/Ardalis.EFCore.Extensions/ModelBuilderExtensions.cs
            // builder.ApplyConfigurationsFromAssembly()

            builder.ApplyConfiguration(new EnumerationTypeConfiguration<BillingInterval>());
            builder.ApplyConfiguration(new EnumerationTypeConfiguration<CardType>());
            builder.ApplyConfiguration(new EnumerationTypeConfiguration<Currency>());
            builder.ApplyConfiguration(new EnumerationTypeConfiguration<InvoiceStatus>());
            builder.ApplyConfiguration(new EnumerationTypeConfiguration<SubscriptionStatus>());

            builder.ApplyConfiguration(new CartEntityTypeConfiguration());

            builder.ApplyConfiguration(new CustomerEntityTypeConfiguration());
            builder.ApplyConfiguration(new InvoiceEntityTypeConfiguration());
            builder.ApplyConfiguration(new PlanEntityTypeConfiguration());
            builder.ApplyConfiguration(new ProductEntityTypeConfiguration());
            builder.ApplyConfiguration(new SubscriptionEntityTypeConfiguration());
        }
    }
}
