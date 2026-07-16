using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Overnight_meeting_slot_is_rejected_in_the_mvp()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new MeetingSlot(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ActivityType.Lecture,
                DayOfWeek.Monday,
                new TimeOnly(23, 0),
                new TimeOnly(1, 0)));

        Assert.Contains(
            "cannot cross midnight",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }
}
