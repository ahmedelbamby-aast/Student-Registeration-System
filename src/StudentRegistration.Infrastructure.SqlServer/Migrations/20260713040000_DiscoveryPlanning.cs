using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistration.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class DiscoveryPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "registration");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_SectionGroups_OfferingId_Id",
                schema: "scheduling",
                table: "SectionGroups",
                columns: new[] { "OfferingId", "Id" });

            migrationBuilder.CreateTable(
                name: "RegistrationPlans",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalCredits = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ConflictsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidationSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationPlans", x => x.Id);
                    table.CheckConstraint("CK_RegistrationPlans_ConflictsJson", "ISJSON([ConflictsJson]) = 1");
                    table.CheckConstraint("CK_RegistrationPlans_State", "[State] IN ('draft', 'review-blocked')");
                    table.CheckConstraint("CK_RegistrationPlans_TotalCredits", "[TotalCredits] >= 0");
                    table.CheckConstraint("CK_RegistrationPlans_ValidationSnapshotJson", "[ValidationSnapshotJson] IS NULL OR ISJSON([ValidationSnapshotJson]) = 1");
                    table.ForeignKey(
                        name: "FK_RegistrationPlans_AcademicTerms_TermId",
                        column: x => x.TermId,
                        principalSchema: "academics",
                        principalTable: "AcademicTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationPlans_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "academics",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationPlanItems",
                schema: "registration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapturedOfferingVersion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CapturedGroupVersion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationPlanItems_CourseOfferings_OfferingId",
                        column: x => x.OfferingId,
                        principalSchema: "scheduling",
                        principalTable: "CourseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationPlanItems_RegistrationPlans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "registration",
                        principalTable: "RegistrationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationPlanItems_SectionGroups_OfferingId_SelectedGroupId",
                        columns: x => new { x.OfferingId, x.SelectedGroupId },
                        principalSchema: "scheduling",
                        principalTable: "SectionGroups",
                        principalColumns: new[] { "OfferingId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationPlanItems_OfferingId_SelectedGroupId",
                schema: "registration",
                table: "RegistrationPlanItems",
                columns: new[] { "OfferingId", "SelectedGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationPlanItems_PlanId_OfferingId",
                schema: "registration",
                table: "RegistrationPlanItems",
                columns: new[] { "PlanId", "OfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationPlanItems_PlanId_SelectedGroupId",
                schema: "registration",
                table: "RegistrationPlanItems",
                columns: new[] { "PlanId", "SelectedGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationPlans_StudentId_TermId",
                schema: "registration",
                table: "RegistrationPlans",
                columns: new[] { "StudentId", "TermId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationPlans_TermId",
                schema: "registration",
                table: "RegistrationPlans",
                column: "TermId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationPlanItems",
                schema: "registration");

            migrationBuilder.DropTable(
                name: "RegistrationPlans",
                schema: "registration");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SectionGroups_OfferingId_Id",
                schema: "scheduling",
                table: "SectionGroups");
        }
    }
}
