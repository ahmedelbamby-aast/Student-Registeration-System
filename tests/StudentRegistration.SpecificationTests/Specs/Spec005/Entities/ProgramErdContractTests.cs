namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class ProgramErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/Program.md",
            new(
                "Program",
                "PROGRAM",
                "SPEC-009",
                "src/StudentRegistration.Academics/Domain/Program.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier CatalogueVersionId FK",
                    "string Code", "string Name"
                ],
                [
                    "PROGRAM ||--o{ CURRICULUM_COURSE : defines",
                    "PROGRAM ||--o{ POLICY_SET : specializes",
                    "Unique Program(CatalogueVersionId, Code)",
                    "published catalogue version and all of its program/course/curriculum rows are immutable"
                ]));
}
