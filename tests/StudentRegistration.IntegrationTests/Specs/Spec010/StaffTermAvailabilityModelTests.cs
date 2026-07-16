using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class StaffTermAvailabilityModelTests
{
    private static readonly DateTime DeadlineUtc =
        new(2026, 8, 31, 21, 59, 59, DateTimeKind.Utc);

    [Fact]
    public void Aggregate_preserves_the_unique_staff_term_key_complete_ranges_and_version()
    {
        var id = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var range = Range(id);

        var availability = new StaffTermAvailability(
            id,
            staffId,
            termId,
            DeadlineUtc,
            [range]);

        Assert.Equal(id, availability.Id);
        Assert.Equal(staffId, availability.StaffId);
        Assert.Equal(termId, availability.TermId);
        Assert.Equal(DeadlineUtc, availability.DeadlineUtc);
        Assert.Equal([range], availability.Ranges);
        Assert.Empty(availability.Version);
        AssertPrivateSetter(nameof(StaffTermAvailability.StaffId));
        AssertPrivateSetter(nameof(StaffTermAvailability.TermId));
        AssertPrivateSetter(nameof(StaffTermAvailability.Version));
    }

    [Fact]
    public void Replacement_is_complete_parent_scoped_and_blocked_after_deadline()
    {
        var availability = Create();
        var replacement = new StaffAvailability(
            Guid.NewGuid(),
            availability.Id,
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(14, 0),
            AvailabilityKind.Unavailable);

        availability.ReplaceRanges(
            [replacement],
            new DateTime(2026, 8, 31, 20, 0, 0, DateTimeKind.Utc));
        Assert.Equal([replacement], availability.Ranges);

        Assert.Throws<InvalidOperationException>(() => availability.ReplaceRanges(
            [replacement],
            DeadlineUtc.AddTicks(1)));
        Assert.Throws<ArgumentException>(() => availability.ReplaceRanges(
            [Range(Guid.NewGuid())],
            DeadlineUtc.AddHours(-1)));
    }

    [Fact]
    public void Aggregate_rejects_missing_identity_non_utc_deadline_and_empty_range_set()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(staffId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(
            deadlineUtc: DateTime.SpecifyKind(DeadlineUtc, DateTimeKind.Unspecified)));
        Assert.Throws<ArgumentException>(() => Create(ranges: []));
    }

    private static StaffTermAvailability Create(
        Guid? id = null,
        Guid? staffId = null,
        Guid? termId = null,
        DateTime? deadlineUtc = null,
        IReadOnlyList<StaffAvailability>? ranges = null)
    {
        var aggregateId = id ?? Guid.NewGuid();
        return new StaffTermAvailability(
            aggregateId,
            staffId ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            deadlineUtc ?? DeadlineUtc,
            ranges ?? [Range(aggregateId)]);
    }

    private static StaffAvailability Range(Guid aggregateId) =>
        new(
            Guid.NewGuid(),
            aggregateId,
            DayOfWeek.Sunday,
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            AvailabilityKind.Available);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(StaffTermAvailability).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
