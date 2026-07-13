namespace StudentRegistration.IntegrationTests.Specs.Spec005.EdgeCases;

public sealed class EC_4Tests
{
    private const string DeferredReason =
        "Deferred reconciliation proof: activate SPEC-014 EnrollmentCounterReconciler/RegistrationModelConfiguration in S6Registration, SPEC-010 SectionGroup/SchedulingModelConfiguration in S2, SPEC-004 AuditEvent mapping, and SPEC-017 observation at entity-ownership 2.0.0 and persistence-manifest 2.1.0.";

    [Fact(Skip = DeferredReason)]
    public void Enrollment_counter_mismatch_alerts_and_pauses_before_authorized_audited_repair_without_silent_history_change()
    {
        // Given controlled SQL fault injection creates EnrolledCount != active Enrollment count.
        // When the scheduled reconciliation boundary runs.
        // Then it alerts and pauses the group; only Registration.Reconcile performs an idempotent audited repair from active rows,
        // and enrollment history is never silently rewritten before verified resume.
        throw new NotImplementedException(
            "Fault and repair the real S2/S6 schema with the transaction-aware audit writer; recomputing two local integers is not reconciliation evidence.");
    }
}
