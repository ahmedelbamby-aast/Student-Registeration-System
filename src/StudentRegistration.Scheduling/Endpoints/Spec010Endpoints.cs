using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Scheduling;

namespace StudentRegistration.Scheduling.Endpoints;

public static class Spec010Endpoints
{
    private const string OfferingDetailsRead = "OfferingDetailsRead";
    private const string OfferingsManage = "Offerings.Manage";
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;

    public static IEndpointRouteBuilder MapSpec010Endpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        Standard(
            endpoints.MapGet("/api/offerings/{offeringId}", GetOffering)
                .RequireAuthorization(OfferingDetailsRead)
                .Produces<CourseOfferingDto>(StatusCodes.Status200OK),
            includesNotFound: true);

        Standard(
            endpoints.MapGet("/api/groups/{groupId}", GetGroup)
                .RequireAuthorization(OfferingDetailsRead)
                .Produces<GroupDto>(StatusCodes.Status200OK),
            includesNotFound: true);

        Standard(
            endpoints.MapGet("/api/admin/offerings", GetOfferings)
                .RequireAuthorization(OfferingsManage)
                .Produces<Page<CourseOfferingSummaryDto>>(StatusCodes.Status200OK));

        Standard(
            Mutation(endpoints.MapPost("/api/admin/offerings", CreateOffering))
                .Produces<CourseOfferingDto>(StatusCodes.Status201Created),
            includesNotFound: true,
            includesConflict: true);

        Standard(
            Mutation(endpoints.MapPut("/api/admin/groups/{groupId}", UpdateGroup))
                .Produces<GroupDto>(StatusCodes.Status200OK),
            includesNotFound: true,
            includesConflict: true);

        Standard(
            Mutation(
                    endpoints.MapPost(
                        "/api/admin/offerings/{offeringId}/validate",
                        ValidateOffering))
                .Produces<OfferingValidationResult>(StatusCodes.Status200OK),
            includesNotFound: true,
            includesConflict: true);

        Standard(
            Mutation(
                    endpoints.MapPost(
                        "/api/admin/offerings/{offeringId}/publish",
                        PublishOffering))
                .Produces<CourseOfferingDto>(StatusCodes.Status200OK),
            includesNotFound: true,
            includesConflict: true);

        Standard(
            endpoints.MapGet("/api/admin/rooms", GetRooms)
                .RequireAuthorization(OfferingsManage)
                .Produces<Page<RoomDto>>(StatusCodes.Status200OK));

        Standard(
            Mutation(endpoints.MapPost("/api/admin/rooms", CreateRoom))
                .Produces<RoomDto>(StatusCodes.Status201Created),
            includesConflict: true);

        Standard(
            Mutation(endpoints.MapPut("/api/admin/rooms/{roomId}", UpdateRoom))
                .Produces<RoomDto>(StatusCodes.Status200OK),
            includesNotFound: true,
            includesConflict: true);

        Standard(
            endpoints.MapGet(
                    "/api/admin/staff-availability",
                    GetStaffAvailability)
                .RequireAuthorization(OfferingsManage)
                .Produces<Page<StaffTermAvailabilityDto>>(StatusCodes.Status200OK),
            includesNotFound: true);

        Standard(
            endpoints.MapGet(
                    "/api/admin/schedule-impact-alerts",
                    GetScheduleImpactAlerts)
                .RequireAuthorization(OfferingsManage)
                .Produces<Page<ScheduleImpactAlertDto>>(StatusCodes.Status200OK));

        Standard(
            Mutation(
                    endpoints.MapPost(
                        "/api/admin/schedule-impact-alerts/{alertId}/revalidate",
                        RevalidateScheduleImpactAlert))
                .Produces<ScheduleImpactAlertDto>(StatusCodes.Status200OK),
            includesNotFound: true,
            includesConflict: true);

        Standard(
            Mutation(
                    endpoints.MapPost(
                        "/api/admin/schedule-impact-alerts/{alertId}/resolve",
                        ResolveScheduleImpactAlert))
                .Produces<ScheduleImpactAlertDto>(StatusCodes.Status200OK),
            includesNotFound: true,
            includesConflict: true);

