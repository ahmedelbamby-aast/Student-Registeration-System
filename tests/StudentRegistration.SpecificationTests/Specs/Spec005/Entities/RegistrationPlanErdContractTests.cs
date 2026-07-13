namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class RegistrationPlanErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/RegistrationPlan.md",
            new(
                "RegistrationPlan",
                "REGISTRATION_PLAN",
                "SPEC-012",
                "src/StudentRegistration.Registration/Domain/RegistrationPlan.cs",
                [
                    "uniqueidentifier Id PK", "uniqueidentifier StudentId FK",
                    "uniqueidentifier TermId FK", "string State", "rowversion Version"
                ],
                [
                    "STUDENT ||--o{ REGISTRATION_PLAN : prepares",
                    "REGISTRATION_PLAN ||--o{ REGISTRATION_PLAN_ITEM : contains",
                    "Unique RegistrationPlanItem(PlanId, OfferingId)",
                    "rowversion on mutable aggregate roots and admin records"
                ]));
}
