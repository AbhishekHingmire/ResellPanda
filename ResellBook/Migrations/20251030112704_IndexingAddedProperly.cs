using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResellBook.Migrations
{
    /// <inheritdoc />
    public partial class IndexingAddedProperly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BookName",
                table: "Books",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocations_LatLon",
                table: "UserLocations",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookName",
                table: "Books",
                column: "BookName");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_LatLon",
                table: "Banners",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLocations_LatLon",
                table: "UserLocations");

            migrationBuilder.DropIndex(
                name: "IX_Books_BookName",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Banners_LatLon",
                table: "Banners");

            migrationBuilder.AlterColumn<string>(
                name: "BookName",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
