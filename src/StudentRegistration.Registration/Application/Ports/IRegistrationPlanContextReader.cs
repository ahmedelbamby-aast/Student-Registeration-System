namespace StudentRegistration.Registration.Application.Ports;

public sealed record RegistrationPlanMeetingSnapshot(
    Guid MeetingId,
    DayOfWeek DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string ActivityKind,
    string RoomCode,
    string Location,
    string? LecturerName,
    IReadOnlyList<string> TeachingAssistantNames,
    string Timezone);

public sealed record RegistrationPlanGroupSnapshot(
    Guid OfferingId,
    Guid GroupId,
    string GroupCode,
    string CourseCode,
    string SubjectTitle,
    decimal Credits,
    string State,
    bool RegistrationPaused,
    int Capacity,
    int EnrolledCount,
    string OfferingVersion,
    string GroupVersion,
    IReadOnlyList<RegistrationPlanMeetingSnapshot> Meetings);

public sealed record RegistrationPlanContextSnapshot(
    string AcademicContextVersion,
    Guid PolicySetId,
    string PolicyVersion,
    string PolicySourceReference,
    string CatalogueVersion,
    IReadOnlyList<RegistrationPlanGroupSnapshot> Groups,
    string CatalogueScopeCode = "",
    string PolicyScopeCode = "");

public interface IRegistrationPlanContextReader
{
    Task<RegistrationPlanContextSnapshot?> ReadAsync(
        Guid studentId,
        Guid termId,
        IReadOnlyCollection<Guid> groupIds,
        DateTime evaluatedAtUtc,
        CancellationToken cancellationToken = default);
}
