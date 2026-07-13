namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class CourseOfferingErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/CourseOffering.md",
            new(
                "CourseOffering",
                "COURSE_OFFERING",
                "SPEC-010",
                "src/StudentRegistration.Scheduling/Domain/CourseOffering.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier TermId FK",
                    "uniqueidentifier CourseId FK", "string State", "rowversion Version"
                ],
                [
                    "COURSE ||--o{ COURSE_OFFERING : offered_as",
                    "COURSE_OFFERING ||--o{ SECTION_GROUP : has",
                    "Unique CourseOffering(TermId, CourseId)",
                    "rowversion on mutable aggregate roots and admin records"
                ]));
}
