namespace StudentRegistration.SpecificationTests.Specs.Spec005.Entities;

public sealed class PolicySetErdContractTests
{
    [Fact]
    public void Reference_declares_owner_source_fields_and_invariants() =>
        ErdReferenceContractAssertions.AssertReference(
            "specs/005-erd-data-lifecycle/contracts/entities/PolicySet.md",
            new(
                "PolicySet",
                "POLICY_SET",
                "SPEC-009",
                "src/StudentRegistration.Academics/Domain/PolicySet.cs",
                [
                    "uniqueidentifier Id PK", "string Version", "uniqueidentifier TermId FK",
                    "uniqueidentifier ProgramId FK", "string ApprovalStatus",
                    "datetime2 EffectiveFromUtc", "rowversion VersionToken"
                ],
                [
                    "ACADEMIC_TERM ||--o{ POLICY_SET : governed_by",
                    "POLICY_SET ||--o{ POLICY_RULE : contains",
                    "Published policy sets are immutable and superseded by new effective-dated versions"
                ]));
}
