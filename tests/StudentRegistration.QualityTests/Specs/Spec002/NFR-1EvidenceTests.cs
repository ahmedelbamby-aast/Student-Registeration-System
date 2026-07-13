using System.Text.Json;
using StudentRegistration.TestSupport.Spec002;

namespace StudentRegistration.QualityTests.Specs.Spec002;

public sealed class NFR_1EvidenceTests
{
    private const int RepeatCountPerFixture = 25;

    [Fact]
    public void Every_approved_boundary_input_and_policy_version_produce_identical_full_decisions()
    {
        var harness = Spec002PolicyTestHarness.Load();
        var fixtures = harness.BoundaryCases.OrderBy(fixture => fixture.Id).ToArray();
        var expectedIds = Enumerable.Range(1, 19).Select(number => $"PB-{number:00}");

        Assert.Equal(expectedIds, fixtures.Select(fixture => fixture.Id));

        foreach (var fixture in fixtures)
        {
            var baseline = Serialize(harness.Evaluate(fixture.Input));

            for (var iteration = 1; iteration <= RepeatCountPerFixture; iteration++)
            {
                var repeated = Serialize(harness.Evaluate(fixture.Input));
                Assert.True(
                    string.Equals(baseline, repeated, StringComparison.Ordinal),
                    $"{fixture.Id} complete decision differed on repeat {iteration}.");
            }
        }
    }

    private static string Serialize(PolicyDecision decision) =>
        JsonSerializer.Serialize(decision);
}
