namespace StudentRegistration.Scheduling.Application.Ports;

public interface IEligibilityOfferingReader
{
    Task<IReadOnlyList<EligibilityOfferingSnapshot>> ListForTermAsync(
        Guid termId,
        CancellationToken cancellationToken = default);

    Task<EligibilityOfferingSnapshot?> FindAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default);
}

public sealed record EligibilityOfferingSnapshot(
    Guid OfferingId,
    Guid TermId,
    Guid CourseId,
    string State,
    byte[] RowVersion,
    IReadOnlyList<EligibilityGroupSnapshot> Groups);

public sealed record EligibilityGroupSnapshot(
    Guid GroupId,
    string GroupCode,
    string State,
    bool RegistrationPaused,
    int Capacity,
    int EnrolledCount,
    byte[] RowVersion,
    IReadOnlyList<EligibilityMeetingSnapshot> Meetings);

public sealed record EligibilityMeetingSnapshot(
    Guid MeetingId,
    string Activity,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string RoomCode,
    string Location,
    bool RoomAvailable,
    IReadOnlyList<EligibilityMeetingStaffSnapshot> Staff);

public sealed record EligibilityMeetingStaffSnapshot(
    Guid StaffId,
    string Role,
    string Name);
