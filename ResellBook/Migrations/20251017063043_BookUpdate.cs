using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResellBook.Migrations
{
    /// <inheritdoc />
    public partial class BookUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceBoostingUpto",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IsBoosted",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ListingLastDate",
                table: "Books");
        }
    }
}
