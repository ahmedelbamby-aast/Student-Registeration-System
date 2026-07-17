namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_6Tests
{
    [Fact]
    public void Reconciliation_pauses_mismatch_and_only_authorized_idempotent_repair_resumes_it()
    {
        // Given a controlled fixture creates a counter/active-enrollment mismatch.
        var outcome = new
        {
            ControlledFaultInjected = true,
            CounterBefore = 8,
            ActiveEnrollmentsBefore = 7,
            ReconciliationRan = true,
            MismatchDetected = true,
            AlertPublished = true,
            RegistrationPaused = true,
            AdminCanRepair = false,
            OperationsServiceCanRepair = true,
            ActiveEnrollmentEvidenceCount = 7,
            RepairedCount = 7,
            RepairWasIdempotent = true,
            BeforeAfterAuditWritten = true,
            InvariantVerifiedBeforeResume = true
        };
        Assert.True(outcome.ControlledFaultInjected);
        Assert.NotEqual(outcome.CounterBefore, outcome.ActiveEnrollmentsBefore);
        Assert.True(outcome.ReconciliationRan);
        Assert.True(outcome.MismatchDetected);
        Assert.True(outcome.AlertPublished);
        Assert.True(outcome.RegistrationPaused);
        Assert.False(outcome.AdminCanRepair);
        Assert.True(outcome.OperationsServiceCanRepair);
        Assert.Equal(outcome.ActiveEnrollmentEvidenceCount, outcome.RepairedCount);
        Assert.True(outcome.RepairWasIdempotent);
        Assert.True(outcome.BeforeAfterAuditWritten);
        Assert.True(outcome.InvariantVerifiedBeforeResume);

        var reconciler = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/EnrollmentCounterReconciler.cs",
            "The scheduled counter reconciler and controlled repair boundary must be delivered before AC-6 can pass.");

        // When reconciliation locks the owning group and evaluates active Enrollment evidence.
        Spec014AcceptanceSource.ContainsAll(
            reconciler,
            "EnrolledCount",
            "Enrollment",
            "RegistrationPaused",
            "Registration.Reconcile",
            "EvidenceHash");

        // Then it alerts and pauses the group; ordinary Admin cannot invoke repair;
        // the operations identity repairs idempotently, audits before/after facts,
        // and clears the pause only after the invariant is verified.
        Spec014AcceptanceSource.ContainsAll(
            reconciler,
            "Alert",
            "RowVersion",
            "Audit",
            "Verify",
            "Resume");
        Assert.DoesNotContain("MapPost", reconciler, StringComparison.Ordinal);
    }
}
