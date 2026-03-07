using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertTickets.Reporting.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationIdToReservationEventLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "ReservationEventLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ReservationEventLogs");
        }
    }
}