        return endpoints;
    }

    private static RouteHandlerBuilder Mutation(RouteHandlerBuilder builder) =>
        builder
            .RequireAuthorization(OfferingsManage)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true));

    private static RouteHandlerBuilder Standard(
        RouteHandlerBuilder builder,
        bool includesNotFound = false,
        bool includesConflict = false)
    {
        builder
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        if (includesNotFound)
        {
            builder.Produces<ApiError>(StatusCodes.Status404NotFound);
        }

        if (includesConflict)
        {
            builder.Produces<ApiError>(StatusCodes.Status409Conflict);
        }

        return builder;
    }

    private static IResult GetOffering(
        [FromRoute] Guid offeringId,
        HttpContext context) =>
        ValidId(offeringId, context) ?? SchedulingUnavailable(context);

    private static IResult GetGroup(
        [FromRoute] Guid groupId,
        HttpContext context) =>
        ValidId(groupId, context) ?? SchedulingUnavailable(context);

    private static IResult GetOfferings(
        [FromQuery] Guid? termId,
        [FromQuery] string? state,
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context) =>
        ValidOptionalId(termId, context)
        ?? ValidList(query, page, pageSize, context)
        ?? SchedulingUnavailable(context);

    private static IResult CreateOffering(
        CreateOfferingRequest request,
        HttpContext context) =>
        ValidId(request.TermId, context)
        ?? ValidId(request.CourseId, context)
        ?? ValidId(request.ClientRequestId, context)
        ?? (request.Groups is null || request.Groups.Count == 0
            ? ValidationError(context)
            : null)
        ?? SchedulingUnavailable(context);

    private static IResult UpdateGroup(
        [FromRoute] Guid groupId,
        UpdateGroupRequest request,
        HttpContext context) =>
        ValidId(groupId, context)
        ?? Required(request.ExpectedOfferingRowVersion, context)
        ?? Required(request.ExpectedGroupRowVersion, context)
        ?? Required(request.GroupCode, context)
        ?? Required(request.Reason, context)
        ?? SchedulingUnavailable(context);

    private static IResult ValidateOffering(
        [FromRoute] Guid offeringId,
        ValidateOfferingRequest request,
        HttpContext context) =>
        ValidId(offeringId, context)
        ?? Required(request.ExpectedOfferingRowVersion, context)
        ?? SchedulingUnavailable(context);

    private static IResult PublishOffering(
        [FromRoute] Guid offeringId,
        PublishOfferingRequest request,
        HttpContext context) =>
        ValidId(offeringId, context)
        ?? ValidId(request.ClientRequestId, context)
        ?? Required(request.ExpectedOfferingRowVersion, context)
        ?? Required(request.PreviewToken, context)
        ?? Required(request.Reason, context)
        ?? SchedulingUnavailable(context);

    private static IResult GetRooms(
        [FromQuery] string? query,
        [FromQuery] string? state,
        [FromQuery] int? minimumCapacity,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context) =>
        (minimumCapacity < 0 ? ValidationError(context) : null)
        ?? ValidList(query, page, pageSize, context)
        ?? SchedulingUnavailable(context);

    private static IResult CreateRoom(
        CreateRoomRequest request,
        HttpContext context) =>
        Required(request.Code, context)
        ?? Required(request.Location, context)
        ?? Required(request.State, context)
        ?? ValidId(request.ClientRequestId, context)
        ?? (request.Capacity < 0 ? ValidationError(context) : null)
        ?? SchedulingUnavailable(context);

    private static IResult UpdateRoom(
        [FromRoute] Guid roomId,
        UpdateRoomRequest request,
        HttpContext context) =>
        ValidId(roomId, context)
        ?? Required(request.ExpectedRowVersion, context)
        ?? Required(request.Code, context)
        ?? Required(request.Location, context)
        ?? Required(request.State, context)
        ?? Required(request.Reason, context)
        ?? (request.Capacity < 0 ? ValidationError(context) : null)
        ?? SchedulingUnavailable(context);

    private static IResult GetStaffAvailability(
        [FromQuery] Guid termId,
        [FromQuery] Guid? staffId,
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context) =>
        ValidId(termId, context)
        ?? ValidOptionalId(staffId, context)
        ?? ValidList(query, page, pageSize, context)
        ?? SchedulingUnavailable(context);

    private static IResult GetScheduleImpactAlerts(
        [FromQuery] string? state,
        [FromQuery] Guid? groupId,
        [FromQuery] string? reasonCode,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        HttpContext context) =>
        ValidOptionalId(groupId, context)
        ?? ValidList(reasonCode, page, pageSize, context)
        ?? SchedulingUnavailable(context);

    private static IResult RevalidateScheduleImpactAlert(
        [FromRoute] Guid alertId,
        RevalidateScheduleImpactAlertRequest request,
        HttpContext context) =>
        ValidId(alertId, context)
        ?? Required(request.ExpectedAlertRowVersion, context)
        ?? Required(request.ExpectedGroupRowVersion, context)
        ?? SchedulingUnavailable(context);

    private static IResult ResolveScheduleImpactAlert(
        [FromRoute] Guid alertId,
        ResolveScheduleImpactAlertRequest request,
        HttpContext context) =>
        ValidId(alertId, context)
        ?? Required(request.ExpectedAlertRowVersion, context)
        ?? Required(request.Reason, context)
        ?? SchedulingUnavailable(context);

    private static IResult? ValidList(
        string? query,
        int? page,
        int? pageSize,
        HttpContext context)
    {
        var requestedPage = page ?? 1;
        var requestedPageSize = pageSize ?? DefaultPageSize;
        if (requestedPage < 1 || requestedPageSize is < 1 or > MaximumPageSize)
        {
            return Error(
                context,
                StatusCodes.Status400BadRequest,
                "PAGE_SIZE_INVALID",
                "Page and page-size values are outside the supported range.");
        }

        var trimmed = query?.Trim();
        return trimmed is not null && trimmed.Length is < 3 or > 50
            ? ValidationError(context)
            : null;
    }

    private static IResult? ValidId(Guid value, HttpContext context) =>
        value == Guid.Empty ? ValidationError(context) : null;

    private static IResult? ValidOptionalId(Guid? value, HttpContext context) =>
        value == Guid.Empty ? ValidationError(context) : null;

    private static IResult? Required(string? value, HttpContext context) =>
        string.IsNullOrWhiteSpace(value) ? ValidationError(context) : null;

    private static IResult ValidationError(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            "The request body or parameters are invalid.");

    private static IResult SchedulingUnavailable(HttpContext context) =>
        Error(
            context,
            StatusCodes.Status503ServiceUnavailable,
            "SCHEDULING_UNAVAILABLE",
            "Scheduling data is temporarily unavailable.");

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
