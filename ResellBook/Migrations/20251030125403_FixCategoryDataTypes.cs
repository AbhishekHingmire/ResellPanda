using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResellBook.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryDataTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add temporary columns for the new integer types
            migrationBuilder.AddColumn<int>(
                name: "Category_Int",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubCategory_Int",
                table: "Books",
                type: "int",
                nullable: true);

            // Update Category_Int based on string values
            migrationBuilder.Sql(@"
                UPDATE Books SET Category_Int = CASE
                    WHEN Category = 'Primary School Books' THEN 1
                    WHEN Category = 'Secondary School Books' THEN 2
                    WHEN Category = 'Senior Secondary Books' THEN 3
                    WHEN Category = 'Engineering & Technology' THEN 4
                    WHEN Category = 'Business & Management' THEN 5
                    WHEN Category = 'Medical & Health Sciences' THEN 6
                    WHEN Category = 'Competitive Exams' THEN 7
                    WHEN Category = 'Law & Judiciary' THEN 8
                    WHEN Category = 'Arts & Humanities' THEN 9
                    WHEN Category = 'Science & Mathematics' THEN 10
                    WHEN Category = 'Languages' THEN 11
                    WHEN Category = 'Vocational & Skill Development' THEN 12
                    WHEN Category = 'Test Prep & Certifications' THEN 13
                    WHEN Category = 'Hobbies & Extracurricular' THEN 14
                    ELSE 1 -- Default to Primary School Books
                END
            ");

            // For SubCategory, we'll set it to NULL for now since mapping all subcategories would be complex
            // The application can handle NULL subcategories
            migrationBuilder.Sql("UPDATE Books SET SubCategory_Int = NULL");

            // Drop the old string columns
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "Books");

            // Rename the temporary columns to the final names
            migrationBuilder.RenameColumn(
                name: "Category_Int",
                table: "Books",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "SubCategory_Int",
                table: "Books",
                newName: "SubCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back the string columns
            migrationBuilder.AddColumn<string>(
                name: "Category_Str",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory_Str",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            // Convert back from integers to strings
            migrationBuilder.Sql(@"
                UPDATE Books SET Category_Str = CASE
                    WHEN Category = 1 THEN 'Primary School Books'
                    WHEN Category = 2 THEN 'Secondary School Books'
                    WHEN Category = 3 THEN 'Senior Secondary Books'
                    WHEN Category = 4 THEN 'Engineering & Technology'
                    WHEN Category = 5 THEN 'Business & Management'
                    WHEN Category = 6 THEN 'Medical & Health Sciences'
                    WHEN Category = 7 THEN 'Competitive Exams'
                    WHEN Category = 8 THEN 'Law & Judiciary'
                    WHEN Category = 9 THEN 'Arts & Humanities'
                    WHEN Category = 10 THEN 'Science & Mathematics'
                    WHEN Category = 11 THEN 'Languages'
                    WHEN Category = 12 THEN 'Vocational & Skill Development'
                    WHEN Category = 13 THEN 'Test Prep & Certifications'
                    WHEN Category = 14 THEN 'Hobbies & Extracurricular'
                    ELSE 'Primary School Books'
                END
            ");

            migrationBuilder.Sql("UPDATE Books SET SubCategory_Str = NULL");

            // Drop the integer columns
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "Books");

            // Rename back to original names
            migrationBuilder.RenameColumn(
                name: "Category_Str",
                table: "Books",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "SubCategory_Str",
                table: "Books",
                newName: "SubCategory");
        }
    }
}
