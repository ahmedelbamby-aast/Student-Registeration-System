using System.Reflection;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013.EdgeCases;

public sealed class EC_4Tests
{
    private static readonly ScoreFactor[] ApprovedFactorOrder =
    [
        ScoreFactor.PreferenceViolations,
        ScoreFactor.IdleMinutes,
        ScoreFactor.TeachingDays,
        ScoreFactor.StableGroupTuple
    ];

    [Fact]
    public void Invalid_preferences_and_configuration_leave_the_approved_version_active()
    {
        var approved = CreateConfiguration();
        var state = new ApprovedConfigurationState(approved);

        Assert.Throws<ArgumentException>(() => new SchedulePreferences(
            earliestPreferredStartLocal: new TimeOnly(17, 0),
            latestPreferredEndLocal: new TimeOnly(8, 0)));
        Assert.False(state.TryActivate(() => CreateConfiguration(
            version: "version-one")));
        Assert.False(state.TryActivate(() => CreateConfiguration(
            factorOrder:
            [
                ScoreFactor.IdleMinutes,
                ScoreFactor.PreferenceViolations,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ])));

        Assert.Same(approved, state.Active);
        Assert.Equal("1.0.0", state.Active.Version);
        Assert.Equal(ApprovedFactorOrder, state.Active.FactorOrder);
    }

    [Fact]
    public void Production_scorer_consumes_the_validated_optimizer_configuration()
    {
        var scorer = Assembly.Load("StudentRegistration.Registration").GetType(
            "StudentRegistration.Registration.Domain.ScheduleScorer");

        Assert.True(
            scorer is not null,
            "T043 must provide ScheduleScorer before EC-4 can pass against production code.");
        Assert.True(
            scorer!.GetConstructors().Any(constructor =>
                constructor.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(OptimizerConfiguration))) ||
            scorer.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Any(method => method.GetParameters().Any(parameter =>
                    parameter.ParameterType == typeof(OptimizerConfiguration))),
            "ScheduleScorer must consume the validated immutable OptimizerConfiguration.");
    }

    private static OptimizerConfiguration CreateConfiguration(
        string version = "1.0.0",
        IReadOnlyList<ScoreFactor>? factorOrder = null) =>
        new(
            version,
            factorOrder ?? ApprovedFactorOrder,
            "SPEC-013-GATE-A-2026-07-13");

    private sealed class ApprovedConfigurationState(
        OptimizerConfiguration active)
    {
        public OptimizerConfiguration Active { get; private set; } = active;

        public bool TryActivate(Func<OptimizerConfiguration> candidateFactory)
        {
            try
            {
                Active = candidateFactory();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
