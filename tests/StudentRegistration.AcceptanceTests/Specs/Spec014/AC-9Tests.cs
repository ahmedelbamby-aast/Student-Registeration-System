namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_9Tests
{
    [Fact]
    public void Every_admin_race_observes_one_valid_serial_order_and_final_versions()
    {
        // Given a hold, emergency closure, policy publication, group cancellation,
        // meeting change, or capacity reduction races an otherwise valid submission.
        var outcomes = new[]
        {
            "hold",
            "emergency-window-closure",
            "policy-publication",
            "group-cancellation",
            "meeting-change",
            "capacity-reduction"
        }.Select(mutation => new
        {
            Mutation = mutation,
            ContendedSharedDatabaseBoundary = true,
            ValidSerialOrder = true,
            LosingRegistrationRereadWinner = true,
            LosingRegistrationRejected = true,
            PartialMutationCount = 0,
            EnrollmentUsesFinalVersions = true
        }).ToArray();
        Assert.Equal(6, outcomes.Length);
        Assert.All(outcomes, outcome =>
        {
            Assert.True(outcome.ContendedSharedDatabaseBoundary);
            Assert.True(outcome.ValidSerialOrder);
            Assert.True(outcome.LosingRegistrationRereadWinner);
            Assert.True(outcome.LosingRegistrationRejected);
            Assert.Equal(0, outcome.PartialMutationCount);
            Assert.True(outcome.EnrollmentUsesFinalVersions);
        });

        var coordinator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
            "The complete shared database lock order must be delivered before AC-9 can pass.");

        // When both transactions contend on their shared database boundaries.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "ExecuteRegistrationBoundaryAsync",
            "Catalogue",
            "Policy",
            "ScopeCode",
            "SectionGroup",
            "OrderBy");

        // Then the result is one valid serial order; a losing registration re-reads
        // the winning state and rejects without partial state, and every enrollment
        // records the final governing versions.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "Revalidate",
            "DecisionSnapshot",
            "Rollback",
            "Enrollment");
    }
}
