using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Spec018RegistrationReadPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSubmissions_StudentTermStateReceived",
                schema: "registration",
                table: "RegistrationSubmissions",
                columns: new[] { "StudentId", "TermId", "ProcessingState", "ReceivedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistrationSubmissions_StudentTermStateReceived",
                schema: "registration",
                table: "RegistrationSubmissions");
        }
    }
}
