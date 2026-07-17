using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec013;

public sealed class OptimizerConfigurationModelTests
{
    private static readonly ScoreFactor[] CanonicalFactorOrder =
    [
        ScoreFactor.PreferenceViolations,
        ScoreFactor.IdleMinutes,
        ScoreFactor.TeachingDays,
        ScoreFactor.StableGroupTuple
    ];

    [Fact]
    public void Model_is_owned_by_the_registration_domain()
    {
        Assert.Equal(
            typeof(RegistrationPlan).Assembly,
            typeof(OptimizerConfiguration).Assembly);
        Assert.Equal(
            "StudentRegistration.Registration.Domain",
            typeof(OptimizerConfiguration).Namespace);
    }

    [Fact]
    public void Configuration_preserves_the_approved_lexicographic_order_immutably()
    {
        var factorOrder = CanonicalFactorOrder.ToList();

        var configuration = new OptimizerConfiguration(
            "1.0.0",
            factorOrder,
            "SPEC-013-GATE-A-2026-07-13");
        factorOrder.Reverse();

        Assert.Equal("1.0.0", configuration.Version);
        Assert.Equal(CanonicalFactorOrder, configuration.FactorOrder);
        Assert.Equal(
            "SPEC-013-GATE-A-2026-07-13",
            configuration.ApprovalReference);

        var mutableView = Assert.IsAssignableFrom<IList<ScoreFactor>>(
            configuration.FactorOrder);
        Assert.Throws<NotSupportedException>(() =>
            mutableView.Add(ScoreFactor.PreferenceViolations));
    }

    [Fact]
    public void Configuration_rejects_invalid_version_approval_or_factor_order()
    {
        Assert.Throws<ArgumentException>(() => Create(version: "1.0"));
        Assert.Throws<ArgumentException>(() => Create(version: "v1.0.0"));
        Assert.Throws<ArgumentException>(() => Create(approvalReference: " "));
        Assert.Throws<ArgumentException>(() => Create(
            factorOrder:
            [
                ScoreFactor.IdleMinutes,
                ScoreFactor.PreferenceViolations,
                ScoreFactor.TeachingDays,
                ScoreFactor.StableGroupTuple
            ]));
        Assert.Throws<ArgumentException>(() => Create(
            factorOrder:
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                ScoreFactor.TeachingDays
            ]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            factorOrder:
            [
                ScoreFactor.PreferenceViolations,
                ScoreFactor.IdleMinutes,
                ScoreFactor.TeachingDays,
                (ScoreFactor)999
            ]));
    }

    [Fact]
    public void Json_contract_round_trips_version_order_and_approval_reference()
    {
        var configuration = Create();

        var json = JsonSerializer.Serialize(
            configuration,
            JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["approvalReference", "factorOrder", "version"],
            root.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            [
                "preference-violations",
                "idle-minutes",
                "teaching-days",
                "stable-group-tuple"
            ],
            root.GetProperty("factorOrder")
                .EnumerateArray()
                .Select(value => value.GetString()));

        var roundTrip = JsonSerializer.Deserialize<OptimizerConfiguration>(
            json,
            JsonSerializerOptions.Web);
        Assert.Equal(configuration, roundTrip);
    }

    private static OptimizerConfiguration Create(
        string version = "1.0.0",
        IReadOnlyList<ScoreFactor>? factorOrder = null,
        string approvalReference = "SPEC-013-GATE-A-2026-07-13") =>
        new(
            version,
            factorOrder ?? CanonicalFactorOrder,
            approvalReference);
}
