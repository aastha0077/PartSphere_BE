using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToVehiclePart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "VehicleParts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "VehicleParts");
        }
    }
}
