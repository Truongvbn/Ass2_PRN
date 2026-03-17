using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExtraChargeAmount",
                table: "Bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExtraChargeDescription",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtraChargePaid",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LostAndFoundImageUrl",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LostAndFoundNotes",
                table: "Bookings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraChargeAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ExtraChargeDescription",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsExtraChargePaid",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LostAndFoundImageUrl",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LostAndFoundNotes",
                table: "Bookings");
        }
    }
}
