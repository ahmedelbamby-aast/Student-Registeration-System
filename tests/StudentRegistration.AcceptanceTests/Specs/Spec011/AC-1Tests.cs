using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class AC_1Tests
{
    [Fact]
    public async Task Eligible_three_credit_offering_projects_fifteen_to_eighteen_with_complete_groups()
    {
        var fixture = new Spec011ScenarioBuilder
        {
            CurrentPlanCredits = 15m,
            CourseCredits = 3m
        };

        var result = await Spec011AcceptanceSupport.Service(fixture)
            .EvaluateTermAsync(fixture.ApplicationUserId, fixture.TermId);

        var offering = Assert.Single(result.Items);
        Assert.True(offering.Eligible);
        Assert.Equal(15m, offering.CurrentPlanCredits);
        Assert.Equal(18m, offering.ProjectedPlanCredits);
        Assert.Equal(18m, offering.DefaultTargetCredits);
        Assert.Equal(18m, offering.MaximumAllowedCredits);
        var group = Assert.Single(offering.Groups);
        Assert.True(group.Selectable);
        Assert.Contains(group.Meetings, meeting =>
            meeting.Activity == "Lecture"
            && meeting.Staff.Any(staff => staff.Role == "Lecturer"));
        Assert.Contains(group.Meetings, meeting =>
            meeting.Activity == "Tutorial"
            && meeting.Staff.Any(staff => staff.Role == "TeachingAssistant"));
        Assert.All(group.Meetings, meeting =>
        {
            Assert.False(string.IsNullOrWhiteSpace(meeting.RoomCode));
            Assert.False(string.IsNullOrWhiteSpace(meeting.Location));
            Assert.True(meeting.EndLocal > meeting.StartLocal);
        });
    }
}
