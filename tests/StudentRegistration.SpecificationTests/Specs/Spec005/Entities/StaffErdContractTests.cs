namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class StaffErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/Staff.md",
            new(
                "Staff",
                "STAFF",
                "SPEC-007",
                "src/StudentRegistration.IdentityAccess/Domain/Staff.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier ApplicationUserId FK,UK",
                    "string StaffNumber UK", "string DisplayName", "bool IsActive"
                ],
                [
                    "STAFF ||--o{ GROUP_STAFF_ASSIGNMENT : assigned",
                    "STAFF ||--o{ STAFF_TERM_AVAILABILITY : declares",
                    "unique Staff.StaffNumber",
                    "Scheduling owns StaffTermAvailability and StaffAvailability"
                ]));
}
