using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_7Tests
{
    [Fact]
    public async Task Read_reason_timezone_and_bounded_page_gates_are_explicit()
    {
        var cairoStore = new OfferingStoreFake();
        var cairoService = new OfferingService(cairoStore);
        var cairo = await cairoService.GetStudentDetailAsync(
            cairoStore.Snapshot.Id);

        var londonMeeting = Spec010Scenario.Meeting(
            Spec010Scenario.Id(5),
            StudentRegistration.Scheduling.Domain.ActivityType.Lecture,
            StudentRegistration.Scheduling.Domain.TeachingRole.Lecturer,
            "Dr Lecturer",
            "L-101",
            "London Campus",
            DayOfWeek.Sunday,
            new(7, 0),
            new(8, 0));
        var londonStore = new OfferingStoreFake
        {
            Snapshot = new(
                Spec010Scenario.Id(2),
                "published",
                [
                    new(
                        Spec010Scenario.Id(14),
                        "G02",
                        30,
                        0,
                        false,
                        "published",
                        [1],
                        [londonMeeting])
                ])
        };
        var london = await new OfferingService(londonStore)
            .GetStudentDetailAsync(londonStore.Snapshot.Id);

        var invalidStore = new PublicationStoreFake
        {
            Snapshot = Spec010Scenario.Publication(
                bundleComplete: false,
                staffAvailable: false)
        };
        var validation = await new OfferingPublicationValidator().ValidateAsync(
            Spec010Scenario.ValidateCommand(invalidStore.Snapshot),
            invalidStore);
        var list = await cairoService.ListAdminOfferingsAsync(
            new AdminOfferingQuery(
                TermId: null,
                State: "published",
                Query: "csc",
                Page: 2,
                PageSize: 100,
                Sort: null));
        var oversized = await cairoService.ListAdminOfferingsAsync(
            new AdminOfferingQuery(
                null,
                null,
                null,
                Page: 1,
                PageSize: 101,
                Sort: null));

        Assert.Equal(new TimeOnly(9, 0),
            Assert.Single(cairo!.Groups).Meetings[0].StartLocal);
        Assert.Equal(new TimeOnly(7, 0),
            Assert.Single(london!.Groups).Meetings[0].StartLocal);
        Assert.Contains(
            "MISSING_TUTORIAL_OR_LABORATORY",
            validation.ReasonCodes);
        Assert.Contains("STAFF_UNAVAILABLE", validation.ReasonCodes);
        Assert.NotEqual(OfferingOutcome.ValidationError, list.Outcome);
        Assert.Equal(2, list.Page!.Page);
        Assert.Equal(100, list.Page.PageSize);
        Assert.Equal("courseCode,id", list.Page.Sort);
        Assert.Equal(OfferingOutcome.ValidationError, oversized.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", oversized.ErrorCode);
    }
}
