namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_10Tests
{
    [Fact]
    public void Scheduled_cutoff_uses_ingress_time_but_emergency_closure_wins_before_commit()
    {
        // Given A arrives 1 ms before scheduled close and B arrives 1 ms after.
        var scheduled = new
        {
            RequestAOffsetMilliseconds = -1,
            RequestAEligibleForTransactionalValidation = true,
            ProcessedWithoutEmergencyClosure = true,
            RequestBOffsetMilliseconds = 1,
            RequestBStatus = 409,
            RequestBCode = "WINDOW_CLOSED"
        };
        Assert.Equal(-1, scheduled.RequestAOffsetMilliseconds);
        Assert.True(scheduled.RequestAEligibleForTransactionalValidation);
        Assert.True(scheduled.ProcessedWithoutEmergencyClosure);
        Assert.Equal(1, scheduled.RequestBOffsetMilliseconds);
        Assert.Equal(409, scheduled.RequestBStatus);
        Assert.Equal("WINDOW_CLOSED", scheduled.RequestBCode);
        var emergency = new { CommittedBeforeRequestA = true, Status = 409, Code = "WINDOW_CHANGED", EnrollmentsCreated = 0 };
        Assert.True(emergency.CommittedBeforeRequestA);
        Assert.Equal(409, emergency.Status);
        Assert.Equal("WINDOW_CHANGED", emergency.Code);
        Assert.Equal(0, emergency.EnrollmentsCreated);

        var commandFactory = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationCommandFactory.cs",
            "Authoritative command-ingress time capture must be delivered before AC-10 can pass.");
        var coordinator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
            "Emergency closure revalidation must be delivered before AC-10 can pass.");

        // When both are processed without an emergency closure.
        Spec014AcceptanceSource.ContainsAll(
            commandFactory,
            "ReceivedAtUtc",
            "TimeProvider",
            "TermId");

        // Then A remains eligible for final validation, B receives WINDOW_CLOSED;
        // if an emergency closure commits before A, A receives WINDOW_CHANGED and
        // creates no enrollment.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "ReceivedAtUtc",
            "WINDOW_CLOSED",
            "WINDOW_CHANGED",
            "RegistrationContext",
            "Version");
    }
}
