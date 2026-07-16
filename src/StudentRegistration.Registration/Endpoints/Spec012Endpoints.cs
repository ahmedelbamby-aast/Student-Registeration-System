using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.Registration.Endpoints;

public static class Spec012Endpoints
{
    private const string StudentPolicy = "Student";
    private const int MaximumSelectedGroups = 100;

    public static IEndpointRouteBuilder MapSpec012Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        Standard(
                endpoints.MapGet(
                        "/api/student/terms/{termId}/registration-plan",
                        GetPlan)
                    .RequireAuthorization(StudentPolicy)
                    .Produces<RegistrationPlanDto>(StatusCodes.Status200OK))
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        Standard(
                endpoints.MapPut(
                        "/api/student/terms/{termId}/registration-plan",
                        ReplacePlan)
                    .RequireAuthorization(StudentPolicy)
                    .Produces<RegistrationPlanDto>(StatusCodes.Status200OK))
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<StaleRegistrationPlanResponse>(StatusCodes.Status409Conflict);

        Standard(
                endpoints.MapPost(
                        "/api/student/terms/{termId}/registration-plan/validate",
                        ValidatePlan)
                    .RequireAuthorization(StudentPolicy)
                    .Produces<RegistrationPlanDto>(StatusCodes.Status200OK))
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static RouteHandlerBuilder Standard(RouteHandlerBuilder builder) =>
        builder
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

    private static async Task<IResult> GetPlan(
        [FromRoute] Guid termId,
        HttpContext context,
        [FromServices] RegistrationPlanService service,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Unauthorized(context);
        }

        return Result(
            context,
            await service.GetAsync(
                applicationUserId,
                termId,
                cancellationToken));
    }

    private static async Task<IResult> ReplacePlan(
        [FromRoute] Guid termId,
        [FromBody] RegistrationPlanMutationRequest request,
        HttpContext context,
        [FromServices] RegistrationPlanService service,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Unauthorized(context);
        }

        if (request is null ||
            string.IsNullOrWhiteSpace(request.ExpectedPlanRowVersion) ||
            request.SelectedGroupIds is null ||
            request.SelectedGroupIds.Count > MaximumSelectedGroups)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "The complete plan replacement is invalid.");
        }

        return Result(
            context,
            await service.ReplaceAsync(
                applicationUserId,
                termId,
                new(
                    request.ExpectedPlanRowVersion,
                    request.SelectedGroupIds),
                cancellationToken));
    }

    private static async Task<IResult> ValidatePlan(
        [FromRoute] Guid termId,
        HttpContext context,
        [FromServices] RegistrationPlanService service,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Unauthorized(context);
        }

        return Result(
            context,
            await service.ValidateAsync(
                applicationUserId,
                termId,
                cancellationToken));
    }

    private static IResult Result(
        HttpContext context,
        RegistrationPlanOperationResult result)
    {
        if (result.Outcome is RegistrationPlanOperationOutcome.StaleVersion &&
            result.Plan is not null)
        {
            var current = ToDto(result.Plan);
            return Results.Json(
                new StaleRegistrationPlanResponse(
                    new ApiError(
                        "STALE_VERSION",
                        "The registration plan changed in another editor.",
                        CorrelationId(context),
                        currentVersion: current.RowVersion),
                    current),
                statusCode: StatusCodes.Status409Conflict);
        }

        if (result.Plan is not null &&
            result.Outcome is RegistrationPlanOperationOutcome.Found
                or RegistrationPlanOperationOutcome.Updated
                or RegistrationPlanOperationOutcome.Validated)
        {
            return Results.Ok(ToDto(result.Plan));
        }

        return result.Outcome switch
        {
            RegistrationPlanOperationOutcome.InvalidSelection => Error(
                context,
                result.ErrorCode is "PLAN_REPLACEMENT_INVALID"
                    or "DUPLICATE_GROUP_SELECTION"
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status409Conflict,
                result.ErrorCode ?? "VALIDATION_ERROR",
                "The requested plan selection is invalid."),
            RegistrationPlanOperationOutcome.NotFound => Error(
                context,
                StatusCodes.Status404NotFound,
                "REGISTRATION_CONTEXT_NOT_FOUND",
                "The requested registration context was not found."),
            RegistrationPlanOperationOutcome.DependencyUnavailable => Error(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "REGISTRATION_PLAN_UNAVAILABLE",
                "The registration plan is temporarily unavailable."),
            _ => Error(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "The request could not be completed.")
        };
    }

    private static RegistrationPlanDto ToDto(RegistrationPlanView plan) =>
        new(
            plan.Id,
            plan.TermId,
            plan.RowVersion,
            plan.SelectedGroups.Select(ToDto).ToArray(),
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
            plan.SelectionIssues.Select(issue => new PlanSelectionIssueDto(
                issue.Code,
                issue.OfferingId,
                issue.GroupId,
                issue.GroupCode,
                issue.Message,
                true,
                issue.Actions.Select(ToDto).ToArray())).ToArray(),
            plan.Conflicts.Select(ToDto).ToArray(),
            new ValidationSnapshotDto(
                plan.Validation.EvaluatedAtUtc,
                plan.Validation.AcademicContextVersion,
                plan.Validation.PolicyVersion,
                plan.Validation.CatalogueVersion,
                plan.Validation.OfferingVersions.ToDictionary(
                    pair => pair.Key.ToString("D"),
                    pair => pair.Value,
                    StringComparer.Ordinal),
                plan.Validation.GroupVersions.ToDictionary(
                    pair => pair.Key.ToString("D"),
                    pair => pair.Value,
                    StringComparer.Ordinal)),
            plan.ReviewBlocked);

    private static RegistrationPlanGroupDto ToDto(
        RegistrationPlanSelectedGroup group) =>
        new(
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
                meeting.Timezone)).ToArray());

    private static ScheduleConflictDto ToDto(ScheduleConflict conflict) =>
        new(
            conflict.Code,
            ToDto(conflict.First),
            ToDto(conflict.Second),
            (int)conflict.DayOfWeek,
            conflict.OverlapStartLocal,
            conflict.OverlapEndLocal,
            conflict.Message,
            conflict.Actions.Select(ToDto).ToArray());

    private static ScheduleConflictParticipantDto ToDto(
        ScheduleConflictParticipant participant) =>
        new(
            participant.GroupId,
            participant.GroupCode,
            participant.CourseCode,
            participant.SubjectTitle,
            participant.StartLocal,
            participant.EndLocal);

    private static ResolutionActionDto ToDto(
        RegistrationPlanResolutionAction action) =>
        new(action.Action, action.TargetGroupId, action.Label, action.Route);

    private static ResolutionActionDto ToDto(
        ScheduleConflictAction action) =>
        new(action.Action, action.TargetGroupId, action.Label, action.Route);

    private static bool TryApplicationUserId(
        HttpContext context,
        out Guid applicationUserId) =>
        Guid.TryParse(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out applicationUserId)
        && applicationUserId != Guid.Empty;

    private static IResult Unauthorized(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status401Unauthorized,
            "UNAUTHORIZED",
            "Authentication is required.");

    private static IResult Error(
        HttpContext context,
        int statusCode,
        string code,
        string message) =>
        Results.Json(
            new ApiError(code, message, CorrelationId(context)),
            statusCode: statusCode);

    private static string CorrelationId(HttpContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            return context.TraceIdentifier;
        }

        context.TraceIdentifier = Guid.NewGuid().ToString("N");
        return context.TraceIdentifier;
    }
}
