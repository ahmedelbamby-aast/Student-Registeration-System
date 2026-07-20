namespace StudentRegistration.Contracts.Registration;

public sealed record EligibilityReasonDto(
    string Code,
    bool Passed,
    bool Blocking,
    string Message,
    string? RequiredValue,
    string? CurrentValue,
    Guid PolicySetId,
    string PolicyVersion,
    string SourceReference,
    DateOnly SourceAccessedOn,
    string ApprovedBy,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    bool OverridePossible,
    string? SupportReferencePath);

public sealed record GroupNonSelectableReasonDto(
    string Code,
    string Message);

public sealed record GroupMeetingStaffDto(
    string Role,
    string Name);

public sealed record GroupMeetingDto(
    Guid MeetingId,
    string Activity,
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal,
    string RoomCode,
    string Location,
    IReadOnlyList<GroupMeetingStaffDto> Staff);

public sealed record GroupSummaryDto(
    Guid GroupId,
    string GroupCode,
    string State,
    bool Selectable,
    int Capacity,
    int EnrolledCount,
    int SeatsRemaining,
    IReadOnlyList<GroupNonSelectableReasonDto> NonSelectableReasons,
    IReadOnlyList<GroupMeetingDto> Meetings,
    string RowVersion,
    int HeldSeatCount = 0);

public sealed record OfferingEligibilityDto(
    Guid OfferingId,
    string CourseCode,
    string Title,
    decimal Credits,
    decimal CurrentPlanCredits,
    decimal ProjectedPlanCredits,
    decimal DefaultTargetCredits,
    decimal MaximumAllowedCredits,
    bool Eligible,
    IReadOnlyList<EligibilityReasonDto> Reasons,
    IReadOnlyList<GroupSummaryDto> Groups,
    IReadOnlyDictionary<string, string> InputSummary,
    DateTime EvaluatedAtUtc,
    string AcademicContextVersion,
    string CatalogueVersion,
    Guid PolicySetId,
    string PolicyVersion,
    string OfferingRowVersion,
    string CurrentPlanVersion);
