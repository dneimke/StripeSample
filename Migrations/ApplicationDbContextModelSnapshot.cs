﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StripeSample.Infrastructure.Data;

namespace StripeSample.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("StripeSample.Entities.Cart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CartState");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<DateTime>("ModifiedDateTime");

                    b.Property<string>("SessionId");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("SessionId")
                        .IsUnique()
                        .HasFilter("[SessionId] IS NOT NULL");

                    b.HasIndex("UserId");

                    b.ToTable("Cart");
                });

            modelBuilder.Entity("StripeSample.Entities.Invoice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AmountDue");

                    b.Property<long>("AmountPaid");

                    b.Property<long>("AmountRemaining");

                    b.Property<string>("BillingReason");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("InvoiceId")
                        .IsRequired();

                    b.Property<string>("InvoiceNumber")
                        .IsRequired();

                    b.Property<string>("InvoicePdfUrl");

                    b.Property<DateTime>("ModifiedDateTime");

                    b.Property<DateTime>("PeriodEnd");

                    b.Property<DateTime>("PeriodStart");

                    b.Property<int>("Status");

                    b.Property<Guid>("SubscriptionId");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId")
                        .IsUnique();

                    b.HasIndex("SubscriptionId");

                    b.ToTable("Invoice");
                });

            modelBuilder.Entity("StripeSample.Entities.Subscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<DateTime>("ModifiedDateTime");

                    b.Property<string>("PlanId")
                        .IsRequired();

                    b.Property<int>("State");

                    b.Property<string>("SubscriptionId")
                        .IsRequired();

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("SubscriptionId")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Subscription");
                });

            modelBuilder.Entity("StripeSample.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomerId");

                    b.Property<string>("EmailAddress")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("EmailAddress")
                        .IsUnique();

                    b.ToTable("User");
                });

            modelBuilder.Entity("StripeSample.Entities.Cart", b =>
                {
                    b.HasOne("StripeSample.Entities.User", "User")
                        .WithMany("Carts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("StripeSample.Entities.Invoice", b =>
                {
                    b.HasOne("StripeSample.Entities.Subscription", "Subscription")
                        .WithMany("Invoices")
                        .HasForeignKey("SubscriptionId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("StripeSample.Entities.Subscription", b =>
                {
                    b.HasOne("StripeSample.Entities.User", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
