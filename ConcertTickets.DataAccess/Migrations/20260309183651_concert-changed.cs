using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertTickets_API.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class concertchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPublished",
                table: "Concerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isPublished",
                table: "Concerts");
        }
    }
}
