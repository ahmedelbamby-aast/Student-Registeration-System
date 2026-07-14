namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class StudentErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/Student.md",
            new(
                "Student",
                "STUDENT",
                "SPEC-008",
                "src/StudentRegistration.Academics/Domain/Student.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier ApplicationUserId FK,UK",
                    "string ProgramCode", "string Cohort", "decimal CurrentGpa",
                    "decimal EarnedCredits", "string Standing", "bool IsActive",
                    "string Source", "string SourceReference", "string DataVersion",
                    "datetime2 DataAsOfUtc", "datetime2 ImportedAtUtc", "rowversion Version"
                ],
                [
                    "STUDENT ||--o{ TRANSCRIPT_ATTEMPT : has",
                    "STUDENT ||--o{ REGISTRATION_PLAN : prepares",
                    "Unique StudentTermAcademicState(StudentId, TermId)",
                    "No transcript attempt is overwritten"
                ]));
}
