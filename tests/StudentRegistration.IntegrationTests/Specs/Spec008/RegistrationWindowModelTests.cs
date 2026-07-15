using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;

namespace StudentRegistration.IntegrationTests.Specs.Spec008;

public sealed class RegistrationWindowModelTests
{
    private static readonly DateTime OpensAtUtc =
        new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime ClosesAtUtc =
        new(2026, 9, 10, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_preserves_the_approved_window_fields_and_rowversion_shape()
    {
        var id = Guid.NewGuid();
        var termId = Guid.NewGuid();

        var window = new RegistrationWindow(
            id,
            termId,
            RegistrationWindowScopeType.Program,
            "AI",
            OpensAtUtc,
            ClosesAtUtc,
            RegistrationWindowLifecycleState.Draft);

        Assert.Equal(id, window.Id);
        Assert.Equal(termId, window.TermId);
        Assert.Equal(RegistrationWindowScopeType.Program, window.ScopeType);
        Assert.Equal("AI", window.ScopeValue);
        Assert.Equal(OpensAtUtc, window.OpensAtUtc);
        Assert.Equal(ClosesAtUtc, window.ClosesAtUtc);
        Assert.Equal(RegistrationWindowLifecycleState.Draft, window.State);
        Assert.Empty(window.Version);
    }

    [Fact]
    public void Constructor_requires_identifiers_utc_instants_and_a_positive_interval()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(
            opensAtUtc: DateTime.SpecifyKind(OpensAtUtc, DateTimeKind.Local)));
        Assert.Throws<ArgumentException>(() => Create(
            closesAtUtc: DateTime.SpecifyKind(ClosesAtUtc, DateTimeKind.Unspecified)));
        Assert.Throws<ArgumentException>(() => Create(closesAtUtc: OpensAtUtc));
        Assert.Throws<ArgumentException>(() => Create(closesAtUtc: OpensAtUtc.AddTicks(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            lifecycleState: (RegistrationWindowLifecycleState)999));
    }

    [Fact]
    public void Scope_type_and_value_must_form_one_normalized_scope()
    {
        var allStudents = Create(
            scopeType: RegistrationWindowScopeType.AllStudents,
            scopeValue: null);
        var cohort = Create(
            scopeType: RegistrationWindowScopeType.Cohort,
            scopeValue: "2026");

        Assert.Null(allStudents.ScopeValue);
        Assert.Equal("2026", cohort.ScopeValue);
        Assert.Throws<ArgumentException>(() => Create(
            scopeType: RegistrationWindowScopeType.AllStudents,
            scopeValue: "AI"));
        Assert.Throws<ArgumentException>(() => Create(
            scopeType: RegistrationWindowScopeType.Program,
            scopeValue: null));
        Assert.Throws<ArgumentException>(() => Create(
            scopeType: RegistrationWindowScopeType.Cohort,
            scopeValue: " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            scopeType: (RegistrationWindowScopeType)999,
            scopeValue: null));
    }

    [Fact]
    public void Published_window_uses_the_authoritative_half_open_utc_interval()
    {
        var window = Create(lifecycleState: RegistrationWindowLifecycleState.Published);

        Assert.Equal(
            RegistrationWindowState.Upcoming,
            window.GetComputedState(OpensAtUtc.AddTicks(-1)));
        Assert.Equal(
            RegistrationWindowState.Open,
            window.GetComputedState(OpensAtUtc));
        Assert.Equal(
            RegistrationWindowState.Open,
            window.GetComputedState(ClosesAtUtc.AddTicks(-1)));
        Assert.Equal(
            RegistrationWindowState.Closed,
            window.GetComputedState(ClosesAtUtc));
        Assert.Throws<ArgumentException>(() => window.GetComputedState(
            DateTime.SpecifyKind(OpensAtUtc, DateTimeKind.Local)));
    }

    [Theory]
    [InlineData(RegistrationWindowLifecycleState.Draft)]
    [InlineData(RegistrationWindowLifecycleState.EmergencyClosed)]
    [InlineData(RegistrationWindowLifecycleState.Superseded)]
    public void Non_published_window_fails_closed(
        RegistrationWindowLifecycleState lifecycleState)
    {
        var window = Create(lifecycleState: lifecycleState);

        Assert.Equal(
            RegistrationWindowState.Closed,
            window.GetComputedState(OpensAtUtc.AddHours(1)));
    }

    [Fact]
    public void Lifecycle_commands_allow_only_the_owned_transitions()
    {
        var published = Create();
        published.Publish();
        Assert.Equal(RegistrationWindowLifecycleState.Published, published.State);
        Assert.Throws<InvalidOperationException>(published.Publish);

        published.EmergencyClose();
        Assert.Equal(RegistrationWindowLifecycleState.EmergencyClosed, published.State);
        Assert.Throws<InvalidOperationException>(published.Supersede);

        var superseded = Create(lifecycleState: RegistrationWindowLifecycleState.Published);
        superseded.Supersede();
        Assert.Equal(RegistrationWindowLifecycleState.Superseded, superseded.State);

        Assert.Throws<InvalidOperationException>(() => Create().EmergencyClose());
        Assert.Throws<InvalidOperationException>(() => Create().Supersede());
    }

    [Fact]
    public void Published_interval_scope_identity_and_rowversion_are_not_publicly_mutable()
    {
        AssertPrivateSetter(nameof(RegistrationWindow.Id));
        AssertPrivateSetter(nameof(RegistrationWindow.TermId));
        AssertPrivateSetter(nameof(RegistrationWindow.ScopeType));
        AssertPrivateSetter(nameof(RegistrationWindow.ScopeValue));
        AssertPrivateSetter(nameof(RegistrationWindow.OpensAtUtc));
        AssertPrivateSetter(nameof(RegistrationWindow.ClosesAtUtc));
        AssertPrivateSetter(nameof(RegistrationWindow.Version));
    }

    private static RegistrationWindow Create(
        Guid? id = null,
        Guid? termId = null,
        RegistrationWindowScopeType scopeType = RegistrationWindowScopeType.AllStudents,
        string? scopeValue = null,
        DateTime? opensAtUtc = null,
        DateTime? closesAtUtc = null,
        RegistrationWindowLifecycleState lifecycleState =
            RegistrationWindowLifecycleState.Draft) =>
        new(
            id ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            scopeType,
            scopeValue,
            opensAtUtc ?? OpensAtUtc,
            closesAtUtc ?? ClosesAtUtc,
            lifecycleState);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(RegistrationWindow).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
