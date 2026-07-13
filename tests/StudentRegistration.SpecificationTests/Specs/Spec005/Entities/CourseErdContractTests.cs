namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class CourseErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/Course.md",
            new(
                "Course",
                "COURSE",
                "SPEC-009",
                "src/StudentRegistration.Academics/Domain/Course.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier CatalogueVersionId FK",
                    "string Code", "string Title", "decimal Credits", "bool IsActive"
                ],
                [
                    "COURSE ||--o{ COURSE_PREREQUISITE : course",
                    "COURSE ||--o{ COURSE_PREREQUISITE : required",
                    "Course(CatalogueVersionId, Code)",
                    "published catalogue version and all of its program/course/curriculum rows are immutable"
                ]));
}
