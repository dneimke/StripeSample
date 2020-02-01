using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    public class PlanEntityTypeConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.ToTable(nameof(Plan), SubscriptionsContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("ExternalKey")
              .IsUnique(true);

            builder.Property(ct => ct.Name)
                .IsRequired();

            builder.HasIndex("Name")
              .IsUnique(true);

            builder.Property<Guid>("ProductId")
               .IsRequired();

            builder.Property(ct => ct.AmountInCents)
                .IsRequired();

            builder.HasOne<BillingInterval>(nameof(Plan.Interval))
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Currency>(nameof(Plan.Currency))
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
