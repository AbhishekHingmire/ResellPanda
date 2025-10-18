using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResellBook.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserLocations_UserId_CreateDate",
                table: "UserLocations",
                columns: new[] { "UserId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Books_CreatedAt",
                table: "Books",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsBoosted_ListingLastDate",
                table: "Books",
                columns: new[] { "IsBoosted", "ListingLastDate" },
                filter: "[IsBoosted] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsSold",
                table: "Books",
                column: "IsSold");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsSold_CreatedAt",
                table: "Books",
                columns: new[] { "IsSold", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLocations_UserId_CreateDate",
                table: "UserLocations");

            migrationBuilder.DropIndex(
                name: "IX_Books_CreatedAt",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_IsBoosted_ListingLastDate",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_IsSold",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_IsSold_CreatedAt",
                table: "Books");
        }
    }
}
