using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertTickets_API.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentApplied",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentApplied",
                table: "Reservations");
        }
    }
}
