using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudentRegistration.Infrastructure.SqlServer.Persistence;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations;

[DbContext(typeof(StudentRegistrationDbContext))]
[Migration("20260717120000_Spec017ExportFilter")]
public partial class Spec017ExportFilter : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FilterJson",
            schema: "administration",
            table: "ExportJobs",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: false,
            defaultValue: "{}");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FilterJson",
            schema: "administration",
            table: "ExportJobs");
    }
}
