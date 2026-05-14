using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddLastCreditReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCreditReminderSentAt",
                table: "CreditPayments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCreditReminderSentAt",
                table: "CreditPayments");
        }
    }
}
