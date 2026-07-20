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

public sealed record DiscoveryOutcomeHeadersMetadata(
    IReadOnlyList<string> HeaderNames);

public static class Spec011Endpoints
{
    private const string CatalogueReadAvailable = "Catalogue.ReadAvailable";
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    private static readonly string[] OutcomeHeaderNames =
    [
        "X-Eligibility-Policy-Version",
        "X-Eligibility-Reason-Codes",
        "X-Registration-Window-State",
        "X-Support-Reference-Path",
    ];

    private static readonly HashSet<string> ListQueryKeys =
        new(
            [
                "q",
                "eligibility",
                "credits",
                "day",
                "availability",
                "sort",
                "page",
                "pageSize",
            ],
            StringComparer.OrdinalIgnoreCase);

    public static IEndpointRouteBuilder MapSpec011Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        Standard(
                endpoints.MapGet(
                        "/api/student/terms/{termId}/offerings",
                        GetOfferings)
                    .RequireAuthorization(CatalogueReadAvailable)
                    .WithMetadata(
                        new DiscoveryOutcomeHeadersMetadata(
                            Array.AsReadOnly(OutcomeHeaderNames)))
                    .Produces<Page<OfferingEligibilityDto>>(
                        StatusCodes.Status200OK),
                includesBadRequest: true)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        Standard(
                endpoints.MapGet(
                        "/api/student/offerings/{offeringId}/eligibility",
                        GetOfferingEligibility)
                    .RequireAuthorization(CatalogueReadAvailable)
                    .Produces<OfferingEligibilityDto>(StatusCodes.Status200OK))
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static RouteHandlerBuilder Standard(
        RouteHandlerBuilder builder,
        bool includesBadRequest = false)
    {
        if (includesBadRequest)
        {
            builder.Produces<ApiError>(StatusCodes.Status400BadRequest);
        }

        return builder
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetOfferings(
        [FromRoute] Guid termId,
        [FromQuery(Name = "q")] string? q,
        [FromQuery] string? eligibility,
        [FromQuery] decimal? credits,
        [FromQuery] int? day,
        [FromQuery] string? availability,
        [FromQuery] string? sort,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        HttpContext context,
        [FromServices] OfferingSearchQuery search,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Error(
                context,
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required.");
        }

        if (!HasOnlySingleKnownQueryValues(context.Request.Query))
        {
            return ValidationError(context);
        }

        var result = await search.SearchAsync(
            applicationUserId,
            termId,
            new OfferingSearchRequest(
                q,
                eligibility,
                credits,
                day,
                availability,
                sort,
                page ?? DefaultPage,
                pageSize ?? DefaultPageSize),
            cancellationToken);

        return result.Outcome switch
        {
            OfferingSearchOutcome.Found when result.Page is not null
                && result.Metadata is not null =>
                DiscoveryPage(context, result.Page, result.Metadata),
            OfferingSearchOutcome.PageSizeInvalid => Error(
                context,
                StatusCodes.Status400BadRequest,
                "PAGE_SIZE_INVALID",
                "Page and page-size values are outside the supported range."),
            OfferingSearchOutcome.ValidationError => ValidationError(context),
            OfferingSearchOutcome.ContextNotFound => Error(
                context,
                StatusCodes.Status404NotFound,
                "REGISTRATION_CONTEXT_NOT_FOUND",
                "The requested registration context was not found."),
            OfferingSearchOutcome.DependencyUnavailable => DiscoveryUnavailable(
                context),
            _ => InternalError(context),
        };
    }

    private static async Task<IResult> GetOfferingEligibility(
        [FromRoute] Guid offeringId,
        HttpContext context,
        [FromServices] EligibilityService eligibility,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context, out var applicationUserId))
        {
            return Error(
                context,
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required.");
        }

        var result = await eligibility.EvaluateOfferingAsync(
            applicationUserId,
            offeringId,
            cancellationToken);

        return result.Outcome switch
        {
            EligibilityEvaluationOutcome.Found when result.Items.Count == 1 =>
                Results.Ok(ToDto(result.Items[0])),
            EligibilityEvaluationOutcome.ContextNotFound
                or EligibilityEvaluationOutcome.OfferingNotFound => Error(
                    context,
                    StatusCodes.Status404NotFound,
                    "OFFERING_NOT_FOUND_OR_OUTSIDE_CONTEXT",
                    "The offering was not found in the authorized registration context."),
            EligibilityEvaluationOutcome.DependencyUnavailable =>
                DiscoveryUnavailable(context),
            _ => InternalError(context),
        };
    }

