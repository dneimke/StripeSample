using StripeSample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace StripeSample.Infrastructure.Data.EntityConfigurations
{
    class SubscriptionEntityTypeConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable(nameof(Subscription), SubscriptionsContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex("ExternalKey")
                .IsUnique(true);

            builder.Property<Guid>("CustomerId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.Property<Guid>("PlanId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.HasOne<Plan>(nameof(Subscription.Plan))
               .WithMany()
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<SubscriptionStatus>(nameof(Subscription.Status))
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            var navigation = builder.Metadata.FindNavigation(nameof(Subscription.Invoices));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
