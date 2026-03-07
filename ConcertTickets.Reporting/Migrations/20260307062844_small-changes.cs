using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertTickets.Reporting.Migrations
{
    /// <inheritdoc />
    public partial class smallchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "ReservationEventLogs");

            migrationBuilder.AddColumn<string>(
                name: "ReservationCode",
                table: "ReservationEventLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationCode",
                table: "ReservationEventLogs");

            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "ReservationEventLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
