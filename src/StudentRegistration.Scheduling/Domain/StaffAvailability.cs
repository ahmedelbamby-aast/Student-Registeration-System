namespace StudentRegistration.Scheduling.Domain;

public enum AvailabilityKind
{
    Available = 1,
    Unavailable = 2,
}

public sealed class StaffAvailability
{
    private StaffAvailability()
    {
    }

    public StaffAvailability(
        Guid id,
        Guid staffTermAvailabilityId,
        DayOfWeek dayOfWeek,
        TimeOnly startLocal,
        TimeOnly endLocal,
        AvailabilityKind kind)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Identifier(
            staffTermAvailabilityId,
            nameof(staffTermAvailabilityId));
        SchedulingDomainValue.Defined(dayOfWeek, nameof(dayOfWeek));
        SchedulingDomainValue.Defined(kind, nameof(kind));
        if (endLocal <= startLocal)
        {
            throw new ArgumentException(
                "An availability range must end after it starts.",
                nameof(endLocal));
        }

        Id = id;
        StaffTermAvailabilityId = staffTermAvailabilityId;
        DayOfWeek = dayOfWeek;
        StartLocal = startLocal;
        EndLocal = endLocal;
        Kind = kind;
    }

    public Guid Id { get; private set; }

    public Guid StaffTermAvailabilityId { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartLocal { get; private set; }

    public TimeOnly EndLocal { get; private set; }

    public AvailabilityKind Kind { get; private set; }
}
