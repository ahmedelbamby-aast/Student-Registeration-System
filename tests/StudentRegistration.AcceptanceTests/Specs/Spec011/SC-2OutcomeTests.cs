using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class SC_2OutcomeTests
{
    [Fact]
    public async Task Discovery_remains_bounded_and_relevant_with_a_large_candidate_set()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            MinimumGpa = null,
            MinimumEarnedCredits = null
        };
        var offerings = Enumerable.Range(1, 150)
            .Select(index => fixture.OfferingSnapshot(
                offeringId: Guid.Parse($"01100000-0000-0000-0001-{index:000000000000}"),
                groupId: Guid.Parse($"01100000-0000-0000-0002-{index:000000000000}")))
            .ToArray();

        var page = (await Spec011AcceptanceSupport.Search(fixture, offerings)
            .SearchAsync(
                fixture.ApplicationUserId,
                fixture.TermId,
                new("DS413", "eligible", 3m, 1, "available", "courseCode,id", 1, 100)))
            .Page!;

        Assert.Equal(100, page.Items.Count);
        Assert.Equal(150, page.TotalCount);
        Assert.All(page.Items, item => Assert.Equal("DS413", item.CourseCode));
        Assert.Equal(
            page.Items.Select(item => item.OfferingId).OrderBy(id => id),
            page.Items.Select(item => item.OfferingId));
    }
}
