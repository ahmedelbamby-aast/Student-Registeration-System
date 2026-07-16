namespace StudentRegistration.Scheduling.Domain;

public sealed class StaffTermAvailability
{
    private IReadOnlyList<StaffAvailability> _ranges = [];

    private StaffTermAvailability()
    {
    }

    public StaffTermAvailability(
        Guid id,
        Guid staffId,
        Guid termId,
        DateTime deadlineUtc,
        IReadOnlyList<StaffAvailability> ranges)
    {
        SchedulingDomainValue.Identifier(id, nameof(id));
        SchedulingDomainValue.Identifier(staffId, nameof(staffId));
        SchedulingDomainValue.Identifier(termId, nameof(termId));
        SchedulingDomainValue.Utc(deadlineUtc, nameof(deadlineUtc));

        Id = id;
        StaffId = staffId;
        TermId = termId;
        DeadlineUtc = deadlineUtc;
        _ranges = ValidateRanges(ranges);
    }

    public Guid Id { get; private set; }

    public Guid StaffId { get; private set; }

    public Guid TermId { get; private set; }

    public DateTime DeadlineUtc { get; private set; }

    public IReadOnlyList<StaffAvailability> Ranges => _ranges;

    public byte[] Version { get; private set; } = [];

    public void ReplaceRanges(
        IReadOnlyList<StaffAvailability> ranges,
        DateTime changedAtUtc)
    {
        SchedulingDomainValue.Utc(changedAtUtc, nameof(changedAtUtc));
        if (changedAtUtc > DeadlineUtc)
        {
            throw new InvalidOperationException("Availability can no longer be changed.");
        }

        _ranges = ValidateRanges(ranges);
    }

    private IReadOnlyList<StaffAvailability> ValidateRanges(
        IReadOnlyList<StaffAvailability> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);
        if (ranges.Count == 0
            || ranges.Any(range => range is null || range.StaffTermAvailabilityId != Id))
        {
            throw new ArgumentException(
                "A complete non-empty range set owned by this aggregate is required.",
                nameof(ranges));
        }

        return Array.AsReadOnly(ranges.ToArray());
    }
}
