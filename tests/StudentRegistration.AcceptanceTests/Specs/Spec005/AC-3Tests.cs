namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_3Tests
{
    private const string DeferredReason =
        "Deferred real-SQL history proof: activate SPEC-009 PolicySet/CatalogueModelConfiguration in S2CatalogueScheduling and SPEC-014 RegistrationSubmission/DecisionSnapshot/RegistrationModelConfiguration in S6Registration at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Superseding_policy_2026_1_with_2026_2_preserves_the_immutable_historical_submission_view()
    {
        // Given published policy 2026.1 referenced by a stored registration decision.
        // When published policy 2026.2 supersedes it.
        // Then 2026.1 cannot be mutated and remains queryable through the historical submission snapshot.
        throw new NotImplementedException(
            "Run against the real SPEC-009 and SPEC-014 mappings; a copied object or markdown assertion is not historical persistence proof.");
    }
}
