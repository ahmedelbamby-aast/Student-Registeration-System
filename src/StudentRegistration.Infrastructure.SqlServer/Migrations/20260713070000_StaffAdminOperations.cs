using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class StaffAdminOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "administration");

            migrationBuilder.CreateTable(
                name: "ExportJobs",
                schema: "administration",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ArtifactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseOwnerId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportJobs", x => x.JobId);
                    table.CheckConstraint("CK_ExportJobs_AttemptCount", "[AttemptCount] >= 0 AND [AttemptCount] <= 3");
                    table.CheckConstraint("CK_ExportJobs_LeaseShape", "(([State] = 'running' AND [LeaseOwnerId] IS NOT NULL AND [LeaseExpiresAtUtc] IS NOT NULL AND [AttemptCount] >= 1) OR ([State] <> 'running' AND [LeaseOwnerId] IS NULL AND [LeaseExpiresAtUtc] IS NULL))");
                    table.CheckConstraint("CK_ExportJobs_ResultShape", "(([State] IN ('pending', 'running') AND [ArtifactId] IS NULL AND [CompletedAtUtc] IS NULL AND [ExpiresAtUtc] IS NULL AND [FailureCode] IS NULL) OR ([State] IN ('complete', 'expired') AND [ArtifactId] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL AND [ExpiresAtUtc] IS NOT NULL AND [ExpiresAtUtc] > [CompletedAtUtc] AND [FailureCode] IS NULL) OR ([State] = 'failed' AND [ArtifactId] IS NULL AND [CompletedAtUtc] IS NOT NULL AND [ExpiresAtUtc] IS NULL AND [FailureCode] IS NOT NULL))");
                    table.CheckConstraint("CK_ExportJobs_State", "[State] IN ('pending', 'running', 'complete', 'failed', 'expired')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_ArtifactId",
                schema: "administration",
                table: "ExportJobs",
                column: "ArtifactId",
                unique: true,
                filter: "[ArtifactId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_ExpiresAtUtc_JobId",
                schema: "administration",
                table: "ExportJobs",
                columns: new[] { "ExpiresAtUtc", "JobId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_OwnerId_ScopeHash_ClientRequestId",
                schema: "administration",
                table: "ExportJobs",
                columns: new[] { "OwnerId", "ScopeHash", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExportJobs_State_LeaseExpiresAtUtc_AttemptCount_JobId",
                schema: "administration",
                table: "ExportJobs",
                columns: new[] { "State", "LeaseExpiresAtUtc", "AttemptCount", "JobId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportJobs",
                schema: "administration");
        }
    }
}
