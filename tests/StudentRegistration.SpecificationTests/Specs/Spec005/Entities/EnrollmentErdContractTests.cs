namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class EnrollmentErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/Enrollment.md",
            new(
                "Enrollment",
                "ENROLLMENT",
                "SPEC-014",
                "src/StudentRegistration.Registration/Domain/Enrollment.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier StudentId FK",
                    "uniqueidentifier OfferingId FK", "uniqueidentifier GroupId FK",
                    "uniqueidentifier SubmissionId FK", "string State",
                    "datetime2 RegisteredAtUtc", "rowversion Version"
                ],
                [
                    "SECTION_GROUP ||--o{ ENROLLMENT : allocates",
                    "Alternate key SectionGroup(Id, OfferingId), referenced by Enrollment",
                    "Unique Enrollment(StudentId, OfferingId)",
                    "Enrollments retain successful registration history"
                ]));
}