    private static IResult DiscoveryPage(
        HttpContext context,
        Page<OfferingEligibility> page,
        DiscoveryMetadata metadata)
    {
        context.Response.Headers["X-Eligibility-Policy-Version"] =
            metadata.PolicyVersion;
        context.Response.Headers["X-Eligibility-Reason-Codes"] =
            string.Join(",", metadata.ReasonCodes);
        context.Response.Headers["X-Registration-Window-State"] =
            metadata.RegistrationWindowState;
        context.Response.Headers["X-Support-Reference-Path"] =
            metadata.SupportReferencePath;

        return Results.Ok(
            new Page<OfferingEligibilityDto>(
                page.Items.Select(ToDto).ToArray(),
                page.PageNumber,
                page.PageSize,
                page.TotalCount,
                page.Sort));
    }

    private static OfferingEligibilityDto ToDto(OfferingEligibility item) =>
        new(
            item.OfferingId,
            item.CourseCode,
            item.Title,
            item.Credits,
            item.CurrentPlanCredits,
            item.ProjectedPlanCredits,
            item.DefaultTargetCredits,
            item.MaximumAllowedCredits,
            item.Eligible,
            item.Reasons.Select(ToDto).ToArray(),
            item.Groups.Select(ToDto).ToArray(),
            item.InputSummary,
            item.EvaluatedAtUtc,
            item.AcademicContextVersion,
            item.CatalogueVersion,
            item.PolicySetId,
            item.PolicyVersion,
            item.OfferingRowVersion,
            item.CurrentPlanVersion);

    private static EligibilityReasonDto ToDto(EligibilityReason reason) =>
        new(
            reason.Code,
            reason.Passed,
            reason.Blocking,
            reason.Message,
            reason.RequiredValue,
            reason.CurrentValue,
            reason.PolicySetId,
            reason.PolicyVersion,
            reason.SourceReference,
            reason.SourceAccessedOn,
            reason.ApprovedBy,
            reason.EffectiveFromUtc,
            reason.EffectiveToUtc,
            reason.OverridePossible,
            reason.SupportReferencePath);

    private static GroupSummaryDto ToDto(GroupSummary group) =>
        new(
            group.GroupId,
            group.GroupCode,
            group.State,
            group.Selectable,
            group.Capacity,
            group.EnrolledCount,
            group.SeatsRemaining,
            group.NonSelectableReasons.Select(reason =>
                new GroupNonSelectableReasonDto(
                    reason.Code,
                    reason.Message)).ToArray(),
            group.Meetings.Select(meeting =>
                new GroupMeetingDto(
                    meeting.MeetingId,
                    meeting.Activity,
                    (int)meeting.DayOfWeek,
                    meeting.StartLocal,
                    meeting.EndLocal,
                    meeting.RoomCode,
                    meeting.Location,
                    meeting.Staff.Select(staff =>
                        new GroupMeetingStaffDto(
                            staff.Role,
                            staff.Name)).ToArray())).ToArray(),
            group.RowVersion,
            group.HeldSeatCount);

    private static bool HasOnlySingleKnownQueryValues(IQueryCollection query) =>
        query.All(pair =>
            ListQueryKeys.Contains(pair.Key)
            && pair.Value.Count == 1);

    private static bool TryApplicationUserId(
        HttpContext context,
        out Guid applicationUserId) =>
        Guid.TryParse(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out applicationUserId)
        && applicationUserId != Guid.Empty;

    private static IResult ValidationError(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            "The query parameters are invalid.");

    private static IResult DiscoveryUnavailable(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status503ServiceUnavailable,
            "DISCOVERY_UNAVAILABLE",
            "Subject discovery is temporarily unavailable.");

    private static IResult InternalError(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status500InternalServerError,
            "INTERNAL_ERROR",
            "The request could not be completed.");

    private static IResult Error(
        HttpContext context,
        int status,
        string code,
        string message) =>
        Results.Json(
            new ApiError(code, message, CorrelationId(context)),
            statusCode: status);

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
