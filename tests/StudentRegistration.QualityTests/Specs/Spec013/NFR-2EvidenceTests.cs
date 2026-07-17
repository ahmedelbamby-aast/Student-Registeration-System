using StudentRegistration.Registration.Domain;
using StudentRegistration.TestSupport;
using Xunit.Abstractions;

namespace StudentRegistration.QualityTests.Specs.Spec013;

public sealed class NFR_2EvidenceTests(ITestOutputHelper output)
{
    private const int RepeatCount = 50;

    [Fact]
    public void Same_and_reversed_inputs_produce_exactly_the_same_option_order()
    {
        var candidates = Spec013OptimizerEvidenceFixture.Candidates();
        var optimizer = Spec013OptimizerEvidenceFixture.Optimizer();
        var baseline = optimizer.Optimize(
            candidates,
            Spec013OptimizerEvidenceFixture.Preferences,
            Spec013OptimizerEvidenceFixture.RequiredCredits);
        var expected = OrderedOptionSignatures(baseline);

        Assert.Equal(3, expected.Length);
        Assert.False(baseline.SearchLimitReached);
        for (var repeat = 0; repeat < RepeatCount; repeat++)
        {
            var sameOrder = optimizer.Optimize(
                candidates,
                Spec013OptimizerEvidenceFixture.Preferences,
                Spec013OptimizerEvidenceFixture.RequiredCredits);
            var reversedOrder = optimizer.Optimize(
                candidates.Reverse().ToArray(),
                Spec013OptimizerEvidenceFixture.Preferences,
                Spec013OptimizerEvidenceFixture.RequiredCredits);

            Assert.False(sameOrder.SearchLimitReached);
            Assert.False(reversedOrder.SearchLimitReached);
            Assert.Equal(expected, OrderedOptionSignatures(sameOrder));
            Assert.Equal(expected, OrderedOptionSignatures(reversedOrder));
            Assert.Equal(
                baseline.OrderedCourseIds,
                sameOrder.OrderedCourseIds);
            Assert.Equal(
                baseline.OrderedCourseIds,
                reversedOrder.OrderedCourseIds);
        }

        output.WriteLine(
            "SPEC013_NFR002 repeats={0} comparisons={1} options={2} " +
            "ordering=exact search_limit=false",
            RepeatCount,
            RepeatCount * 2,
            expected.Length);
    }

    [Fact]
    public void Evidence_records_the_exact_determinism_boundary()
    {
        var evidence = NormalizeWhitespace(RepositoryFiles.Read(
            "docs/release-evidence/SPEC-013-NFR-2.md"));

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-013 NFR-2 Deterministic Ordering Evidence",
            "same 8-course by 10-group fixture",
            "50 repetitions",
            "100 comparisons",
            "original candidate order",
            "reversed candidate order",
            "rank",
            "course, offering, and group identifiers",
            "all four score components",
            "ScheduleOptimizer.Optimize",
            "exactly equal",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|PENDING|PARTIAL)\b",
            evidence);
    }

    private static string[] OrderedOptionSignatures(
        ScheduleOptimizationResult result) =>
        result.Options
            .Select(option => string.Join(
                "#",
                option.Rank,
                string.Join(
                    ",",
                    option.Selections.Select(selection =>
                        $"{selection.CourseId:N}/" +
                        $"{selection.OfferingId:N}/" +
                        $"{selection.GroupId:N}")),
                string.Join(
                    ",",
                    option.ScoreExplanation.Select(component =>
                        $"{component.Factor}/" +
                        $"{component.Value}/" +
                        component.Message))))
            .ToArray();

    private static string NormalizeWhitespace(string value) =>
        string.Join(
            " ",
            value.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
