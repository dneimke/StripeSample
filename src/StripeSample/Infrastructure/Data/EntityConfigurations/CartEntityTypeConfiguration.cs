using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    class CartEntityTypeConfiguration
        : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> config)
        {
            config.ToTable(nameof(Cart), SubscriptionsContext.DEFAULT_SCHEMA);

            config.HasKey(ct => ct.Id);

            config.Property(ct => ct.Id)
                .IsRequired();

            config.Property(ct => ct.Email)
                .HasMaxLength(200)
                .IsRequired();

            config.Property(ct => ct.SessionId)
                .HasMaxLength(200)
                .IsRequired();

            config.Property(ct => ct.CartState)
                .IsRequired();

            config.HasIndex(b => b.SessionId)
               .IsUnique();
        }
    }
}
