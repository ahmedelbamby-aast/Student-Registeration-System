namespace StudentRegistration.Registration.Application.Ports;

public interface ICurrentPlanReader
{
    Task<CurrentPlanSnapshot> ReadAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default);
}

public sealed record CurrentPlanSnapshot(
    decimal Credits,
    IReadOnlyList<CurrentPlanSelectionSnapshot> Selections,
    string Version)
{
    public static CurrentPlanSnapshot Empty { get; } =
        new(
            0m,
            Array.Empty<CurrentPlanSelectionSnapshot>(),
            EmptyCurrentPlanReader.InitialVersion);
}

public sealed record CurrentPlanSelectionSnapshot(
    Guid OfferingId,
    Guid GroupId,
    IReadOnlyList<CurrentPlanMeetingSnapshot> Meetings);

public sealed record CurrentPlanMeetingSnapshot(
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal);

public sealed class EmptyCurrentPlanReader : ICurrentPlanReader
{
    public const string InitialVersion = "initial-empty/1";

    public Task<CurrentPlanSnapshot> ReadAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException(
                "A student identifier is required.",
                nameof(studentId));
        }

        if (termId == Guid.Empty)
        {
            throw new ArgumentException(
                "An academic term identifier is required.",
                nameof(termId));
        }

        return Task.FromResult(CurrentPlanSnapshot.Empty);
    }
}
