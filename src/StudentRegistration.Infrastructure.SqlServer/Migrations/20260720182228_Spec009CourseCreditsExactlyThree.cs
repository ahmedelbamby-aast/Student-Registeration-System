using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Spec009CourseCreditsExactlyThree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_Credits",
                schema: "academics",
                table: "Courses");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_Credits",
                schema: "academics",
                table: "Courses",
                sql: "[Credits] = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_Credits",
                schema: "academics",
                table: "Courses");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_Credits",
                schema: "academics",
                table: "Courses",
                sql: "[Credits] > 0");
        }
    }
}
