using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.RateLimiting;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Endpoints;

public static class Spec013Endpoints
{
    private const string StudentPolicy = "Student";
    private const string RecommendationRateLimitPolicy =
        "registration-schedule-recommendations";
    private static readonly TimeSpan ComputationBudget =
        TimeSpan.FromMilliseconds(450);

    public static IEndpointRouteBuilder MapSpec013Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        Standard(
                endpoints.MapPost(
                        "/api/student/terms/{termId}/registration-plan/recommendations",
                        Recommend)
                    .RequireAuthorization(StudentPolicy)
                    .RequireRateLimiting(RecommendationRateLimitPolicy)
                    .Produces<OptimizationResultDto>(StatusCodes.Status200OK))
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict);

        Standard(
                endpoints.MapPut(
                        "/api/student/terms/{termId}/registration-plan/recommended-option",
                        Apply)
                    .RequireAuthorization(StudentPolicy)
                    .RequireRateLimiting(RecommendationRateLimitPolicy)
                    .Produces<RegistrationPlanDto>(StatusCodes.Status200OK))
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static RouteHandlerBuilder Standard(RouteHandlerBuilder builder) =>
        builder
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

    private static async Task<IResult> Recommend(
        [FromRoute] Guid termId,
        [FromBody] RecommendScheduleRequest request,
        HttpContext context,
        [FromServices] IRegistrationPlanOwnerReader ownerReader,
        [FromServices] IRecommendationSnapshotReader snapshotReader,
        [FromServices] OptimizationCoordinator coordinator,
        [FromServices] RecommendationApplicationService applicationService,
        [FromServices] TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Error(context, 401, "UNAUTHORIZED", "Authentication is required.");
        }

        if (!Valid(request))
        {
            return Error(
                context,
                400,
                "VALIDATION_ERROR",
                "The recommendation request is invalid.");
        }

        var studentId = await ownerReader.ResolveStudentIdAsync(
            applicationUserId,
            termId,
            cancellationToken);
        if (!studentId.HasValue)
        {
            return Error(
                context,
                404,
                "REGISTRATION_CONTEXT_NOT_FOUND",
                "The requested registration context was not found.");
        }

        var captured = await snapshotReader.ReadAsync(
            studentId.Value,
            termId,
            request.ExpectedPlanRowVersion,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);
        if (captured.Outcome is not RecommendationSnapshotOutcome.Captured ||
            captured.Snapshot is null)
        {
            return captured.Outcome switch
            {
                RecommendationSnapshotOutcome.NotFound => Error(
                    context, 404, "REGISTRATION_CONTEXT_NOT_FOUND",
                    "The requested registration context was not found."),
                RecommendationSnapshotOutcome.PlanChanged => Error(
                    context, 409, "PLAN_CHANGED",
                    "The registration plan changed."),
                RecommendationSnapshotOutcome.StaleInput => Error(
                    context, 409, "STALE_INPUT",
                    "Schedule inputs changed during capture."),
                _ => Error(
                    context, 503, "RECOMMENDATIONS_UNAVAILABLE",
                    "Schedule recommendations are temporarily unavailable.")
            };
        }

        var snapshot = captured.Snapshot;
        var preferences = new SchedulePreferences(
            request.Preferences.AvoidedWeekdays.Select(day => (DayOfWeek)day)
                .ToArray(),
            request.Preferences.EarliestPreferredStartLocal,
            request.Preferences.LatestPreferredEndLocal);
        var optimization = await coordinator.OptimizeAsync(
            snapshot.Candidates,
            preferences,
            snapshot.RequiredCredits,
            snapshot.SelectedCourseIds,
            ComputationBudget,
            cancellationToken);
        var groups = snapshot.Groups.ToDictionary(group => group.GroupId);
        var options = optimization.Options.Select(option =>
        {
            var token = applicationService.IssueOptionToken(
                new(
                    snapshot.StudentId,
                    snapshot.TermId,
                    snapshot.PlanId,
                    snapshot.PlanRowVersion,
                    option.Selections,
                    snapshot.AcademicContextVersion,
                    snapshot.CatalogueVersion,
                    snapshot.PolicySetId,
                    snapshot.PolicyVersion,
                    snapshot.OfferingVersions,
                    snapshot.GroupVersions,
                    "1.0.0",
                    request.RequestCorrelationId));
            return new ScheduleOptionDto(
                token,
                option.Rank,
                option.Selections.Select(selection => ToGroup(groups[selection.GroupId]))
                    .ToArray(),
                option.ScoreExplanation.Select(component =>
                    new ScheduleScoreComponentDto(
                        Factor(component.Factor),
                        component.Value,
                        component.Message))
                    .ToArray());
        }).ToArray();

        return Results.Ok(
            new OptimizationResultDto(
                request.RequestCorrelationId,
                snapshot.PlanId,
                snapshot.PlanRowVersion,
                snapshot.AcademicContextVersion,
                snapshot.CatalogueVersion,
                snapshot.PolicySetId,
                snapshot.PolicyVersion,
                snapshot.OfferingVersions,
                snapshot.GroupVersions,
                "1.0.0",
                Status(optimization.Status),
                options,
                optimization.Diagnostics.Select(ToDiagnostic).ToArray(),
                timeProvider.GetUtcNow().UtcDateTime));
    }

    private static async Task<IResult> Apply(
        [FromRoute] Guid termId,
        [FromBody] ApplyScheduleOptionRequest request,
        HttpContext context,
        [FromServices] IRegistrationPlanOwnerReader ownerReader,
        [FromServices] RecommendationApplicationService applicationService,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Error(context, 401, "UNAUTHORIZED", "Authentication is required.");
        }

        if (request is null ||
            string.IsNullOrWhiteSpace(request.OptionToken) ||
            string.IsNullOrWhiteSpace(request.ExpectedPlanRowVersion) ||
            string.IsNullOrWhiteSpace(request.RequestCorrelationId))
        {
            return Error(
                context, 400, "INVALID_OPTION_TOKEN",
                "The protected schedule option is invalid.");
        }

        var studentId = await ownerReader.ResolveStudentIdAsync(
            applicationUserId,
            termId,
            cancellationToken);
        if (!studentId.HasValue)
        {
            return Error(
                context, 404, "REGISTRATION_CONTEXT_NOT_FOUND",
                "The requested registration context was not found.");
        }

        var result = await applicationService.ApplyAsync(
            studentId.Value,
            termId,
            request,
            cancellationToken);
        if (result.Outcome is RecommendationApplyOutcome.Updated &&
            result.Plan is not null)
        {
            return Results.Ok(ToPlan(result.Plan));
        }

        return result.Outcome switch
        {
            RecommendationApplyOutcome.InvalidOptionToken => Error(
                context, 400, "INVALID_OPTION_TOKEN",
                "The protected schedule option is invalid."),
            RecommendationApplyOutcome.ContextNotFound => Error(
                context, 404, "REGISTRATION_CONTEXT_NOT_FOUND",
                "The requested registration context was not found."),
            RecommendationApplyOutcome.OptionExpired => Error(
                context, 409, "OPTION_EXPIRED",
                "The schedule option expired."),
            RecommendationApplyOutcome.PlanChanged => Error(
                context, 409, "PLAN_CHANGED",
                "The registration plan changed."),
            RecommendationApplyOutcome.StaleInput => Error(
                context, 409, "STALE_INPUT",
                "Schedule inputs changed."),
            _ => Error(
                context, 503, "RECOMMENDATIONS_UNAVAILABLE",
                "Schedule recommendations are temporarily unavailable.")
        };
    }

    private static bool Valid(object? value) =>
        value is RecommendScheduleRequest request &&
        !string.IsNullOrWhiteSpace(request.ExpectedPlanRowVersion) &&
        !string.IsNullOrWhiteSpace(request.RequestCorrelationId) &&
        request.RequestCorrelationId.Length <= 128 &&
        request.Preferences is not null &&
        request.Preferences.AvoidedWeekdays.Count <= 7 &&
        request.Preferences.AvoidedWeekdays.Distinct().Count() ==
            request.Preferences.AvoidedWeekdays.Count &&
        request.Preferences.AvoidedWeekdays.All(day => day is >= 0 and <= 6) &&
        (request.Preferences.EarliestPreferredStartLocal is null ||
         request.Preferences.LatestPreferredEndLocal is null ||
         request.Preferences.EarliestPreferredStartLocal <=
            request.Preferences.LatestPreferredEndLocal);

    private static GroupDto ToGroup(RegistrationPlanGroupSnapshot group) =>
        new(
            group.GroupId,
            group.OfferingId,
            group.GroupCode,
            group.Capacity,
            group.EnrolledCount,
            group.RegistrationPaused,
            group.State,
            group.State == "published" &&
                !group.RegistrationPaused &&
                group.EnrolledCount < group.Capacity,
            [],
            [],
            group.Meetings.Select(meeting => new MeetingDto(
                meeting.MeetingId,
                meeting.ActivityKind,
                meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal,
                Guid.Empty,
                meeting.RoomCode,
                meeting.Location)).ToArray(),
            group.GroupVersion);

    private static OptimizationDiagnosticDto ToDiagnostic(
        OptimizationDiagnostic diagnostic) =>
        new(
            diagnostic.DiagnosticId,
            diagnostic.ReasonCode,
            diagnostic.Members.Select(member =>
                new OptimizationDiagnosticMemberDto(
                    member.CourseId,
                    member.GroupId,
                    member.Interval is null
                        ? null
                        : new(
                            (int)member.Interval.DayOfWeek,
                            member.Interval.StartLocal,
                            member.Interval.EndLocal),
                    member.Action)).ToArray(),
            diagnostic.ConflictingInterval is null
                ? null
                : new(
                    (int)diagnostic.ConflictingInterval.DayOfWeek,
                    diagnostic.ConflictingInterval.StartLocal,
                    diagnostic.ConflictingInterval.EndLocal),
            diagnostic.Minimality);

    private static string Factor(ScoreFactor factor) => factor switch
    {
        ScoreFactor.PreferenceViolations => "preference-violations",
        ScoreFactor.IdleMinutes => "idle-minutes",
        ScoreFactor.TeachingDays => "teaching-days",
        ScoreFactor.StableGroupTuple => "stable-group-tuple",
        _ => throw new InvalidOperationException("Unknown score factor.")
    };

    private static string Status(OptimizationStatus status) => status switch
    {
        OptimizationStatus.Complete => "complete",
        OptimizationStatus.NoSolution => "no-solution",
        OptimizationStatus.TimeBudget => "time-budget",
        _ => throw new InvalidOperationException("Unknown optimizer status.")
    };

    private static RegistrationPlanDto ToPlan(RegistrationPlanView plan) =>
        new(
            plan.Id,
            plan.TermId,
            plan.RowVersion,
            plan.SelectedGroups.Select(group => new RegistrationPlanGroupDto(
                group.GroupId,
                group.OfferingId,
                group.CourseCode,
                group.SubjectTitle,
                group.GroupCode,
                group.Credits,
                group.Capacity,
                group.EnrolledCount,
                group.GroupVersion,
                group.Meetings.Select(meeting => new RegistrationPlanMeetingDto(
                    meeting.MeetingId,
                    meeting.ActivityKind,
                    (int)meeting.DayOfWeek,
                    meeting.StartLocal,
                    meeting.EndLocal,
                    meeting.RoomCode,
                    meeting.Location,
                    meeting.LecturerName,
                    meeting.TeachingAssistantNames,
                    meeting.Timezone)).ToArray(),
                group.HeldSeatCount)).ToArray(),
            plan.TotalCredits,
            plan.DefaultTargetCredits,
            plan.MaximumAllowedCredits,
            plan.LoadReasons.Select(reason => new LoadPolicyReasonDto(
                reason.Code,
                reason.Blocking,
                reason.Message,
                reason.RequiredValue,
                reason.CurrentValue,
                reason.PolicySetId,
                reason.PolicyVersion,
                reason.SourceReference)).ToArray(),
            [],
            [],
            new(
                plan.Validation.EvaluatedAtUtc,
                plan.Validation.AcademicContextVersion,
                plan.Validation.PolicyVersion,
                plan.Validation.CatalogueVersion,
                plan.Validation.OfferingVersions.ToDictionary(
                    pair => pair.Key.ToString("D"),
                    pair => pair.Value),
                plan.Validation.GroupVersions.ToDictionary(
                    pair => pair.Key.ToString("D"),
                    pair => pair.Value)),
            plan.ReviewBlocked);

    private static bool TryApplicationUserId(
        HttpContext context,
        out Guid applicationUserId) =>
        Guid.TryParse(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out applicationUserId) &&
        applicationUserId != Guid.Empty;

    private static IResult Error(
        HttpContext context,
        int status,
        string code,
        string message) =>
        Results.Json(
            new ApiError(code, message, CorrelationId(context)),
            statusCode: status);

    private static string CorrelationId(HttpContext context) =>
        string.IsNullOrWhiteSpace(context.TraceIdentifier)
            ? context.TraceIdentifier = Guid.NewGuid().ToString("N")
            : context.TraceIdentifier;

    // Stable middleware/contract outcomes: RATE_LIMITED and INTERNAL_ERROR.
}
