namespace StudentRegistration.AcceptanceTests.Specs.Spec005;

public sealed class AC_5Tests
{
    private const string DeferredReason =
        "Deferred production-rehearsal proof: requires the reviewed S1/S2/S4/S6 bundle pinned by persistence-manifest 2.1.0, a production-like backup, an approved numeric AASTMT Operations window, and passing SPEC-018 restore evidence; production topology remains fail closed.";

    [Fact(Skip = DeferredReason)]
    public void Reviewed_bundle_is_applied_only_as_a_controlled_step_and_rollback_restores_the_verified_backup()
    {
        // Given a reviewed migration bundle and verified production-like backup.
        // When the deployment rehearsal runs through the controlled operation.
        // Then startup applies no migration and the documented rollback restores the prior verified state.
        throw new NotImplementedException(
            "Execute the approved bundle/rollback rehearsal with measured evidence; startup configuration text is not deployment proof.");
    }
}
