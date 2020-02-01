using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    class InvoiceEntityTypeConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable(nameof(Invoice), SubscriptionsContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("ExternalKey")
                .IsUnique(true);

            builder.Property<Guid>("CustomerId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.Property<Guid>("SubscriptionId")
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .IsRequired();

            builder.Property(x => x.InvoiceNumber)
                .HasMaxLength(200)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.Property(x => x.IsPaid)
               .HasDefaultValue(false)
               .IsRequired();

            builder.HasOne<InvoiceStatus>(nameof(Invoice.Status))
               .WithMany()
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
