using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    class InvoiceStatusTypeEntityTypeConfiguration
        : IEntityTypeConfiguration<InvoiceStatus>
    {
        public void Configure(EntityTypeBuilder<InvoiceStatus> cardTypesConfiguration)
        {
            cardTypesConfiguration.ToTable(nameof(InvoiceStatus), SubscriptionsContext.DEFAULT_SCHEMA);

            cardTypesConfiguration.HasKey(ct => ct.Id);

            cardTypesConfiguration.Property(ct => ct.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            cardTypesConfiguration.Property(ct => ct.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
