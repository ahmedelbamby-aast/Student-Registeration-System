using System.Security.Claims;
using StudentRegistration.Registration.Application;

namespace StudentRegistration.ApplicationTests.Registration;

public sealed class RegistrationCommandFactoryTests
{
    private static readonly Guid ApplicationUserId = Guid.NewGuid();
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid TermId = Guid.NewGuid();
    private static readonly Guid PlanId = Guid.NewGuid();
    private static readonly Guid ClientRequestId = Guid.NewGuid();
    private static readonly DateTimeOffset ScheduledOpenUtc =
        new(2026, 7, 1, 6, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ScheduledCloseUtc =
        new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_uses_only_the_authenticated_identity_and_resolved_student()
    {
        var clock = new CountingTimeProvider(ScheduledCloseUtc.AddMilliseconds(-1));
        var factory = new RegistrationCommandFactory(clock);

        var result = factory.Create(
            Principal(ApplicationUserId),
            TermId,
            Request(),
            Context());

        Assert.Equal(RegistrationCommandCreationOutcome.Created, result.Outcome);
        Assert.NotNull(result.Command);
        Assert.Equal(StudentId, result.Command.StudentId);
        Assert.Equal(ApplicationUserId, result.Command.ApplicationUserId);

        var requestProperties = typeof(SubmitRegistrationRequest)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain("StudentId", requestProperties);
        Assert.DoesNotContain("TermId", requestProperties);
    }

    [Fact]
    public void Create_rejects_an_unauthenticated_or_substituted_identity()
    {
        var factory = new RegistrationCommandFactory(
            new CountingTimeProvider(ScheduledCloseUtc.AddMinutes(-1)));

        var unauthenticated = factory.Create(
            new ClaimsPrincipal(new ClaimsIdentity()),
            TermId,
            Request(),
            Context());
        var substituted = factory.Create(
            Principal(Guid.NewGuid()),
            TermId,
            Request(),
            Context());

        Assert.Equal(
            RegistrationCommandCreationOutcome.Unauthorized,
            unauthenticated.Outcome);
        Assert.Null(unauthenticated.Command);
        Assert.Equal(
            RegistrationCommandCreationOutcome.RegistrationContextNotFound,
            substituted.Outcome);
        Assert.Null(substituted.Command);
    }

    [Fact]
    public void Create_rejects_a_route_term_outside_the_resolved_context()
    {
        var factory = new RegistrationCommandFactory(
            new CountingTimeProvider(ScheduledCloseUtc.AddMinutes(-1)));

        var result = factory.Create(
            Principal(ApplicationUserId),
            Guid.NewGuid(),
            Request(),
            Context());

        Assert.Equal(
            RegistrationCommandCreationOutcome.RegistrationContextNotFound,
            result.Outcome);
        Assert.Null(result.Command);
    }

    [Fact]
    public void Create_copies_the_plan_version_request_key_and_canonical_scope()
    {
        var factory = new RegistrationCommandFactory(
            new CountingTimeProvider(ScheduledCloseUtc.AddMinutes(-1)));

        var result = factory.Create(
            Principal(ApplicationUserId),
            TermId,
            Request(),
            Context());

        Assert.NotNull(result.Command);
        Assert.Equal(PlanId, result.Command.PlanId);
        Assert.Equal("plan-v7", result.Command.ExpectedPlanRowVersion);
        Assert.Equal(ClientRequestId, result.Command.ClientRequestId);
        Assert.Equal(
            new RegistrationIdempotencyScope(StudentId, TermId, ClientRequestId),
            result.Command.IdempotencyScope);
    }

    [Fact]
    public void Create_captures_one_authoritative_utc_instant_at_ingress()
    {
        var receivedAtUtc = ScheduledCloseUtc.AddMilliseconds(-1);
        var clock = new CountingTimeProvider(receivedAtUtc);
        var factory = new RegistrationCommandFactory(clock);

        var result = factory.Create(
            Principal(ApplicationUserId),
            TermId,
            Request(),
            Context());

        Assert.Equal(RegistrationCommandCreationOutcome.Created, result.Outcome);
        Assert.NotNull(result.Command);
        Assert.Equal(receivedAtUtc.UtcDateTime, result.Command.ReceivedAtUtc);
        Assert.Equal(DateTimeKind.Utc, result.Command.ReceivedAtUtc.Kind);
        Assert.Equal(1, clock.ReadCount);
    }

    [Fact]
    public void Create_uses_received_at_for_the_scheduled_cutoff()
    {
        var beforeClose = new RegistrationCommandFactory(
            new CountingTimeProvider(ScheduledCloseUtc.AddMilliseconds(-1)))
            .Create(Principal(ApplicationUserId), TermId, Request(), Context());
        var atClose = new RegistrationCommandFactory(
            new CountingTimeProvider(ScheduledCloseUtc))
            .Create(Principal(ApplicationUserId), TermId, Request(), Context());

        Assert.Equal(RegistrationCommandCreationOutcome.Created, beforeClose.Outcome);
        Assert.Equal(RegistrationCommandCreationOutcome.WindowClosed, atClose.Outcome);
        Assert.Equal("WINDOW_CLOSED", atClose.ErrorCode);
        Assert.Null(atClose.Command);
    }

    [Fact]
    public void Create_captures_context_version_and_rejects_an_existing_emergency_close()
    {
        var clock = new CountingTimeProvider(ScheduledCloseUtc.AddMinutes(-1));
        var factory = new RegistrationCommandFactory(clock);

        var open = factory.Create(
            Principal(ApplicationUserId),
            TermId,
            Request(),
            Context());
        var emergencyClosed = factory.Create(
            Principal(ApplicationUserId),
            TermId,
            Request(),
            Context(emergencyClosed: true));

        Assert.NotNull(open.Command);
        Assert.Equal("registration-context-v11", open.Command.ExpectedRegistrationContextVersion);
        Assert.Equal(
            RegistrationCommandCreationOutcome.WindowChanged,
            emergencyClosed.Outcome);
        Assert.Equal("WINDOW_CHANGED", emergencyClosed.ErrorCode);
        Assert.Null(emergencyClosed.Command);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("plan-v7", false)]
    public void Create_rejects_invalid_client_command_fields(
        string expectedPlanVersion,
        bool useValidRequestIds)
    {
        var request = new SubmitRegistrationRequest(
            useValidRequestIds ? PlanId : Guid.Empty,
            expectedPlanVersion,
            useValidRequestIds ? ClientRequestId : Guid.Empty);
        var factory = new RegistrationCommandFactory(
            new CountingTimeProvider(ScheduledCloseUtc.AddMinutes(-1)));

        var result = factory.Create(
            Principal(ApplicationUserId),
            TermId,
            request,
            Context());

        Assert.Equal(RegistrationCommandCreationOutcome.InvalidRequest, result.Outcome);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
        Assert.Null(result.Command);
    }

    private static ClaimsPrincipal Principal(Guid applicationUserId) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, applicationUserId.ToString("D"))],
            authenticationType: "test"));

    private static SubmitRegistrationRequest Request() =>
        new(PlanId, "plan-v7", ClientRequestId);

    private static ResolvedRegistrationContext Context(bool emergencyClosed = false) =>
        new(
            ApplicationUserId,
            StudentId,
            TermId,
            "registration-context-v11",
            ScheduledOpenUtc.UtcDateTime,
            ScheduledCloseUtc.UtcDateTime,
            emergencyClosed);

    private sealed class CountingTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public int ReadCount { get; private set; }

        public override DateTimeOffset GetUtcNow()
        {
            ReadCount++;
            return utcNow;
        }
    }
}
