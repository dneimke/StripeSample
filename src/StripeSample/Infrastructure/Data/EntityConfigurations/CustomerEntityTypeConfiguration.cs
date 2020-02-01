using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable(nameof(Customer), SubscriptionsContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("ExternalKey")
              .IsUnique(true);

            builder.Property(x => x.IdentityKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("IdentityKey")
              .IsUnique(true);

            var navigation = builder.Metadata.FindNavigation(nameof(Customer.Subscriptions));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
