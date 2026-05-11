using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartSphere.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVendorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "VehicleParts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VehicleParts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VehicleParts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LifespanKm",
                table: "VehicleParts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "VehicleParts",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "VehicleParts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MileageAtSale",
                table: "SalesInvoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "SalesInvoices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoints",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_VehicleId",
                table: "SalesInvoices",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Vehicles_VehicleId",
                table: "SalesInvoices",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Vehicles_VehicleId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_VehicleId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "LifespanKm",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "MileageAtSale",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LoyaltyPoints",
                table: "Customers");
        }
    }
}
