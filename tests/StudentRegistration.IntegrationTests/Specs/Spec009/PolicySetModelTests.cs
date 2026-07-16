using StudentRegistration.Academics.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class PolicySetModelTests
{
    private static readonly DateTime EffectiveFromUtc =
        new(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Policy_set_preserves_typed_scope_effective_dates_and_lifecycle()
    {
        var termId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var policy = new PolicySet(
            Guid.NewGuid(),
            "DEMO-POC-2026.1",
            termId,
            programId,
            "AI-DS",
            EffectiveFromUtc,
            null,
            PolicySetState.Draft);

        Assert.Equal(termId, policy.TermId);
        Assert.Equal(programId, policy.ProgramId);
        Assert.Equal("AI-DS", policy.ScopeCode);
        Assert.Equal(PolicySetState.Draft, policy.State);
        Assert.Empty(policy.VersionToken);

        policy.MarkValidated();
        policy.Publish();
        Assert.Equal(PolicySetState.Published, policy.State);
        Assert.Throws<InvalidOperationException>(policy.MarkValidated);
        policy.Supersede();
        Assert.Equal(PolicySetState.Superseded, policy.State);
    }

    [Fact]
    public void Policy_set_rejects_invalid_scope_dates_and_state()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(versionCode: " "));
        Assert.Throws<ArgumentException>(() => Create(scopeCode: " "));
        Assert.Throws<ArgumentException>(() => Create(
            effectiveFromUtc: DateTime.SpecifyKind(EffectiveFromUtc, DateTimeKind.Local)));
        Assert.Throws<ArgumentException>(() => Create(effectiveToUtc: EffectiveFromUtc));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(state: (PolicySetState)999));
    }

    private static PolicySet Create(
        Guid? id = null,
        string versionCode = "DEMO-POC-2026.1",
        Guid? termId = null,
        string scopeCode = "AI-DS",
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        PolicySetState state = PolicySetState.Draft) =>
        new(
            id ?? Guid.NewGuid(),
            versionCode,
            termId ?? Guid.NewGuid(),
            null,
            scopeCode,
            effectiveFromUtc ?? EffectiveFromUtc,
            effectiveToUtc,
            state);
}
