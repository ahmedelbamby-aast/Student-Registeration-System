using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class AC_4Tests
{
    [Fact]
    public async Task Oversized_pages_fail_and_client_values_cannot_escalate_eligibility()
    {
        var fixture = new Spec011ScenarioBuilder { BlockingHold = true };
        var query = Spec011AcceptanceSupport.Search(fixture);

        var oversized = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new("Project", "all", 3m, 1, "all", "courseCode,id", 1, 101));
        var bypass = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new("Project", "client-eligible", 3m, 1, "available", "courseCode,id", 1, 20));
        var all = await query.SearchAsync(
            fixture.ApplicationUserId,
            fixture.TermId,
            new("Project", "all", 3m, 1, "all", "courseCode,id", 1, 20));

        Assert.Equal(OfferingSearchOutcome.PageSizeInvalid, oversized.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", oversized.ErrorCode);
        Assert.Equal(OfferingSearchOutcome.ValidationError, bypass.Outcome);
        var offering = Assert.Single(all.Page!.Items);
        Assert.False(offering.Eligible);
        Assert.Contains(offering.Reasons, reason => reason.Code == "REGISTRATION_HOLD");
        Assert.Equal("courseCode,id", all.Page.Sort);
    }
}
