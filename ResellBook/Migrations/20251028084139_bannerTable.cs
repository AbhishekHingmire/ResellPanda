using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResellBook.Migrations
{
    /// <inheritdoc />
    public partial class bannerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_IsBoosted_ListingLastDate",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DistanceBoostingUpto",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IsBoosted",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ListingLastDate",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedirectURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Radius = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.AddColumn<int>(
                name: "DistanceBoostingUpto",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBoosted",
                table: "Books",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ListingLastDate",
                table: "Books",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsBoosted_ListingLastDate",
                table: "Books",
                columns: new[] { "IsBoosted", "ListingLastDate" },
                filter: "[IsBoosted] = 1");
        }
    }
}
