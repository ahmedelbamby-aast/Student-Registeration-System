using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAcademicFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "academics");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "AcademicTerms",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreationClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationPayloadHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeachingStartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    TeachingEndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicTerms", x => x.Id);
                    table.CheckConstraint("CK_AcademicTerms_State", "[State] IN ('draft', 'registrationOpen', 'registrationClosed', 'teaching', 'completed', 'archived')");
                    table.CheckConstraint("CK_AcademicTerms_TeachingRange", "[TeachingEndsOn] > [TeachingStartsOn]");
                });

            migrationBuilder.CreateTable(
                name: "AdminSecurityGuards",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSecurityGuards", x => x.Id);
                    table.CheckConstraint("CK_AdminSecurityGuards_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UniversityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BeforeSummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterSummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationAbuseStates",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectKeyHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    WindowStartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LockedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationAbuseStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationWindows",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ScopeValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OpensAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosesAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationWindows", x => x.Id);
                    table.CheckConstraint("CK_RegistrationWindows_Range", "[ClosesAtUtc] > [OpensAtUtc]");
                    table.CheckConstraint("CK_RegistrationWindows_Scope", "([ScopeType] = 'all-students' AND [ScopeValue] IS NULL) OR ([ScopeType] IN ('program', 'cohort') AND [ScopeValue] IS NOT NULL AND LEN(LTRIM(RTRIM([ScopeValue]))) > 0)");
                    table.CheckConstraint("CK_RegistrationWindows_State", "[State] IN ('draft', 'published', 'emergencyClosed', 'superseded')");
                    table.ForeignKey(
                        name: "FK_RegistrationWindows_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountRecoveryChallenges",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DeliveryReferenceHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FailedAttemptCount = table.Column<int>(type: "int", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRecoveryChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountRecoveryChallenges_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdentityImportBatches",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientRequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SourceHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorSummaryJson = table.Column<string>(type: "nvarchar(max)", maxLength: 16000, nullable: false),
                    ResultSummaryJson = table.Column<string>(type: "nvarchar(max)", maxLength: 16000, nullable: true),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityImportBatches", x => x.Id);
                    table.CheckConstraint("CK_IdentityImportBatches_State", "[State] IN ('uploaded', 'invalid', 'validated', 'published', 'failed')");
                    table.ForeignKey(
                        name: "FK_IdentityImportBatches_ApplicationUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.Id);
                    table.CheckConstraint("CK_RoleAssignments_EffectiveRange", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]");
                    table.ForeignKey(
                        name: "FK_RoleAssignments_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActorReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BeforeSummaryJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    AfterSummaryJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityEvents_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentActivations",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvisionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAttemptCount = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentActivations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentActivations_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cohort = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentGpa = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    EarnedCredits = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Standing = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataAsOfUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.CheckConstraint("CK_Students_CurrentGpa", "[CurrentGpa] >= 0 AND [CurrentGpa] <= 4");
                    table.CheckConstraint("CK_Students_EarnedCredits", "[EarnedCredits] >= 0");
                    table.ForeignKey(
                        name: "FK_Students_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalSchema: "auth",
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdentityImportCandidateRows",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    ExternalReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UniversityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StaffNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Roles = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityImportCandidateRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityImportCandidateRows_IdentityImportBatches_IdentityImportBatchId",
                        column: x => x.IdentityImportBatchId,
                        principalSchema: "auth",
                        principalTable: "IdentityImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentHolds",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BlocksRegistration = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentHolds", x => x.Id);
                    table.CheckConstraint("CK_StudentHolds_EffectiveRange", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]");
                    table.ForeignKey(
                        name: "FK_StudentHolds_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentHolds_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentTermAcademicStates",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GpaAtStart = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    EarnedCreditsAtStart = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    StandingAtStart = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataAsOfUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTermAcademicStates", x => x.Id);
                    table.CheckConstraint("CK_StudentTermAcademicStates_EarnedCreditsAtStart", "[EarnedCreditsAtStart] >= 0");
                    table.CheckConstraint("CK_StudentTermAcademicStates_GpaAtStart", "[GpaAtStart] >= 0 AND [GpaAtStart] <= 4");
                    table.ForeignKey(
                        name: "FK_StudentTermAcademicStates_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTermAcademicStates_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TranscriptAttempts",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupersedesAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CourseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Credits = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    GradeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscriptAttempts", x => x.Id);
                    table.CheckConstraint("CK_TranscriptAttempts_Credits", "[Credits] > 0");
                    table.CheckConstraint("CK_TranscriptAttempts_NotSelfSuperseding", "[SupersedesAttemptId] IS NULL OR [SupersedesAttemptId] <> [Id]");
                    table.CheckConstraint("CK_TranscriptAttempts_Status", "[Status] IN ('in-progress', 'passed', 'failed', 'withdrawn')");
                    table.ForeignKey(
                        name: "FK_TranscriptAttempts_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TranscriptAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TranscriptAttempts_TranscriptAttempts_SupersedesAttemptId",
                        column: x => x.SupersedesAttemptId,
                        principalSchema: "academics",
                        principalTable: "TranscriptAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicTerms_Code",
                schema: "academics",
                table: "AcademicTerms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicTerms_CreationClientRequestId",
                schema: "academics",
                table: "AcademicTerms",
                column: "CreationClientRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicTerms_State_TeachingStartsOn_Id",
                schema: "academics",
                table: "AcademicTerms",
                columns: new[] { "State", "TeachingStartsOn", "Id" });

            migrationBuilder.CreateIndex(
                name: "UX_AcademicTerms_OneRegistrationOpen",
                schema: "academics",
                table: "AcademicTerms",
                column: "State",
                unique: true,
                filter: "[State] = 'registrationOpen'");

            migrationBuilder.CreateIndex(
                name: "UX_AcademicTerms_OneTeaching",
                schema: "academics",
                table: "AcademicTerms",
                column: "State",
                unique: true,
                filter: "[State] = 'teaching'");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryChallenges_ApplicationUserId_ExpiresAtUtc_ConsumedAtUtc",
                schema: "auth",
                table: "AccountRecoveryChallenges",
                columns: new[] { "ApplicationUserId", "ExpiresAtUtc", "ConsumedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryChallenges_TokenHash",
                schema: "auth",
                table: "AccountRecoveryChallenges",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_IsEnabled_LockoutEndUtc",
                schema: "auth",
                table: "ApplicationUsers",
                columns: new[] { "IsEnabled", "LockoutEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_NormalizedUserName",
                schema: "auth",
                table: "ApplicationUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_UniversityId",
                schema: "auth",
                table: "ApplicationUsers",
                column: "UniversityId",
                unique: true,
                filter: "[UniversityId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_UserName",
                schema: "auth",
                table: "ApplicationUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_CorrelationId",
                schema: "audit",
                table: "AuditEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_OccurredAtUtc",
                schema: "audit",
                table: "AuditEvents",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAbuseStates_Operation_LockedUntilUtc",
                schema: "auth",
                table: "AuthenticationAbuseStates",
                columns: new[] { "Operation", "LockedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAbuseStates_SubjectKeyHash",
                schema: "auth",
                table: "AuthenticationAbuseStates",
                column: "SubjectKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityImportBatches_RequestedByUserId_ClientRequestId",
                schema: "auth",
                table: "IdentityImportBatches",
                columns: new[] { "RequestedByUserId", "ClientRequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityImportBatches_SourceHash",
                schema: "auth",
                table: "IdentityImportBatches",
                column: "SourceHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityImportBatches_State_ImportedAtUtc",
                schema: "auth",
                table: "IdentityImportBatches",
                columns: new[] { "State", "ImportedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityImportCandidateRows_IdentityImportBatchId_ExternalReference",
                schema: "auth",
                table: "IdentityImportCandidateRows",
                columns: new[] { "IdentityImportBatchId", "ExternalReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityImportCandidateRows_IdentityImportBatchId_Ordinal",
                schema: "auth",
                table: "IdentityImportCandidateRows",
                columns: new[] { "IdentityImportBatchId", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationWindows_TermId_Id",
                schema: "academics",
                table: "RegistrationWindows",
                columns: new[] { "TermId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationWindows_TermId_State_OpensAtUtc_ClosesAtUtc",
                schema: "academics",
                table: "RegistrationWindows",
                columns: new[] { "TermId", "State", "OpensAtUtc", "ClosesAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_ApplicationUserId_RoleCode_EffectiveFromUtc",
                schema: "auth",
                table: "RoleAssignments",
                columns: new[] { "ApplicationUserId", "RoleCode", "EffectiveFromUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_RoleCode_EffectiveFromUtc_EffectiveToUtc",
                schema: "auth",
                table: "RoleAssignments",
                columns: new[] { "RoleCode", "EffectiveFromUtc", "EffectiveToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_ApplicationUserId",
                schema: "auth",
                table: "SecurityEvents",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_CorrelationId",
                schema: "auth",
                table: "SecurityEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_EventType_OccurredAtUtc",
                schema: "auth",
                table: "SecurityEvents",
                columns: new[] { "EventType", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ApplicationUserId",
                schema: "auth",
                table: "Staff",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_StaffNumber",
                schema: "auth",
                table: "Staff",
                column: "StaffNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentActivations_ApplicationUserId",
                schema: "auth",
                table: "StudentActivations",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentHolds_StudentId_TermId_EffectiveFromUtc_EffectiveToUtc",
                schema: "academics",
                table: "StudentHolds",
                columns: new[] { "StudentId", "TermId", "EffectiveFromUtc", "EffectiveToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentHolds_TermId",
                schema: "academics",
                table: "StudentHolds",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ApplicationUserId",
                schema: "academics",
                table: "Students",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ProgramCode_Cohort_Standing_Id",
                schema: "academics",
                table: "Students",
                columns: new[] { "ProgramCode", "Cohort", "Standing", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentTermAcademicStates_StudentId_TermId",
                schema: "academics",
                table: "StudentTermAcademicStates",
                columns: new[] { "StudentId", "TermId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentTermAcademicStates_TermId",
                schema: "academics",
                table: "StudentTermAcademicStates",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAttempts_StudentId_CourseCode",
                schema: "academics",
                table: "TranscriptAttempts",
                columns: new[] { "StudentId", "CourseCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAttempts_StudentId_ImportedAtUtc_Id",
                schema: "academics",
                table: "TranscriptAttempts",
                columns: new[] { "StudentId", "ImportedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAttempts_SupersedesAttemptId",
                schema: "academics",
                table: "TranscriptAttempts",
                column: "SupersedesAttemptId",
                unique: true,
                filter: "[SupersedesAttemptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAttempts_TermId",
                schema: "academics",
                table: "TranscriptAttempts",
                column: "TermId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountRecoveryChallenges",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AdminSecurityGuards",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuditEvents",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuthenticationAbuseStates",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "IdentityImportCandidateRows",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "RegistrationWindows",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "RoleAssignments",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "SecurityEvents",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Staff",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StudentActivations",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StudentHolds",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "StudentTermAcademicStates",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "TranscriptAttempts",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "IdentityImportBatches",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AcademicTerms",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "ApplicationUsers",
                schema: "auth");
        }
    }
}
