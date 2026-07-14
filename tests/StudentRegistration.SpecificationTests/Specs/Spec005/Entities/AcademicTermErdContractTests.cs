namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class AcademicTermErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/AcademicTerm.md",
            new(
                "AcademicTerm",
                "ACADEMIC_TERM",
                "SPEC-008",
                "src/StudentRegistration.Academics/Domain/AcademicTerm.cs",
                [
                    "uniqueidentifier Id PK", "string Code UK",
                    "uniqueidentifier CreationClientRequestId UK", "string CreationPayloadHash",
                    "string DisplayName",
                    "date TeachingStartsOn", "date TeachingEndsOn", "string TimeZoneId",
                    "string State", "rowversion Version"
                ],
                [
                    "ACADEMIC_TERM ||--o{ REGISTRATION_WINDOW : exposes",
                    "ACADEMIC_TERM ||--o{ COURSE_OFFERING : contains",
                    "AcademicTerm.Code",
                    "AcademicTerm.CreationClientRequestId",
                    "CreationPayloadHash",
                    "TeachingEndsOn > TeachingStartsOn"
                ]));
}
