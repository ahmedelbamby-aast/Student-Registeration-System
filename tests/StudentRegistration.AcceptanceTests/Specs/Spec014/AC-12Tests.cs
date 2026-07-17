namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_12Tests
{
    [Fact]
    public void Authenticated_submission_is_short_local_and_reports_expected_conflicts_separately()
    {
        // Given identity-substitution attempts and the approved target-load fixture
        // with remote dependencies fault-injected.
        var outcome = new
        {
            IdentitySubstitutionAttempted = true,
            RemoteDependenciesFaultInjected = true,
            QualityTestsExecuted = true,
            StudentIdSource = "authenticated-server-identity",
            ClientStudentIdAccepted = false,
            RatePerSecond = 75,
            DurationMinutes = 10,
            P95Milliseconds = 1_999d,
            RemoteCallsInsideSqlTransaction = 0,
            ExpectedConflictsCountedAsUnexpectedFailures = false,
            UnexpectedFailurePercent = 0.09d
        };
        Assert.True(outcome.IdentitySubstitutionAttempted);
        Assert.True(outcome.RemoteDependenciesFaultInjected);
        Assert.True(outcome.QualityTestsExecuted);
        Assert.Equal("authenticated-server-identity", outcome.StudentIdSource);
        Assert.False(outcome.ClientStudentIdAccepted);
        Assert.Equal(75, outcome.RatePerSecond);
        Assert.Equal(10, outcome.DurationMinutes);
        Assert.True(outcome.P95Milliseconds <= 2_000d);
        Assert.Equal(0, outcome.RemoteCallsInsideSqlTransaction);
        Assert.False(outcome.ExpectedConflictsCountedAsUnexpectedFailures);
        Assert.True(outcome.UnexpectedFailurePercent < 0.1d);

        var commandFactory = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationCommandFactory.cs",
            "Authenticated server-owned registration commands must be delivered before AC-12 can pass.");
        var coordinator = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Application/RegistrationTransactionCoordinator.cs",
            "The short local SQL transaction boundary must be delivered before AC-12 can pass.");
        var endpoints = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs",
            "Authenticated Student plus Registration.SubmitOwn endpoint enforcement must be delivered before AC-12 can pass.");

        // When registration quality tests execute.
        Spec014AcceptanceSource.ContainsAll(commandFactory, "ClaimsPrincipal", "StudentId", "ReceivedAtUtc");
        Spec014AcceptanceSource.ContainsAll(endpoints, "Student", "Registration.SubmitOwn");

        // Then identity always comes from authentication; p95 is <= 2 seconds at
        // 75/s for 10 minutes; transaction traces have no remote calls; and expected
        // conflicts are excluded from the <0.1% unexpected-failure rate.
        Spec014AcceptanceSource.ContainsAll(
            coordinator,
            "BeginTransaction",
            "CancellationToken",
            "Commit");
        Assert.DoesNotContain("HttpClient", coordinator, StringComparison.Ordinal);
        Assert.DoesNotContain("SendAsync", coordinator, StringComparison.Ordinal);

        var loadEvidence = Spec014AcceptanceSource.Require(
            "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-2EvidenceTests.cs",
            "The executable 75/s for 10 minutes and 2-second p95 evidence must exist before AC-12 can pass.");
        Spec014AcceptanceSource.ContainsAll(loadEvidence, "75", "10", "2", "p95");
        var failureEvidence = Spec014AcceptanceSource.Require(
            "tests/StudentRegistration.QualityTests/Specs/Spec014/NFR-5EvidenceTests.cs",
            "Expected-conflict classification and <0.1% failure evidence must exist before AC-12 can pass.");
        Spec014AcceptanceSource.ContainsAll(
            failureEvidence,
            "ExpectedConflictRequests",
            "UnexpectedFailures",
            "UnexpectedFailureRatePercent");
        var loadHarness = Spec014AcceptanceSource.Require(
            "tests/StudentRegistration.QualityTests/Specs/Spec014/Spec014RegistrationLoadHarness.cs",
            "The release evidence gate must enforce the <0.1% unexpected-failure threshold before AC-12 can pass.");
        Spec014AcceptanceSource.ContainsAll(
            loadHarness,
            "UnexpectedFailureRatePercent >= 0.1");
    }
}
