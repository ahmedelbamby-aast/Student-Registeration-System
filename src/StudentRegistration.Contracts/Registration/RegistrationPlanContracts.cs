using StudentRegistration.Contracts;

namespace StudentRegistration.Contracts.Registration;

public sealed record LoadPolicyReasonDto(
    string Code,
    bool Blocking,
    string Message,
    string? RequiredValue,
    string? CurrentValue,
    Guid PolicySetId,
    string PolicyVersion,
    string SourceReference);

public sealed record ResolutionActionDto(
    string Action,
    Guid TargetGroupId,
    string Label,
    string Route);

public sealed record ScheduleConflictParticipantDto(
    Guid GroupId,
    string GroupCode,
    string CourseCode,
    string SubjectTitle,
    TimeOnly StartLocal,
    TimeOnly EndLocal);

public sealed record ScheduleConflictDto(
    string Code,
    ScheduleConflictParticipantDto First,
    ScheduleConflictParticipantDto Second,
    int DayOfWeek,
    TimeOnly OverlapStartLocal,
    TimeOnly OverlapEndLocal,
    string Message,
    IReadOnlyList<ResolutionActionDto> Actions);

public sealed record PlanSelectionIssueDto(
    string Code,
    Guid OfferingId,
    Guid GroupId,
    string GroupCode,
    string Message,
    bool Blocking,
    IReadOnlyList<ResolutionActionDto> Actions);

public sealed record ValidationSnapshotDto(
    DateTime EvaluatedAtUtc,
    string AcademicContextVersion,
    string PolicyVersion,
    string CatalogueVersion,
    IReadOnlyDictionary<string, string> OfferingVersions,
    IReadOnlyDictionary<string, string> GroupVersions);

public sealed record RegistrationPlanMeetingDto(
    Guid MeetingId,
    string ActivityKind,
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string RoomCode,
    string Location,
    string? LecturerName,
    IReadOnlyList<string> TeachingAssistantNames,
    string Timezone);

public sealed record RegistrationPlanGroupDto(
    Guid GroupId,
    Guid OfferingId,
    string CourseCode,
    string SubjectTitle,
    string GroupCode,
    decimal Credits,
    int Capacity,
    int EnrolledCount,
    string RowVersion,
    IReadOnlyList<RegistrationPlanMeetingDto> Meetings);

public sealed record RegistrationPlanDto(
    Guid Id,
    Guid TermId,
    string RowVersion,
    IReadOnlyList<RegistrationPlanGroupDto> SelectedGroups,
    decimal TotalCredits,
    decimal DefaultTargetCredits,
    decimal MaximumAllowedCredits,
    IReadOnlyList<LoadPolicyReasonDto> LoadReasons,
    IReadOnlyList<PlanSelectionIssueDto> SelectionIssues,
    IReadOnlyList<ScheduleConflictDto> Conflicts,
    ValidationSnapshotDto Validation,
    bool ReviewBlocked);

public sealed record RegistrationPlanMutationRequest(
    string ExpectedPlanRowVersion,
    IReadOnlyList<Guid> SelectedGroupIds);

public sealed record StaleRegistrationPlanResponse(
    ApiError Error,
    RegistrationPlanDto CurrentPlan);
