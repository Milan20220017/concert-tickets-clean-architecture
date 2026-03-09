using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertTickets_API.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEarlylBirdDiscountUntilConcert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EarlyBirdDiscountUntil",
                table: "Concerts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarlyBirdDiscountUntil",
                table: "Concerts");
        }
    }
}
