using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Registration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationSubmissions",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessingState = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ResultCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiptSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DecisionSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationSubmissions", x => x.Id);
                    table.CheckConstraint("CK_RegistrationSubmissions_DecisionSnapshotJson", "[DecisionSnapshotJson] IS NULL OR ISJSON([DecisionSnapshotJson]) = 1");
                    table.CheckConstraint("CK_RegistrationSubmissions_ReceiptSnapshotJson", "[ReceiptSnapshotJson] IS NULL OR ISJSON([ReceiptSnapshotJson]) = 1");
                    table.CheckConstraint("CK_RegistrationSubmissions_ResultShape", "[UpdatedAtUtc] >= [ReceivedAtUtc] AND ([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [ReceivedAtUtc]) AND (([ProcessingState] = 'processing' AND [ResultCode] IS NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR ([ProcessingState] = 'accepted' AND [ResultCode] IS NOT NULL AND [Reference] IS NOT NULL AND [ReceiptSnapshotJson] IS NOT NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR ([ProcessingState] = 'rejected' AND [ResultCode] IS NOT NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL))");
                    table.CheckConstraint("CK_RegistrationSubmissions_State", "[ProcessingState] IN ('processing', 'accepted', 'rejected')");
                    table.ForeignKey(
                        name: "FK_RegistrationSubmissions_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RegisteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.CheckConstraint("CK_Enrollments_State", "[State] IN ('active')");
                    table.ForeignKey(
                        name: "FK_Enrollments_RegistrationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "registration",
                        principalTable: "RegistrationSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_SectionGroups_OfferingId_GroupId",
                        columns: x => new { x.OfferingId, x.GroupId },
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumns: new[] { "OfferingId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_OfferingId_GroupId",
                schema: "registration",
                table: "Enrollments",
                columns: new[] { "OfferingId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_OfferingId",
                schema: "registration",
                table: "Enrollments",
                columns: new[] { "StudentId", "OfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_SubmissionId",
                schema: "registration",
                table: "Enrollments",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSubmissions_Reference",
                schema: "registration",
                table: "RegistrationSubmissions",
                column: "Reference",
                unique: true,
                filter: "[Reference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSubmissions_StudentId_TermId_ClientRequestId",
                schema: "registration",
                table: "RegistrationSubmissions",
                columns: new[] { "StudentId", "TermId", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSubmissions_TermId",
                schema: "registration",
                table: "RegistrationSubmissions",
                column: "TermId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments",
                schema: "registration");

            migrationBuilder.DropTable(
                name: "RegistrationSubmissions",
                schema: "registration");
        }
    }
}
