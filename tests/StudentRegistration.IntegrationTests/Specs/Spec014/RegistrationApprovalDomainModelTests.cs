using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class RegistrationApprovalDomainModelTests
{
    [Fact]
    public void Line_allows_exactly_one_terminal_decision()
    {
        var line = CreateLine();

        line.Approve();

        Assert.Equal(RegistrationSubmissionLineState.Approved, line.State);
        Assert.Throws<InvalidOperationException>(line.Reject);
        Assert.Throws<InvalidOperationException>(line.Expire);
        Assert.Empty(line.Version);
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegistrationSubmissionLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "CS999", "Invalid", 2m));
    }

    [Fact]
    public void Active_hold_can_be_consumed_released_or_expired_only_once()
    {
        var heldAt = Utc(9);
        var consumed = CreateHold(heldAt);
        consumed.Consume(Utc(10));
        Assert.Equal(RegistrationSeatHoldState.Consumed, consumed.State);
        Assert.Equal(Utc(10), consumed.ReleasedAtUtc);
        Assert.Throws<InvalidOperationException>(() => consumed.Release("REJECTED", Utc(11)));

        var released = CreateHold(heldAt);
        released.Release("LINE_REJECTED", Utc(10));
        Assert.Equal(RegistrationSeatHoldState.Released, released.State);
        Assert.Equal("LINE_REJECTED", released.ReleaseReason);

        var expired = CreateHold(heldAt);
        expired.Expire("REGISTRATION_WINDOW_CLOSED", Utc(10));
        Assert.Equal(RegistrationSeatHoldState.Expired, expired.State);
    }

    [Fact]
    public void Approval_decision_is_immutable_and_staff_scope_is_explicit()
    {
        var groupId = Guid.NewGuid();
        var decision = new RegistrationApprovalDecision(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            RegistrationApprovalActorRole.Lecturer,
            RegistrationApprovalDecisionValue.Approved,
            "Prerequisites verified", groupId, Guid.NewGuid(),
            "sha256:decision", Guid.NewGuid(), "2026.1", "corr-1", Utc(10));

        Assert.Equal(groupId, decision.AssignmentScopeGroupId);
        Assert.Equal(RegistrationApprovalDecisionValue.Approved, decision.Decision);
        Assert.False(typeof(RegistrationApprovalDecision)
            .GetProperty(nameof(RegistrationApprovalDecision.Decision))!.SetMethod?.IsPublic ?? false);

        Assert.Throws<ArgumentException>(() => new RegistrationApprovalDecision(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            RegistrationApprovalActorRole.TeachingAssistant,
            RegistrationApprovalDecisionValue.Approved,
            "ok", null, Guid.NewGuid(), "hash", Guid.NewGuid(), "v1", "corr", Utc(10)));
        Assert.Throws<ArgumentException>(() => new RegistrationApprovalDecision(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            RegistrationApprovalActorRole.Admin,
            RegistrationApprovalDecisionValue.Approved,
            "ok", groupId, Guid.NewGuid(), "hash", Guid.NewGuid(), "v1", "corr", Utc(10)));
    }

    private static RegistrationSubmissionLine CreateLine() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        "CS201", "Algorithms", 3m);

    private static RegistrationSeatHold CreateHold(DateTime heldAtUtc) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        Guid.NewGuid(), Guid.NewGuid(), heldAtUtc);

    private static DateTime Utc(int hour) =>
        new(2026, 7, 20, hour, 0, 0, DateTimeKind.Utc);
}
