using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestRoomAllocation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStateAndZipCodeFromAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "Apartments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                table: "Apartments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "Apartments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
