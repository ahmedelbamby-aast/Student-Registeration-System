using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class SectionGroupModelTests
{
    [Fact]
    public void Registration_consumes_the_scheduling_owned_section_group()
    {
        Assert.Equal(
            "StudentRegistration.Scheduling.Domain",
            typeof(SectionGroup).Namespace);
        Assert.NotEqual(typeof(RegistrationPlan).Assembly, typeof(SectionGroup).Assembly);

        var group = Create(capacity: 2, enrolledCount: 1);

        Assert.True(group.IsSelectable);
        Assert.Empty(group.Version);
        group.AllocateSeat();
        Assert.Equal(2, group.EnrolledCount);
        Assert.False(group.IsSelectable);
    }

    [Fact]
    public void Consumed_group_fails_closed_when_paused_full_or_not_published()
    {
        Assert.False(Create(registrationPaused: true).IsSelectable);
        Assert.False(Create(capacity: 2, enrolledCount: 2).IsSelectable);
        Assert.False(Create(state: SectionGroupState.Draft).IsSelectable);
        Assert.Throws<InvalidOperationException>(() =>
            Create(registrationPaused: true).AllocateSeat());
    }

    private static SectionGroup Create(
        int capacity = 30,
        int enrolledCount = 0,
        SectionGroupState state = SectionGroupState.Published,
        bool registrationPaused = false) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "LEC-A",
            capacity,
            enrolledCount,
            state,
            registrationPaused);
}
