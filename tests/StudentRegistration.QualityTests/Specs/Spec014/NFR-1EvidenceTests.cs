namespace StudentRegistration.QualityTests.Specs.Spec014;

public sealed class NFR_1EvidenceTests
{
    [Fact]
    public async Task Target_and_spike_profiles_preserve_every_registration_invariant()
    {
        var evidence = await Spec014EvidenceGate.GetAsync();

        AssertProfileHasNoInvariantViolations(evidence.Target);
        AssertProfileHasNoInvariantViolations(evidence.Spike);
        Assert.Equal(0, evidence.Collision.Invariants.TotalViolations);
    }

    private static void AssertProfileHasNoInvariantViolations(
        Spec014ProfileEvidence profile)
    {
        Assert.Equal(0, profile.Invariants.OverbookedGroups);
        Assert.Equal(0, profile.Invariants.DuplicateActiveOfferingEnrollments);
        Assert.Equal(0, profile.Invariants.PartialScheduleCommits);
        Assert.Equal(0, profile.Invariants.CombinedPolicyOrTimetableViolations);
        Assert.Equal(0, profile.Invariants.TotalViolations);
    }
}

internal static class Spec014EvidenceGate
{
    private static readonly Lazy<Task<Spec014LoadEvidence>> SharedEvidence =
        new(LoadOrRunAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public static Task<Spec014LoadEvidence> GetAsync() => SharedEvidence.Value;

    private static async Task<Spec014LoadEvidence> LoadOrRunAsync()
    {
        if (string.Equals(
                Environment.GetEnvironmentVariable(
                    Spec014RegistrationLoadHarness.RunEnvironmentVariable),
                "1",
                StringComparison.Ordinal))
        {
            var measured = await Spec014RegistrationLoadHarness.RunExactProfilesAsync();
            Spec014RegistrationLoadHarness.ValidateReleaseEvidence(measured);
            await Spec014RegistrationLoadHarness.WriteLocalArtifactAsync(measured);
            await Spec014RegistrationLoadHarness.WriteCheckedInArtifactAsync(measured);
            return measured;
        }

        var recorded = Spec014LoadEvidence.ReadCheckedIn();
        Assert.Equal(
            Spec014RegistrationLoadHarness.CalculateSourceFingerprint(),
            recorded.SourceFingerprint);
        return recorded;
    }
}
