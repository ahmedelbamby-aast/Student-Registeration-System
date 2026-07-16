using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class RoomModelTests
{
    [Fact]
    public void Room_normalizes_code_and_preserves_location_capacity_availability_and_version()
    {
        var room = new Room(
            Guid.NewGuid(),
            " c-201 ",
            "C Building, floor 2",
            capacity: 35,
            RoomAvailabilityState.Available);

        Assert.Equal("C-201", room.Code);
        Assert.Equal("C Building, floor 2", room.Location);
        Assert.Equal(35, room.Capacity);
        Assert.Equal(RoomAvailabilityState.Available, room.AvailabilityState);
        Assert.Empty(room.Version);
        AssertPrivateSetter(nameof(Room.Code));
        AssertPrivateSetter(nameof(Room.Version));
    }

    [Fact]
    public void Room_availability_has_an_explicit_bounded_transition()
    {
        var room = Create();

        room.MarkUnavailable();
        Assert.Equal(RoomAvailabilityState.Unavailable, room.AvailabilityState);

        room.MarkAvailable();
        Assert.Equal(RoomAvailabilityState.Available, room.AvailabilityState);
    }

    [Fact]
    public void Room_rejects_missing_identity_text_negative_capacity_and_unknown_state()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(code: " "));
        Assert.Throws<ArgumentException>(() => Create(location: " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(capacity: -1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(state: (RoomAvailabilityState)999));
    }

    private static Room Create(
        Guid? id = null,
        string code = "C-201",
        string location = "C Building, floor 2",
        int capacity = 35,
        RoomAvailabilityState state = RoomAvailabilityState.Available) =>
        new(id ?? Guid.NewGuid(), code, location, capacity, state);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(Room).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
