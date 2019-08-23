using Microsoft.EntityFrameworkCore.Migrations;

namespace StripeSample.Migrations
{
    public partial class UniqueIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Subscription_SubscriptionId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_User_UserId",
                table: "Subscription");

            migrationBuilder.AlterColumn<string>(
                name: "EmailAddress",
                table: "User",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SubscriptionId",
                table: "Subscription",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceId",
                table: "Invoice",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_User_EmailAddress",
                table: "User",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_SubscriptionId",
                table: "Subscription",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_InvoiceId",
                table: "Invoice",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Subscription_SubscriptionId",
                table: "Invoice",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_User_UserId",
                table: "Subscription",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Subscription_SubscriptionId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_User_UserId",
                table: "Subscription");

            migrationBuilder.DropIndex(
                name: "IX_User_EmailAddress",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Subscription_SubscriptionId",
                table: "Subscription");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_InvoiceId",
                table: "Invoice");

            migrationBuilder.AlterColumn<string>(
                name: "EmailAddress",
                table: "User",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SubscriptionId",
                table: "Subscription",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceId",
                table: "Invoice",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Subscription_SubscriptionId",
                table: "Invoice",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_User_UserId",
                table: "Subscription",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
