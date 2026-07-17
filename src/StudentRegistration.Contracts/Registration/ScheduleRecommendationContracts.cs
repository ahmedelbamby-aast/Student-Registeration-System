using StudentRegistration.Contracts.Scheduling;

namespace StudentRegistration.Contracts.Registration;

public sealed record MeetingIntervalDto(
    int DayOfWeek,
    TimeOnly StartLocal,
    TimeOnly EndLocal);

public sealed record SchedulePreferencesDto(
    IReadOnlyList<int> AvoidedWeekdays,
    TimeOnly? EarliestPreferredStartLocal,
    TimeOnly? LatestPreferredEndLocal);

public sealed record ScheduleScoreComponentDto(
    string Factor,
    string Value,
    string Message);

public sealed record ScheduleOptionDto(
    string OptionToken,
    int Rank,
    IReadOnlyList<GroupDto> Groups,
    IReadOnlyList<ScheduleScoreComponentDto> ScoreExplanation);

public sealed record OptimizationDiagnosticMemberDto(
    Guid CourseId,
    Guid? GroupId,
    MeetingIntervalDto? Interval,
    string Action);

public sealed record OptimizationDiagnosticDto(
    string DiagnosticId,
    string ReasonCode,
    IReadOnlyList<OptimizationDiagnosticMemberDto> Members,
    MeetingIntervalDto? ConflictingInterval,
    string Minimality);

public sealed record OptimizationResultDto(
    string RequestCorrelationId,
    Guid PlanId,
    string PlanRowVersion,
    string AcademicContextVersion,
    string CatalogueVersion,
    Guid PolicySetId,
    string PolicyVersion,
    IReadOnlyDictionary<string, string> OfferingVersions,
    IReadOnlyDictionary<string, string> GroupVersions,
    string OptimizerConfigurationVersion,
    string Status,
    IReadOnlyList<ScheduleOptionDto> Options,
    IReadOnlyList<OptimizationDiagnosticDto> Diagnostics,
    DateTime EvaluatedAtUtc);

public sealed record RecommendScheduleRequest(
    string ExpectedPlanRowVersion,
    string RequestCorrelationId,
    SchedulePreferencesDto Preferences);

public sealed record ApplyScheduleOptionRequest(
    string OptionToken,
    string ExpectedPlanRowVersion,
    string RequestCorrelationId);
