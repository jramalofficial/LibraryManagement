using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAvailableToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Books");
        }
    }
}
