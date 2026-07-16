using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class CatalogueScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduling");

            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    AvailabilityState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.CheckConstraint("CK_Rooms_AvailabilityState", "[AvailabilityState] IN ('available', 'unavailable')");
                    table.CheckConstraint("CK_Rooms_Capacity", "[Capacity] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "StaffTermAvailabilities",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffTermAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffTermAvailabilities_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffTermAvailabilities_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffAvailabilities",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffTermAvailabilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartLocal = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndLocal = table.Column<TimeOnly>(type: "time", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAvailabilities", x => x.Id);
                    table.CheckConstraint("CK_StaffAvailabilities_DayOfWeek", "[DayOfWeek] >= 0 AND [DayOfWeek] <= 6");
                    table.CheckConstraint("CK_StaffAvailabilities_Kind", "[Kind] IN ('available', 'unavailable')");
                    table.CheckConstraint("CK_StaffAvailabilities_Range", "[EndLocal] > [StartLocal]");
                    table.ForeignKey(
                        name: "FK_StaffAvailabilities_StaffTermAvailabilities_StaffTermAvailabilityId",
                        column: x => x.StaffTermAvailabilityId,
                        principalSchema: "scheduling",
                        principalTable: "StaffTermAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogueDrafts",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BasedOnVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CanonicalContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidationSummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogueDrafts", x => x.Id);
                    table.CheckConstraint("CK_CatalogueDrafts_State", "[State] IN ('editing', 'validated', 'published', 'abandoned')");
                });

            migrationBuilder.CreateTable(
                name: "CatalogueVersions",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupersedesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScopeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VersionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogueVersions", x => x.Id);
                    table.CheckConstraint("CK_CatalogueVersions_State", "[State] IN ('published', 'superseded')");
                    table.ForeignKey(
                        name: "FK_CatalogueVersions_CatalogueDrafts_SourceDraftId",
                        column: x => x.SourceDraftId,
                        principalSchema: "academics",
                        principalTable: "CatalogueDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogueVersions_CatalogueVersions_SupersedesId",
                        column: x => x.SupersedesId,
                        principalSchema: "academics",
                        principalTable: "CatalogueVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogueVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Credits = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProvenanceSourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProvenanceAccessedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ProvenanceSourceKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProvenanceSyntheticFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.UniqueConstraint("AK_Courses_CatalogueVersionId_Id", x => new { x.CatalogueVersionId, x.Id });
                    table.CheckConstraint("CK_Courses_Credits", "[Credits] > 0");
                    table.ForeignKey(
                        name: "FK_Courses_CatalogueVersions_CatalogueVersionId",
                        column: x => x.CatalogueVersionId,
                        principalSchema: "academics",
                        principalTable: "CatalogueVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportBatches",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogueDraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AccessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SyntheticFieldCount = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                    table.CheckConstraint("CK_ImportBatches_State", "[State] IN ('uploaded', 'validating', 'invalid', 'validated', 'publishing', 'published', 'failed')");
                    table.CheckConstraint("CK_ImportBatches_SyntheticFieldCount", "[SyntheticFieldCount] >= 0");
                    table.ForeignKey(
                        name: "FK_ImportBatches_CatalogueDrafts_CatalogueDraftId",
                        column: x => x.CatalogueDraftId,
                        principalSchema: "academics",
                        principalTable: "CatalogueDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImportBatches_CatalogueVersions_PublishedVersionId",
                        column: x => x.PublishedVersionId,
                        principalSchema: "academics",
                        principalTable: "CatalogueVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogueVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProvenanceSourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProvenanceAccessedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ProvenanceSourceKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProvenanceSyntheticFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.UniqueConstraint("AK_Programs_CatalogueVersionId_Id", x => new { x.CatalogueVersionId, x.Id });
                    table.ForeignKey(
                        name: "FK_Programs_CatalogueVersions_CatalogueVersionId",
                        column: x => x.CatalogueVersionId,
                        principalSchema: "academics",
                        principalTable: "CatalogueVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseOfferings",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOfferings", x => x.Id);
                    table.CheckConstraint("CK_CourseOfferings_State", "[State] IN ('draft', 'published', 'closed', 'cancelled')");
                    table.ForeignKey(
                        name: "FK_CourseOfferings_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseOfferings_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoursePrerequisites",
                schema: "academics",
                columns: table => new
                {
                    CatalogueVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequiredCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinimumGrade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProvenanceSourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProvenanceAccessedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ProvenanceSourceKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProvenanceSyntheticFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePrerequisites", x => new { x.CatalogueVersionId, x.CourseId, x.RequiredCourseId });
                    table.CheckConstraint("CK_CoursePrerequisites_NotSelf", "[CourseId] <> [RequiredCourseId]");
                    table.ForeignKey(
                        name: "FK_CoursePrerequisites_Courses_CatalogueVersionId_CourseId",
                        columns: x => new { x.CatalogueVersionId, x.CourseId },
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumns: new[] { "CatalogueVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePrerequisites_Courses_CatalogueVersionId_RequiredCourseId",
                        columns: x => new { x.CatalogueVersionId, x.RequiredCourseId },
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumns: new[] { "CatalogueVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumCourses",
                schema: "academics",
                columns: table => new
                {
                    CatalogueVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    RecommendedTerm = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CohortScope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProvenanceSourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProvenanceAccessedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ProvenanceSourceKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProvenanceSyntheticFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumCourses", x => new { x.CatalogueVersionId, x.ProgramId, x.CourseId });
                    table.CheckConstraint("CK_CurriculumCourses_Level", "[Level] > 0");
                    table.CheckConstraint("CK_CurriculumCourses_RecommendedTerm", "[RecommendedTerm] IS NULL OR [RecommendedTerm] > 0");
                    table.ForeignKey(
                        name: "FK_CurriculumCourses_Courses_CatalogueVersionId_CourseId",
                        columns: x => new { x.CatalogueVersionId, x.CourseId },
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumns: new[] { "CatalogueVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurriculumCourses_Programs_CatalogueVersionId_ProgramId",
                        columns: x => new { x.CatalogueVersionId, x.ProgramId },
                        principalSchema: "academics",
                        principalTable: "Programs",
                        principalColumns: new[] { "CatalogueVersionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicySets",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScopeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VersionToken = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicySets", x => x.Id);
                    table.CheckConstraint("CK_PolicySets_EffectiveRange", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]");
                    table.CheckConstraint("CK_PolicySets_State", "[State] IN ('draft', 'validated', 'published', 'superseded')");
                    table.ForeignKey(
                        name: "FK_PolicySets_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicySets_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "academics",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionGroups",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    EnrolledCount = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegistrationPaused = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionGroups", x => x.Id);
                    table.UniqueConstraint("AK_SectionGroups_Id_OfferingId", x => new { x.Id, x.OfferingId });
                    table.CheckConstraint("CK_SectionGroups_Capacity", "[Capacity] >= 0 AND [EnrolledCount] >= 0 AND [EnrolledCount] <= [Capacity]");
                    table.CheckConstraint("CK_SectionGroups_State", "[State] IN ('draft', 'published', 'closed', 'cancelled')");
                    table.ForeignKey(
                        name: "FK_SectionGroups_CourseOfferings_OfferingId",
                        column: x => x.OfferingId,
                        principalSchema: "scheduling",
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyRules",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicySetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValueType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SourceKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyRules_PolicySets_PolicySetId",
                        column: x => x.PolicySetId,
                        principalSchema: "academics",
                        principalTable: "PolicySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingSlots",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartLocal = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndLocal = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingSlots", x => x.Id);
                    table.UniqueConstraint("AK_MeetingSlots_Id_GroupId", x => new { x.Id, x.GroupId });
                    table.CheckConstraint("CK_MeetingSlots_ActivityType", "[ActivityType] IN ('lecture', 'tutorial', 'laboratory')");
                    table.CheckConstraint("CK_MeetingSlots_DayOfWeek", "[DayOfWeek] >= 0 AND [DayOfWeek] <= 6");
                    table.CheckConstraint("CK_MeetingSlots_Range", "[EndLocal] > [StartLocal]");
                    table.ForeignKey(
                        name: "FK_MeetingSlots_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "scheduling",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingSlots_SectionGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleImpactAlerts",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffTermAvailabilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReasonCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DetectedGroupVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false),
                    DetectedResourceVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevalidatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastValidationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastRevalidationPassed = table.Column<bool>(type: "bit", nullable: false),
                    ResolutionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleImpactAlerts", x => x.Id);
                    table.CheckConstraint("CK_ScheduleImpactAlerts_Resource", "[StaffTermAvailabilityId] IS NOT NULL OR [RoomId] IS NOT NULL");
                    table.CheckConstraint("CK_ScheduleImpactAlerts_State", "[State] IN ('open', 'revalidated', 'resolved')");
                    table.ForeignKey(
                        name: "FK_ScheduleImpactAlerts_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "scheduling",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleImpactAlerts_SectionGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleImpactAlerts_StaffTermAvailabilities_StaffTermAvailabilityId",
                        column: x => x.StaffTermAvailabilityId,
                        principalSchema: "scheduling",
                        principalTable: "StaffTermAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupStaffAssignments",
                schema: "scheduling",
                columns: table => new
                {
                    MeetingSlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachingRole = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupStaffAssignments", x => new { x.MeetingSlotId, x.StaffId, x.TeachingRole });
                    table.CheckConstraint("CK_GroupStaffAssignments_RoleActivity", "([ActivityType] = 'lecture' AND [TeachingRole] = 'lecturer') OR ([ActivityType] IN ('tutorial', 'laboratory') AND [TeachingRole] = 'teaching-assistant')");
                    table.ForeignKey(
                        name: "FK_GroupStaffAssignments_MeetingSlots_MeetingSlotId_GroupId",
                        columns: x => new { x.MeetingSlotId, x.GroupId },
                        principalSchema: "scheduling",
                        principalTable: "MeetingSlots",
                        principalColumns: new[] { "Id", "GroupId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupStaffAssignments_SectionGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupStaffAssignments_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueDrafts_BasedOnVersionId",
                schema: "academics",
                table: "CatalogueDrafts",
                column: "BasedOnVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueDrafts_ScopeCode_State_Id",
                schema: "academics",
                table: "CatalogueDrafts",
                columns: new[] { "ScopeCode", "State", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueVersions_ScopeCode_State_PublishedAtUtc_Id",
                schema: "academics",
                table: "CatalogueVersions",
                columns: new[] { "ScopeCode", "State", "PublishedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueVersions_ScopeCode_VersionCode",
                schema: "academics",
                table: "CatalogueVersions",
                columns: new[] { "ScopeCode", "VersionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueVersions_SourceDraftId",
                schema: "academics",
                table: "CatalogueVersions",
                column: "SourceDraftId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogueVersions_SupersedesId",
                schema: "academics",
                table: "CatalogueVersions",
                column: "SupersedesId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_CourseId",
                schema: "scheduling",
                table: "CourseOfferings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_TermId_CourseId",
                schema: "scheduling",
                table: "CourseOfferings",
                columns: new[] { "TermId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_TermId_State_Id",
                schema: "scheduling",
                table: "CourseOfferings",
                columns: new[] { "TermId", "State", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisites_CatalogueVersionId_RequiredCourseId",
                schema: "academics",
                table: "CoursePrerequisites",
                columns: new[] { "CatalogueVersionId", "RequiredCourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CatalogueVersionId_Code",
                schema: "academics",
                table: "Courses",
                columns: new[] { "CatalogueVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumCourses_CatalogueVersionId_CourseId",
                schema: "academics",
                table: "CurriculumCourses",
                columns: new[] { "CatalogueVersionId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumCourses_ProgramId_CohortScope_RecommendedTerm_CourseId",
                schema: "academics",
                table: "CurriculumCourses",
                columns: new[] { "ProgramId", "CohortScope", "RecommendedTerm", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffAssignments_GroupId_ActivityType_MeetingSlotId_StaffId",
                schema: "scheduling",
                table: "GroupStaffAssignments",
                columns: new[] { "GroupId", "ActivityType", "MeetingSlotId", "StaffId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffAssignments_MeetingSlotId_GroupId",
                schema: "scheduling",
                table: "GroupStaffAssignments",
                columns: new[] { "MeetingSlotId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffAssignments_StaffId_GroupId_MeetingSlotId",
                schema: "scheduling",
                table: "GroupStaffAssignments",
                columns: new[] { "StaffId", "GroupId", "MeetingSlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_CatalogueDraftId_State",
                schema: "academics",
                table: "ImportBatches",
                columns: new[] { "CatalogueDraftId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_PublishedVersionId",
                schema: "academics",
                table: "ImportBatches",
                column: "PublishedVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSlots_GroupId_ActivityType_DayOfWeek_StartLocal_Id",
                schema: "scheduling",
                table: "MeetingSlots",
                columns: new[] { "GroupId", "ActivityType", "DayOfWeek", "StartLocal", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSlots_RoomId_DayOfWeek_StartLocal_EndLocal_GroupId",
                schema: "scheduling",
                table: "MeetingSlots",
                columns: new[] { "RoomId", "DayOfWeek", "StartLocal", "EndLocal", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRules_PolicySetId_Code",
                schema: "academics",
                table: "PolicyRules",
                columns: new[] { "PolicySetId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicySets_ProgramId",
                schema: "academics",
                table: "PolicySets",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicySets_ScopeCode_State_EffectiveFromUtc_Id",
                schema: "academics",
                table: "PolicySets",
                columns: new[] { "ScopeCode", "State", "EffectiveFromUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicySets_ScopeCode_VersionCode",
                schema: "academics",
                table: "PolicySets",
                columns: new[] { "ScopeCode", "VersionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicySets_TermId",
                schema: "academics",
                table: "PolicySets",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_CatalogueVersionId_Code",
                schema: "academics",
                table: "Programs",
                columns: new[] { "CatalogueVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_AvailabilityState_Capacity_Code_Id",
                schema: "scheduling",
                table: "Rooms",
                columns: new[] { "AvailabilityState", "Capacity", "Code", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Code",
                schema: "scheduling",
                table: "Rooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleImpactAlerts_GroupId_State_Id",
                schema: "scheduling",
                table: "ScheduleImpactAlerts",
                columns: new[] { "GroupId", "State", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleImpactAlerts_RoomId_State_Id",
                schema: "scheduling",
                table: "ScheduleImpactAlerts",
                columns: new[] { "RoomId", "State", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleImpactAlerts_StaffTermAvailabilityId_State_Id",
                schema: "scheduling",
                table: "ScheduleImpactAlerts",
                columns: new[] { "StaffTermAvailabilityId", "State", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleImpactAlerts_State_DetectedAtUtc_Id",
                schema: "scheduling",
                table: "ScheduleImpactAlerts",
                columns: new[] { "State", "DetectedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroups_OfferingId_GroupCode",
                schema: "scheduling",
                table: "SectionGroups",
                columns: new[] { "OfferingId", "GroupCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroups_OfferingId_State_RegistrationPaused_Id",
                schema: "scheduling",
                table: "SectionGroups",
                columns: new[] { "OfferingId", "State", "RegistrationPaused", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAvailabilities_StaffTermAvailabilityId_DayOfWeek_StartLocal_EndLocal",
                schema: "scheduling",
                table: "StaffAvailabilities",
                columns: new[] { "StaffTermAvailabilityId", "DayOfWeek", "StartLocal", "EndLocal" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffTermAvailabilities_StaffId_TermId",
                schema: "scheduling",
                table: "StaffTermAvailabilities",
                columns: new[] { "StaffId", "TermId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffTermAvailabilities_TermId_DeadlineUtc_Id",
                schema: "scheduling",
                table: "StaffTermAvailabilities",
                columns: new[] { "TermId", "DeadlineUtc", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogueDrafts_CatalogueVersions_BasedOnVersionId",
                schema: "academics",
                table: "CatalogueDrafts",
                column: "BasedOnVersionId",
                principalSchema: "academics",
                principalTable: "CatalogueVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                EXEC(N'
                    CREATE OR ALTER TRIGGER [scheduling].[TR_MeetingSlots_AdvanceSectionGroupVersion]
                    ON [scheduling].[MeetingSlots]
                    AFTER INSERT, UPDATE, DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        UPDATE groups
                        SET [RegistrationPaused] = groups.[RegistrationPaused]
                        FROM [scheduling].[SectionGroups] AS groups
                        INNER JOIN
                        (
                            SELECT [GroupId] FROM inserted
                            UNION
                            SELECT [GroupId] FROM deleted
                        ) AS changed ON changed.[GroupId] = groups.[Id];
                    END;
                ');
                """);

            migrationBuilder.Sql(
                """
                EXEC(N'
                    CREATE OR ALTER TRIGGER [scheduling].[TR_GroupStaffAssignments_AdvanceSectionGroupVersion]
                    ON [scheduling].[GroupStaffAssignments]
                    AFTER INSERT, UPDATE, DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        UPDATE groups
                        SET [RegistrationPaused] = groups.[RegistrationPaused]
                        FROM [scheduling].[SectionGroups] AS groups
                        INNER JOIN
                        (
                            SELECT [GroupId] FROM inserted
                            UNION
                            SELECT [GroupId] FROM deleted
                        ) AS changed ON changed.[GroupId] = groups.[Id];
                    END;
                ');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogueDrafts_CatalogueVersions_BasedOnVersionId",
                schema: "academics",
                table: "CatalogueDrafts");

            migrationBuilder.DropTable(
                name: "CoursePrerequisites",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "CurriculumCourses",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "GroupStaffAssignments",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "ImportBatches",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "PolicyRules",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "ScheduleImpactAlerts",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "StaffAvailabilities",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "MeetingSlots",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "PolicySets",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "StaffTermAvailabilities",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "SectionGroups",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "Programs",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "CourseOfferings",
                schema: "scheduling");

            migrationBuilder.DropTable(
                name: "Courses",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "CatalogueVersions",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "CatalogueDrafts",
                schema: "academics");
        }
    }
}
