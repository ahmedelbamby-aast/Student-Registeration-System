using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class SectionGroupModelTests
{
    [Fact]
    public void Group_normalizes_code_and_preserves_capacity_state_and_root_version()
    {
        var id = Guid.NewGuid();
        var offeringId = Guid.NewGuid();

        var group = new SectionGroup(
            id,
            offeringId,
            " lec-a ",
            capacity: 30,
            enrolledCount: 12,
            SectionGroupState.Published,
            registrationPaused: false);

        Assert.Equal(id, group.Id);
        Assert.Equal(offeringId, group.OfferingId);
        Assert.Equal("LEC-A", group.GroupCode);
        Assert.Equal(30, group.Capacity);
        Assert.Equal(12, group.EnrolledCount);
        Assert.Equal(0, group.HeldSeatCount);
        Assert.Equal(18, group.AvailableSeatCount);
        Assert.Equal(SectionGroupState.Published, group.State);
        Assert.False(group.RegistrationPaused);
        Assert.True(group.IsSelectable);
        Assert.Empty(group.Version);
        AssertPrivateSetter(nameof(SectionGroup.OfferingId));
        AssertPrivateSetter(nameof(SectionGroup.GroupCode));
        AssertPrivateSetter(nameof(SectionGroup.Version));
    }

    [Fact]
    public void Held_seats_occupy_capacity_and_convert_without_changing_occupied_count()
    {
        var group = Create(capacity: 2, enrolledCount: 0, heldSeatCount: 1);

        group.HoldSeat();
        Assert.Equal(2, group.HeldSeatCount);
        Assert.Equal(0, group.AvailableSeatCount);
        Assert.False(group.IsSelectable);

        group.ConsumeHeldSeat();
        Assert.Equal(1, group.EnrolledCount);
        Assert.Equal(1, group.HeldSeatCount);
        Assert.Equal(2, group.OccupiedSeatCount);

        group.ReleaseHeldSeat();
        Assert.Equal(0, group.HeldSeatCount);
        Assert.Equal(1, group.AvailableSeatCount);
    }

    [Fact]
    public void Group_is_not_selectable_when_full_paused_or_outside_published_state()
    {
        Assert.False(Create(capacity: 30, enrolledCount: 30).IsSelectable);
        Assert.False(Create(registrationPaused: true).IsSelectable);
        Assert.False(Create(state: SectionGroupState.Draft).IsSelectable);
        Assert.False(Create(state: SectionGroupState.Closed).IsSelectable);
        Assert.False(Create(state: SectionGroupState.Cancelled).IsSelectable);
    }

    [Fact]
    public void Group_rejects_invalid_identity_code_capacity_counts_and_state()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(offeringId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(groupCode: " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(capacity: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(enrolledCount: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(heldSeatCount: -1));
        Assert.Throws<ArgumentException>(() => Create(capacity: 10, enrolledCount: 11));
        Assert.Throws<ArgumentException>(() => Create(capacity: 10, enrolledCount: 8, heldSeatCount: 3));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(state: (SectionGroupState)999));
    }

    private static SectionGroup Create(
        Guid? id = null,
        Guid? offeringId = null,
        string groupCode = "LEC-A",
        int capacity = 30,
        int enrolledCount = 0,
        int heldSeatCount = 0,
        SectionGroupState state = SectionGroupState.Published,
        bool registrationPaused = false) =>
        new(
            id ?? Guid.NewGuid(),
            offeringId ?? Guid.NewGuid(),
            groupCode,
            capacity,
            enrolledCount,
            state,
            registrationPaused,
            heldSeatCount);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(SectionGroup).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
