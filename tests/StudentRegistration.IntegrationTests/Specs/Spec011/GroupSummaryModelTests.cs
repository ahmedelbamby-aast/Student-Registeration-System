using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec011;

public sealed class GroupSummaryModelTests
{
    [Fact]
    public void Full_is_capacity_status_not_lifecycle_and_staff_stays_nested()
    {
        var model = new GroupSummary(
            Guid.NewGuid(),
            "G1",
            "published",
            false,
            30,
            30,
            [new GroupNonSelectableReason("GROUP_FULL", "No seats remain.")],
            [
                new GroupMeetingSummary(
                    Guid.NewGuid(),
                    "Lecture",
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(10, 0),
                    "R101",
                    "Main",
                    [new GroupMeetingStaffSummary("Lecturer", "Dr. Ada")])
            ],
            "AQID");

        Assert.Equal("published", model.State);
        Assert.Equal(0, model.SeatsRemaining);
        Assert.Equal("GROUP_FULL", Assert.Single(model.NonSelectableReasons).Code);
        Assert.Equal("Dr. Ada", Assert.Single(Assert.Single(model.Meetings).Staff).Name);
        Assert.Equal("AQID", model.RowVersion);
    }

    [Fact]
    public void Capacity_invariants_are_enforced()
    {
        Assert.Throws<ArgumentException>(() => new GroupSummary(
            Guid.NewGuid(), "G1", "published", true, 10, 11, [], [], "version"));
    }
}
