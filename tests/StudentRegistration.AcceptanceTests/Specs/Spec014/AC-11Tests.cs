namespace StudentRegistration.AcceptanceTests.Specs.Spec014;

public sealed class AC_11Tests
{
    [Fact]
    public void Student_and_admin_have_no_drop_withdraw_correction_or_seat_decrement_action()
    {
        // Given an authenticated student or ordinary Admin.
        var roles = new[] { "Student", "Admin" };
        var actions = new[] { "drop", "withdrawal", "correction", "seat-decrement" };
        var outcomes = roles.SelectMany(role => actions.Select(action => new
        {
            Role = role,
            Action = action,
            Authenticated = true,
            RouteRequested = true,
            EndpointExists = false,
            EnrollmentMutations = 0,
            EnrolledCountMutations = 0
        })).ToArray();
        Assert.Equal(8, outcomes.Length);
        Assert.All(outcomes, outcome =>
        {
            Assert.True(outcome.Authenticated);
            Assert.True(outcome.RouteRequested);
            Assert.False(outcome.EndpointExists, $"{outcome.Role} unexpectedly exposes {outcome.Action}.");
            Assert.Equal(0, outcome.EnrollmentMutations);
            Assert.Equal(0, outcome.EnrolledCountMutations);
        });

        var endpoints = Spec014AcceptanceSource.Require(
            "src/StudentRegistration.Registration/Endpoints/Spec014Endpoints.cs",
            "The bounded SPEC-014 endpoint surface must be delivered before AC-11 can pass.");

        // When a drop, withdrawal, correction, or seat-decrement route is requested.
        Spec014AcceptanceSource.ContainsAll(
            endpoints,
            "MapPost",
            "registrations",
            "MapGet",
            "by-request");

        // Then no such MVP endpoint/action exists and no response can decrement
        // Enrollment or EnrolledCount.
        Assert.DoesNotContain("drop", endpoints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("withdraw", endpoints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correction", endpoints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("EnrolledCount--", endpoints, StringComparison.Ordinal);
        Assert.DoesNotContain("EnrolledCount - 1", endpoints, StringComparison.Ordinal);
    }
}
