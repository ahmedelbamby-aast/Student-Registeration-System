using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class StaffAvailabilityModelTests
{
    [Theory]
    [InlineData(AvailabilityKind.Available)]
    [InlineData(AvailabilityKind.Unavailable)]
    public void Range_is_parent_owned_valid_and_has_no_independent_version(
        AvailabilityKind kind)
    {
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var range = new StaffAvailability(
            id,
            parentId,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            kind);

        Assert.Equal(id, range.Id);
        Assert.Equal(parentId, range.StaffTermAvailabilityId);
        Assert.Equal(DayOfWeek.Monday, range.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), range.StartLocal);
        Assert.Equal(new TimeOnly(12, 0), range.EndLocal);
        Assert.Equal(kind, range.Kind);
        AssertPrivateSetter(nameof(StaffAvailability.StaffTermAvailabilityId));
        Assert.Null(typeof(StaffAvailability).GetProperty("Version"));
    }

    [Fact]
    public void Range_rejects_missing_identity_invalid_day_kind_and_incomplete_time_range()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(
            () => Create(staffTermAvailabilityId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(dayOfWeek: (DayOfWeek)999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(kind: (AvailabilityKind)999));
        Assert.Throws<ArgumentException>(
            () => Create(startLocal: new TimeOnly(9, 0), endLocal: new TimeOnly(9, 0)));
        Assert.Throws<ArgumentException>(
            () => Create(startLocal: new TimeOnly(13, 0), endLocal: new TimeOnly(12, 0)));
    }

    private static StaffAvailability Create(
        Guid? id = null,
        Guid? staffTermAvailabilityId = null,
        DayOfWeek dayOfWeek = DayOfWeek.Monday,
        TimeOnly? startLocal = null,
        TimeOnly? endLocal = null,
        AvailabilityKind kind = AvailabilityKind.Available) =>
        new(
            id ?? Guid.NewGuid(),
            staffTermAvailabilityId ?? Guid.NewGuid(),
            dayOfWeek,
            startLocal ?? new TimeOnly(9, 0),
            endLocal ?? new TimeOnly(12, 0),
            kind);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(StaffAvailability).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
