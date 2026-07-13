namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class SectionGroupErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/SectionGroup.md",
            new(
                "SectionGroup",
                "SECTION_GROUP",
                "SPEC-010",
                "src/StudentRegistration.Scheduling/Domain/SectionGroup.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier OfferingId FK",
                    "string GroupCode", "int Capacity", "int EnrolledCount",
                    "string State", "bool RegistrationPaused", "rowversion Version"
                ],
                [
                    "COURSE_OFFERING ||--o{ SECTION_GROUP : has",
                    "Alternate key SectionGroup(Id, OfferingId), referenced by Enrollment",
                    "Check Capacity >= 0 and 0 <= EnrolledCount <= Capacity",
                    "Every group-state, MeetingSlot, room, and GroupStaffAssignment mutation locks and advances its owning SectionGroup.Version"
                ]));
}
