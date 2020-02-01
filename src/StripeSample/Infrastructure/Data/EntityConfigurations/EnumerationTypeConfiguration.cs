using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StripeSample.Domain;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    class EnumerationTypeConfiguration<T> : IEntityTypeConfiguration<T> where T:Enumeration
    {
        public void Configure(EntityTypeBuilder<T> configuration)
        {
            configuration.ToTable(typeof(T).Name, SubscriptionsContext.DEFAULT_SCHEMA);

            configuration.HasKey(ct => ct.Id);

            configuration.Property(ct => ct.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            configuration.Property(ct => ct.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
