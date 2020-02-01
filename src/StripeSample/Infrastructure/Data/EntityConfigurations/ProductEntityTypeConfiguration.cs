using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable(nameof(Product), SubscriptionsContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("ExternalKey")
              .IsUnique(true);

            builder.Property(ct => ct.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("Name")
              .IsUnique(true);

            var navigation = builder.Metadata.FindNavigation(nameof(Product.Plans));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
