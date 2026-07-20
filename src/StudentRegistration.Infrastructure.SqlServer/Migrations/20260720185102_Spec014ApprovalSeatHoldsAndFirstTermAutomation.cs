using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Spec014ApprovalSeatHoldsAndFirstTermAutomation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SectionGroups_Capacity",
                schema: "scheduling",
                table: "SectionGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrationSubmissions_ResultShape",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrationSubmissions_State",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.AddColumn<int>(
                name: "ProgramTermOrdinal",
                schema: "academics",
                table: "StudentTermAcademicStates",
                type: "int",
                nullable: false,
                // Existing rows are conservatively treated as self-service students.
                // A value of two prevents accidental first-term auto-enrollment until
                // an authoritative academic import supplies the real ordinal.
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "HeldSeatCount",
                schema: "scheduling",
                table: "SectionGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                schema: "registration",
                table: "RegistrationSubmissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "student-self-service");

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedCredits",
                schema: "registration",
                table: "RegistrationSubmissions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FirstTermAutoEnrollmentBatches",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogueVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CohortScope = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LeaseOwner = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstTermAutoEnrollmentBatches", x => x.Id);
                    table.CheckConstraint("CK_FirstTermAutoEnrollmentBatches_Lease", "([State] = 'running' AND [LeaseOwner] IS NOT NULL AND [LeaseExpiresAtUtc] IS NOT NULL) OR ([State] <> 'running' AND [LeaseOwner] IS NULL AND [LeaseExpiresAtUtc] IS NULL)");
                    table.CheckConstraint("CK_FirstTermAutoEnrollmentBatches_State", "[State] IN ('pending', 'running', 'complete', 'completed-with-failures', 'failed')");
                    table.ForeignKey(
                        name: "FK_FirstTermAutoEnrollmentBatches_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FirstTermAutoEnrollmentBatches_CatalogueVersions_CatalogueVersionId",
                        column: x => x.CatalogueVersionId,
                        principalSchema: "academics",
                        principalTable: "CatalogueVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationSubmissionLines",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubjectTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Credits = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationSubmissionLines", x => x.Id);
                    table.CheckConstraint("CK_RegistrationSubmissionLines_Credits", "[Credits] = 3");
                    table.CheckConstraint("CK_RegistrationSubmissionLines_State", "[State] IN ('pending-approval', 'approved', 'rejected', 'expired')");
                    table.ForeignKey(
                        name: "FK_RegistrationSubmissionLines_CourseOfferings_OfferingId",
                        column: x => x.OfferingId,
                        principalSchema: "scheduling",
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationSubmissionLines_RegistrationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "registration",
                        principalTable: "RegistrationSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationSubmissionLines_SectionGroups_OfferingId_GroupId",
                        columns: x => new { x.OfferingId, x.GroupId },
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumns: new[] { "OfferingId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FirstTermAutoEnrollmentItems",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstTermAutoEnrollmentItems", x => x.Id);
                    table.CheckConstraint("CK_FirstTermAutoEnrollmentItems_ResultShape", "([State] IN ('pending', 'running') AND [SubmissionId] IS NULL AND [FailureCode] IS NULL AND [CompletedAtUtc] IS NULL) OR ([State] = 'accepted' AND [SubmissionId] IS NOT NULL AND [FailureCode] IS NULL AND [CompletedAtUtc] IS NOT NULL) OR ([State] = 'failed' AND [SubmissionId] IS NULL AND [FailureCode] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL)");
                    table.CheckConstraint("CK_FirstTermAutoEnrollmentItems_State", "[State] IN ('pending', 'running', 'accepted', 'failed')");
                    table.ForeignKey(
                        name: "FK_FirstTermAutoEnrollmentItems_FirstTermAutoEnrollmentBatches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "registration",
                        principalTable: "FirstTermAutoEnrollmentBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FirstTermAutoEnrollmentItems_RegistrationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "registration",
                        principalTable: "RegistrationSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FirstTermAutoEnrollmentItems_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationApprovalDecisions",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorRole = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AssignmentScopeGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PolicySetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationApprovalDecisions", x => x.Id);
                    table.CheckConstraint("CK_RegistrationApprovalDecisions_Decision", "[Decision] IN ('approved', 'rejected')");
                    table.CheckConstraint("CK_RegistrationApprovalDecisions_Role", "[ActorRole] IN ('admin', 'lecturer', 'teaching-assistant')");
                    table.CheckConstraint("CK_RegistrationApprovalDecisions_Scope", "([ActorRole] = 'admin' AND [AssignmentScopeGroupId] IS NULL) OR ([ActorRole] <> 'admin' AND [AssignmentScopeGroupId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RegistrationApprovalDecisions_ApplicationUsers_ActorId",
                        column: x => x.ActorId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationApprovalDecisions_PolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalSchema: "academics",
                        principalTable: "PolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationApprovalDecisions_RegistrationSubmissionLines_SubmissionLineId",
                        column: x => x.SubmissionLineId,
                        principalSchema: "registration",
                        principalTable: "RegistrationSubmissionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationApprovalDecisions_SectionGroups_AssignmentScopeGroupId",
                        column: x => x.AssignmentScopeGroupId,
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationSeatHolds",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeldAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleaseReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationSeatHolds", x => x.Id);
                    table.CheckConstraint("CK_RegistrationSeatHolds_ResultShape", "([State] = 'active' AND [ReleasedAtUtc] IS NULL AND [ReleaseReason] IS NULL) OR ([State] = 'consumed' AND [ReleasedAtUtc] IS NOT NULL AND [ReleaseReason] IS NULL) OR ([State] IN ('released', 'expired') AND [ReleasedAtUtc] IS NOT NULL AND [ReleaseReason] IS NOT NULL)");
                    table.CheckConstraint("CK_RegistrationSeatHolds_State", "[State] IN ('active', 'consumed', 'released', 'expired')");
                    table.CheckConstraint("CK_RegistrationSeatHolds_Time", "[ReleasedAtUtc] IS NULL OR [ReleasedAtUtc] >= [HeldAtUtc]");
                    table.ForeignKey(
                        name: "FK_RegistrationSeatHolds_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationSeatHolds_RegistrationSubmissionLines_SubmissionLineId",
                        column: x => x.SubmissionLineId,
                        principalSchema: "registration",
                        principalTable: "RegistrationSubmissionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationSeatHolds_SectionGroups_OfferingId_GroupId",
                        columns: x => new { x.OfferingId, x.GroupId },
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumns: new[] { "OfferingId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationSeatHolds_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StudentTermAcademicStates_ProgramTermOrdinal",
                schema: "academics",
                table: "StudentTermAcademicStates",
                sql: "[ProgramTermOrdinal] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SectionGroups_Capacity",
                schema: "scheduling",
                table: "SectionGroups",
                sql: "[Capacity] >= 0 AND [EnrolledCount] >= 0 AND [HeldSeatCount] >= 0 AND [EnrolledCount] + [HeldSeatCount] <= [Capacity]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrationSubmissions_Origin",
                schema: "registration",
                table: "RegistrationSubmissions",
                sql: "[Origin] IN ('student-self-service', 'first-term-automatic')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrationSubmissions_RequestedCredits",
                schema: "registration",
                table: "RegistrationSubmissions",
                sql: "[RequestedCredits] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrationSubmissions_ResultShape",
                schema: "registration",
                table: "RegistrationSubmissions",
                sql: "[UpdatedAtUtc] >= [ReceivedAtUtc] AND ([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [ReceivedAtUtc]) AND (([ProcessingState] = 'processing' AND [ResultCode] IS NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR ([ProcessingState] = 'pending-approval' AND [ResultCode] = 'PENDING_APPROVAL' AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR ([ProcessingState] = 'accepted' AND [ResultCode] IS NOT NULL AND [Reference] IS NOT NULL AND [ReceiptSnapshotJson] IS NOT NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR ([ProcessingState] = 'rejected' AND [ResultCode] IS NOT NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR ([ProcessingState] = 'expired' AND [ResultCode] IS NOT NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrationSubmissions_State",
                schema: "registration",
                table: "RegistrationSubmissions",
                sql: "[ProcessingState] IN ('processing', 'pending-approval', 'accepted', 'rejected', 'expired')");

            migrationBuilder.CreateIndex(
                name: "IX_FirstTermAutoEnrollmentBatches_CatalogueVersionId",
                schema: "registration",
                table: "FirstTermAutoEnrollmentBatches",
                column: "CatalogueVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_FirstTermAutoEnrollmentBatches_TermId_CatalogueVersionId_CohortScope_Purpose",
                schema: "registration",
                table: "FirstTermAutoEnrollmentBatches",
                columns: new[] { "TermId", "CatalogueVersionId", "CohortScope", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirstTermAutoEnrollmentItems_BatchId_StudentId",
                schema: "registration",
                table: "FirstTermAutoEnrollmentItems",
                columns: new[] { "BatchId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirstTermAutoEnrollmentItems_StudentId_ClientRequestId",
                schema: "registration",
                table: "FirstTermAutoEnrollmentItems",
                columns: new[] { "StudentId", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirstTermAutoEnrollmentItems_SubmissionId",
                schema: "registration",
                table: "FirstTermAutoEnrollmentItems",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApprovalDecisions_ActorId_SubmissionLineId_ClientRequestId",
                schema: "registration",
                table: "RegistrationApprovalDecisions",
                columns: new[] { "ActorId", "SubmissionLineId", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApprovalDecisions_AssignmentScopeGroupId",
                schema: "registration",
                table: "RegistrationApprovalDecisions",
                column: "AssignmentScopeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApprovalDecisions_PolicySetId",
                schema: "registration",
                table: "RegistrationApprovalDecisions",
                column: "PolicySetId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApprovalDecisions_SubmissionLineId",
                schema: "registration",
                table: "RegistrationApprovalDecisions",
                column: "SubmissionLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSeatHolds_OfferingId_GroupId",
                schema: "registration",
                table: "RegistrationSeatHolds",
                columns: new[] { "OfferingId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSeatHolds_StudentId_TermId_OfferingId",
                schema: "registration",
                table: "RegistrationSeatHolds",
                columns: new[] { "StudentId", "TermId", "OfferingId" },
                unique: true,
                filter: "[State] = 'active'");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSeatHolds_SubmissionLineId",
                schema: "registration",
                table: "RegistrationSeatHolds",
                column: "SubmissionLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSeatHolds_TermId",
                schema: "registration",
                table: "RegistrationSeatHolds",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSubmissionLines_OfferingId_GroupId",
                schema: "registration",
                table: "RegistrationSubmissionLines",
                columns: new[] { "OfferingId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationSubmissionLines_SubmissionId_OfferingId",
                schema: "registration",
                table: "RegistrationSubmissionLines",
                columns: new[] { "SubmissionId", "OfferingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirstTermAutoEnrollmentItems",
                schema: "registration");

            migrationBuilder.DropTable(
                name: "RegistrationApprovalDecisions",
                schema: "registration");

            migrationBuilder.DropTable(
                name: "RegistrationSeatHolds",
                schema: "registration");

            migrationBuilder.DropTable(
                name: "FirstTermAutoEnrollmentBatches",
                schema: "registration");

            migrationBuilder.DropTable(
                name: "RegistrationSubmissionLines",
                schema: "registration");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StudentTermAcademicStates_ProgramTermOrdinal",
                schema: "academics",
                table: "StudentTermAcademicStates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SectionGroups_Capacity",
                schema: "scheduling",
                table: "SectionGroups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrationSubmissions_Origin",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrationSubmissions_RequestedCredits",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrationSubmissions_ResultShape",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrationSubmissions_State",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.DropColumn(
                name: "ProgramTermOrdinal",
                schema: "academics",
                table: "StudentTermAcademicStates");

            migrationBuilder.DropColumn(
                name: "HeldSeatCount",
                schema: "scheduling",
                table: "SectionGroups");

            migrationBuilder.DropColumn(
                name: "Origin",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.DropColumn(
                name: "RequestedCredits",
                schema: "registration",
                table: "RegistrationSubmissions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SectionGroups_Capacity",
                schema: "scheduling",
                table: "SectionGroups",
                sql: "[Capacity] >= 0 AND [EnrolledCount] >= 0 AND [EnrolledCount] <= [Capacity]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrationSubmissions_ResultShape",
                schema: "registration",
                table: "RegistrationSubmissions",
                sql: "[UpdatedAtUtc] >= [ReceivedAtUtc] AND ([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [ReceivedAtUtc]) AND (([ProcessingState] = 'processing' AND [ResultCode] IS NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR ([ProcessingState] = 'accepted' AND [ResultCode] IS NOT NULL AND [Reference] IS NOT NULL AND [ReceiptSnapshotJson] IS NOT NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR ([ProcessingState] = 'rejected' AND [ResultCode] IS NOT NULL AND [Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND [DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrationSubmissions_State",
                schema: "registration",
                table: "RegistrationSubmissions",
                sql: "[ProcessingState] IN ('processing', 'accepted', 'rejected')");
        }
    }
}
