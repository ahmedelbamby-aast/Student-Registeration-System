using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.IntegrationTests.Registration;

public sealed class OfferingSearchTests
{
    [Fact]
    public async Task Search_normalizes_filters_and_stably_sorts_after_evaluation()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            CourseCode = "DS413",
            CourseTitle = "Project I"
        };
        var service = Service(fixture);
        var query = new OfferingSearchQuery(service);

        var result = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new("  project  ", "eligible", 3m, 1, "available", "title,id", 1, 20));

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        var page = Assert.IsType<StudentRegistration.Contracts.Page<
            StudentRegistration.Registration.Domain.OfferingEligibility>>(result.Page);
        Assert.Single(page.Items);
        Assert.Equal("title,id", page.Sort);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Invalid_page_is_rejected_without_results(int page, int pageSize)
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await new OfferingSearchQuery(Service(fixture)).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, null, null, null, null, null, page, pageSize));

        Assert.Equal(OfferingSearchOutcome.PageSizeInvalid, result.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", result.ErrorCode);
        Assert.Null(result.Page);
    }

    [Theory]
    [InlineData(" ", null, null)]
    [InlineData(null, "client-eligible", null)]
    [InlineData(null, null, "unknown-sort")]
    public async Task Invalid_or_client_authored_values_are_rejected(
        string? text,
        string? eligibility,
        string? sort)
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await new OfferingSearchQuery(Service(fixture)).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(text, eligibility, null, null, null, sort, 1, 20));

        Assert.Equal(OfferingSearchOutcome.ValidationError, result.Outcome);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task Very_large_valid_page_is_empty_without_integer_overflow()
    {
        var fixture = new Spec011ScenarioBuilder();
        var result = await new OfferingSearchQuery(Service(fixture)).SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new(null, "all", null, null, "all", null, int.MaxValue, 100));

        Assert.Equal(OfferingSearchOutcome.Found, result.Outcome);
        Assert.Empty(result.Page!.Items);
        Assert.Equal(1, result.Page.TotalCount);
    }

    private static EligibilityService Service(Spec011ScenarioBuilder fixture) =>
        new(
            fixture.AcademicReader(),
            fixture.OfferingReader(),
            fixture.CurrentPlanReader(),
            new GroupSummaryProjection(),
            new FixedTimeProvider(fixture.EvaluatedAtUtc));

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
