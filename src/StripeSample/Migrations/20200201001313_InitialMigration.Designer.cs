﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StripeSample.Infrastructure.Data;

namespace StripeSample.Migrations
{
    [DbContext(typeof(SubscriptionsContext))]
    [Migration("20200201001313_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("subs")
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("StripeSample.Domain.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomerId");

                    b.Property<string>("EmailAddress");

                    b.HasKey("Id");

                    b.ToTable("ApplicationUser");
                });

            modelBuilder.Entity("StripeSample.Domain.BillingInterval", b =>
                {
                    b.Property<int>("Id")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("BillingInterval","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.CardType", b =>
                {
                    b.Property<int>("Id")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("CardType","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Cart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CartState");

                    b.Property<DateTimeOffset>("CreatedDateTime");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("LastModifiedDateTime");

                    b.Property<string>("SessionId")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.HasIndex("SessionId")
                        .IsUnique();

                    b.ToTable("Cart","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Currency", b =>
                {
                    b.Property<int>("Id")
                        .HasDefaultValue(1);

                    b.Property<string>("Language");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("Currency","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedDateTime");

                    b.Property<string>("ExternalKey")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("IdentityKey")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("LastModifiedDateTime");

                    b.HasKey("Id");

                    b.HasIndex("ExternalKey")
                        .IsUnique();

                    b.HasIndex("IdentityKey")
                        .IsUnique();

                    b.ToTable("Customer","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Invoice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AmountDueInCents");

                    b.Property<int>("AmountPaidInCents");

                    b.Property<int>("AmountRemainingInCents");

                    b.Property<DateTimeOffset>("CreatedDateTime");

                    b.Property<string>("CurrencyCode");

                    b.Property<Guid>("CustomerId");

                    b.Property<string>("ExternalKey")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("HostedInvoiceUrl");

                    b.Property<string>("InvoiceNumber")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("InvoicePdfUrl");

                    b.Property<bool>("IsPaid")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<DateTimeOffset>("LastModifiedDateTime");

                    b.Property<DateTime?>("PeriodEnd");

                    b.Property<DateTime?>("PeriodStart");

                    b.Property<string>("ReceiptNumber");

                    b.Property<int>("StatusId");

                    b.Property<Guid>("SubscriptionId");

                    b.Property<int>("Total");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ExternalKey")
                        .IsUnique();

                    b.HasIndex("StatusId");

                    b.HasIndex("SubscriptionId");

                    b.ToTable("Invoice","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.InvoiceStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("InvoiceStatus","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Plan", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AmountInCents");

                    b.Property<DateTimeOffset>("CreatedDateTime");

                    b.Property<int>("CurrencyId");

                    b.Property<string>("ExternalKey")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<int>("IntervalId");

                    b.Property<DateTimeOffset>("LastModifiedDateTime");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid>("ProductId");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("ExternalKey")
                        .IsUnique();

                    b.HasIndex("IntervalId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("ProductId");

                    b.ToTable("Plan","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedDateTime");

                    b.Property<string>("ExternalKey")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("LastModifiedDateTime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.HasIndex("ExternalKey")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Product","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Subscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CancelAtPeriodEnd");

                    b.Property<DateTimeOffset>("CreatedDateTime");

                    b.Property<DateTime?>("CurrentPeriodEnd");

                    b.Property<DateTime?>("CurrentPeriodStart");

                    b.Property<Guid>("CustomerId");

                    b.Property<string>("ExternalKey")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("LastModifiedDateTime");

                    b.Property<Guid>("PlanId");

                    b.Property<int>("StatusId");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ExternalKey")
                        .IsUnique();

                    b.HasIndex("PlanId");

                    b.HasIndex("StatusId");

                    b.ToTable("Subscription","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.SubscriptionStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.ToTable("SubscriptionStatus","subs");
                });

            modelBuilder.Entity("StripeSample.Domain.Invoice", b =>
                {
                    b.HasOne("StripeSample.Domain.Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("StripeSample.Domain.InvoiceStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("StripeSample.Domain.Subscription", "Subscription")
                        .WithMany("Invoices")
                        .HasForeignKey("SubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("StripeSample.Domain.Plan", b =>
                {
                    b.HasOne("StripeSample.Domain.Currency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("StripeSample.Domain.BillingInterval", "Interval")
                        .WithMany()
                        .HasForeignKey("IntervalId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("StripeSample.Domain.Product")
                        .WithMany("Plans")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("StripeSample.Domain.Subscription", b =>
                {
                    b.HasOne("StripeSample.Domain.Customer", "Customer")
                        .WithMany("Subscriptions")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("StripeSample.Domain.Plan", "Plan")
                        .WithMany()
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("StripeSample.Domain.SubscriptionStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
