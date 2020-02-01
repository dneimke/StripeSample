using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StripeSample.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "subs");

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: true),
                    CustomerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingInterval",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingInterval", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardType",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    Email = table.Column<string>(maxLength: 200, nullable: false),
                    SessionId = table.Column<string>(maxLength: 200, nullable: false),
                    CartState = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ExternalKey = table.Column<string>(maxLength: 200, nullable: false),
                    IdentityKey = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceStatus",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ExternalKey = table.Column<string>(maxLength: 200, nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStatus",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 1),
                    Name = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plan",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ExternalKey = table.Column<string>(maxLength: 200, nullable: false),
                    Name = table.Column<string>(nullable: false),
                    AmountInCents = table.Column<int>(nullable: false),
                    CurrencyId = table.Column<int>(nullable: false),
                    IntervalId = table.Column<int>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plan_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "subs",
                        principalTable: "Currency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plan_BillingInterval_IntervalId",
                        column: x => x.IntervalId,
                        principalSchema: "subs",
                        principalTable: "BillingInterval",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plan_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "subs",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ExternalKey = table.Column<string>(maxLength: 200, nullable: false),
                    PlanId = table.Column<Guid>(nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(nullable: true),
                    StatusId = table.Column<int>(nullable: false),
                    CancelAtPeriodEnd = table.Column<bool>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscription_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "subs",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscription_Plan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "subs",
                        principalTable: "Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscription_SubscriptionStatus_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "subs",
                        principalTable: "SubscriptionStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                schema: "subs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    LastModifiedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    ExternalKey = table.Column<string>(maxLength: 200, nullable: false),
                    SubscriptionId = table.Column<Guid>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    InvoiceNumber = table.Column<string>(maxLength: 200, nullable: false),
                    CurrencyCode = table.Column<string>(nullable: true),
                    AmountDueInCents = table.Column<int>(nullable: false),
                    AmountPaidInCents = table.Column<int>(nullable: false),
                    AmountRemainingInCents = table.Column<int>(nullable: false),
                    PeriodStart = table.Column<DateTime>(nullable: true),
                    PeriodEnd = table.Column<DateTime>(nullable: true),
                    HostedInvoiceUrl = table.Column<string>(nullable: true),
                    InvoicePdfUrl = table.Column<string>(nullable: true),
                    IsPaid = table.Column<bool>(nullable: false, defaultValue: false),
                    ReceiptNumber = table.Column<string>(nullable: true),
                    Total = table.Column<int>(nullable: false),
                    StatusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "subs",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoice_InvoiceStatus_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "subs",
                        principalTable: "InvoiceStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoice_Subscription_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "subs",
                        principalTable: "Subscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cart_SessionId",
                schema: "subs",
                table: "Cart",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_ExternalKey",
                schema: "subs",
                table: "Customer",
                column: "ExternalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_IdentityKey",
                schema: "subs",
                table: "Customer",
                column: "IdentityKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CustomerId",
                schema: "subs",
                table: "Invoice",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_ExternalKey",
                schema: "subs",
                table: "Invoice",
                column: "ExternalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_StatusId",
                schema: "subs",
                table: "Invoice",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_SubscriptionId",
                schema: "subs",
                table: "Invoice",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_CurrencyId",
                schema: "subs",
                table: "Plan",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_ExternalKey",
                schema: "subs",
                table: "Plan",
                column: "ExternalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plan_IntervalId",
                schema: "subs",
                table: "Plan",
                column: "IntervalId");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_Name",
                schema: "subs",
                table: "Plan",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plan_ProductId",
                schema: "subs",
                table: "Plan",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ExternalKey",
                schema: "subs",
                table: "Product",
                column: "ExternalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                schema: "subs",
                table: "Product",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_CustomerId",
                schema: "subs",
                table: "Subscription",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_ExternalKey",
                schema: "subs",
                table: "Subscription",
                column: "ExternalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_PlanId",
                schema: "subs",
                table: "Subscription",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_StatusId",
                schema: "subs",
                table: "Subscription",
                column: "StatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUser",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "CardType",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Cart",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Invoice",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "InvoiceStatus",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Subscription",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Plan",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "SubscriptionStatus",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Currency",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "BillingInterval",
                schema: "subs");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "subs");
        }
    }
}
