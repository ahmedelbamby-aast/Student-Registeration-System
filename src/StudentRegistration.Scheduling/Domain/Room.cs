namespace StudentRegistration.Scheduling.Domain;

public enum RoomAvailabilityState
{
    Available = 1,
    Unavailable = 2,
}

public sealed class Room
{
    private Room()
    {
    }

    public Room(
        Guid id,
        string code,
        string location,
        int capacity,
        RoomAvailabilityState availabilityState)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Defined(availabilityState, nameof(availabilityState));
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        Id = id;
        Code = SchedulingDomainValue.Code(code, nameof(code));
        Location = SchedulingDomainValue.Required(location, nameof(location));
        Capacity = capacity;
        AvailabilityState = availabilityState;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public int Capacity { get; private set; }

    public RoomAvailabilityState AvailabilityState { get; private set; }

    public byte[] Version { get; private set; } = [];

    public void MarkUnavailable() => AvailabilityState = RoomAvailabilityState.Unavailable;

    public void MarkAvailable() => AvailabilityState = RoomAvailabilityState.Available;

    public void Update(string code, string location, int capacity, RoomAvailabilityState state)
    {
        SchedulingDomainValue.Defined(state, nameof(state));
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        Code = SchedulingDomainValue.Code(code, nameof(code));
        Location = SchedulingDomainValue.Required(location, nameof(location));
        Capacity = capacity;
        AvailabilityState = state;
    }
}
