using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Specs.Spec011.EdgeCases;

public sealed class EC_3Tests
{
    [Fact]
    public async Task Sql_metacharacters_are_literal_search_text_not_control_syntax()
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await Spec011ServiceFactory.Search(fixture).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new("DS413%' OR 1=1 --", "all", null, null, "all", null, 1, 20));

        Assert.NotNull(result.Page);
        Assert.Empty(result.Page.Items);
        Assert.Equal(0, result.Page.TotalCount);
    }
}
