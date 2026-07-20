using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Scheduling.Application;

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

    private static async Task<IResult> GetOffering(
        [FromRoute] Guid offeringId,
        [FromServices] OfferingService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var invalid = ValidId(offeringId, context);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await service.GetAsync(offeringId, cancellationToken);
        if (result.Outcome is OfferingOutcome.Found
            && result.Offering is { } offering)
        {
            if (IsStudentOnly(context.User) && !IsPublished(offering.State))
            {
                return NotFound(context, "OFFERING_NOT_FOUND");
            }

            return Results.Ok(ToOfferingDto(offering));
        }

        return OfferingError(result, context);
    }

    private static async Task<IResult> GetGroup(
        [FromRoute] Guid groupId,
        [FromServices] OfferingService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var invalid = ValidId(groupId, context);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await service.GetGroupAsync(groupId, cancellationToken);
        if (result.Outcome is OfferingOutcome.Found && result.Group is { } group)
        {
            if (IsStudentOnly(context.User) && !IsPublished(group.State))
            {
                return NotFound(context, "GROUP_NOT_FOUND");
            }

            return Results.Ok(ToGroupDto(group));
        }

        return OfferingError(result, context);
    }

    private static async Task<IResult> GetOfferings(
        [FromQuery] Guid? termId,
        [FromQuery] string? state,
        [FromQuery] string? query,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort,
        [FromServices] OfferingService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var invalid = ValidOptionalId(termId, context)
            ?? ValidList(query, page, pageSize, context);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await service.ListAdminOfferingsAsync(
            new(
                termId,
                state,
                query,
                page ?? 1,
                pageSize ?? DefaultPageSize,
                sort),
            cancellationToken);
        if (result.Outcome is OfferingOutcome.Found && result.Page is { } found)
        {
            return Results.Ok(new Page<CourseOfferingSummaryDto>(
                found.Items.Select(ToOfferingSummaryDto).ToArray(),
                found.Page,
                found.PageSize,
                found.TotalCount,
                found.Sort));
        }

        return Error(
            context,
            StatusCodes.Status400BadRequest,
            result.ErrorCode ?? "VALIDATION_ERROR",
            "The offering query is invalid.");
    }

    private static async Task<IResult> CreateOffering(
        CreateOfferingRequest request,
        [FromServices] OfferingService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var invalid = ValidId(request.TermId, context)
            ?? ValidId(request.CourseId, context)
            ?? ValidId(request.ClientRequestId, context)
            ?? ValidateCreateGroups(request.Groups, context);
        if (invalid is not null)
        {
            return invalid;
        }

        var result = await service.CreateAsync(
            new(
                request.TermId,
                request.CourseId,
                request.Groups.Select(group =>
                    new CreateOfferingGroup(group.GroupCode, group.Capacity)).ToArray(),
                "Create offering"),
            cancellationToken);
        if (result.Outcome is OfferingOutcome.Created && result.Offering is { } offering)
        {
            return Results.Created(
                $"/api/offerings/{offering.Id}",
                ToOfferingDto(offering));
        }

        return OfferingError(result, context);
    }

    private static async Task<IResult> UpdateGroup(
        [FromRoute] Guid groupId,
        UpdateGroupRequest request,
        [FromServices] OfferingService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var invalid = ValidId(groupId, context)
            ?? Required(request.ExpectedOfferingRowVersion, context)
            ?? Required(request.ExpectedGroupRowVersion, context)
            ?? Required(request.GroupCode, context)
            ?? Required(request.Reason, context)
            ?? ValidateUpdateGraph(request, context);
        if (invalid is not null)
        {
            return invalid;
        }

        if (!TryDecodeVersion(request.ExpectedOfferingRowVersion, out var offeringVersion)
            || !TryDecodeVersion(request.ExpectedGroupRowVersion, out var groupVersion))
        {
            return ValidationError(context);
        }

        var meetingIds = request.Meetings.ToDictionary(
            meeting => meeting.RequestMeetingKey,
            meeting => meeting.Id ?? Guid.NewGuid(),
            StringComparer.Ordinal);
        var result = await service.UpdateGroupAsync(
            new(
                groupId,
                offeringVersion,
                groupVersion,
                request.GroupCode,
                request.Capacity,
                request.RegistrationPaused,
                request.Meetings.Select(meeting => new UpdateMeetingInput(
                    meetingIds[meeting.RequestMeetingKey],
                    meeting.RoomId,
                    meeting.ActivityType,
                    (int)meeting.DayOfWeek,
                    meeting.StartLocal,
                    meeting.EndLocal)).ToArray(),
                request.StaffAssignments.Select(assignment =>
                    new UpdateStaffAssignmentInput(
                        meetingIds[assignment.RequestMeetingKey],
                        assignment.StaffId,
                        assignment.Role)).ToArray(),
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.User.Identity?.Name
                    ?? "admin",
                request.Reason),
            cancellationToken);
        if (result.Outcome is OfferingOutcome.Updated && result.Group is { } group)
        {
            return Results.Ok(ToGroupDto(group));
        }

        return OfferingError(result, context);
    }

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

    private static IResult? ValidateCreateGroups(
        IReadOnlyList<CreateGroupInput>? groups,
        HttpContext context)
    {
        if (groups is null || groups.Count == 0
            || groups.Any(group =>
                string.IsNullOrWhiteSpace(group.GroupCode) || group.Capacity < 0))
        {
            return ValidationError(context);
        }

        var codes = groups
            .Select(group => group.GroupCode.Trim().Normalize().ToUpperInvariant())
            .ToArray();
        return codes.Distinct(StringComparer.Ordinal).Count() == codes.Length
            ? null
            : ValidationError(context);
    }

    private static IResult? ValidateUpdateGraph(
        UpdateGroupRequest request,
        HttpContext context)
    {
        if (request.Capacity < 0
            || request.Meetings is null
            || request.StaffAssignments is null)
        {
            return ValidationError(context);
        }

        var meetingKeys = new HashSet<string>(StringComparer.Ordinal);
        var meetingTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var meeting in request.Meetings)
        {
            if (string.IsNullOrWhiteSpace(meeting.RequestMeetingKey)
                || !meetingKeys.Add(meeting.RequestMeetingKey)
                || meeting.Id == Guid.Empty
                || meeting.RoomId == Guid.Empty
                || !Enum.IsDefined(meeting.DayOfWeek)
                || meeting.StartLocal >= meeting.EndLocal
                || meeting.ActivityType is not ("Lecture" or "Tutorial" or "Laboratory"))
            {
                return ValidationError(context);
            }

            meetingTypes[meeting.RequestMeetingKey] = meeting.ActivityType;
        }

        foreach (var assignment in request.StaffAssignments)
        {
            if (assignment.StaffId == Guid.Empty
                || !meetingTypes.TryGetValue(
                    assignment.RequestMeetingKey,
                    out var activityType)
                || !ValidActivityRole(activityType, assignment.Role))
            {
                return ValidationError(context);
            }
        }

        return string.Equals(request.State, "published", StringComparison.OrdinalIgnoreCase)
            ? ValidationError(context)
            : null;
    }

    private static bool ValidActivityRole(string activityType, string role) =>
        activityType switch
        {
            "Lecture" => string.Equals(role, "Lecturer", StringComparison.Ordinal),
            "Tutorial" or "Laboratory" => string.Equals(
                role,
                "TeachingAssistant",
                StringComparison.Ordinal),
            _ => false,
        };

    private static bool TryDecodeVersion(string value, out byte[] version)
    {
        try
        {
            version = Convert.FromBase64String(value);
            return version.Length > 0;
        }
        catch (FormatException)
        {
            version = [];
            return false;
        }
    }

    private static CourseOfferingDto ToOfferingDto(OfferingSnapshot offering) =>
        new(
            offering.Id,
            offering.TermId,
            offering.CourseId,
            offering.CourseCode,
            offering.CourseTitle,
            offering.State,
            offering.Groups.Select(ToGroupDto).ToArray(),
            Convert.ToBase64String(offering.RowVersion));

    private static CourseOfferingSummaryDto ToOfferingSummaryDto(
        OfferingSnapshot offering) =>
        new(
            offering.Id,
            offering.TermId,
            offering.CourseId,
            offering.CourseCode,
            offering.CourseTitle,
            offering.State,
            offering.GroupCount,
            Convert.ToBase64String(offering.RowVersion));

    private static GroupDto ToGroupDto(OfferingGroupSnapshot group)
    {
        var occupied = group.EnrolledCount + group.HeldSeatCount;
        var reasons = new List<ValidationReasonDto>();
        if (!IsPublished(group.State))
        {
            reasons.Add(new(
                "GROUP_NOT_PUBLISHED",
                "The group is not published.",
                []));
        }
        if (group.RegistrationPaused)
        {
            reasons.Add(new(
                "REGISTRATION_PAUSED",
                "Registration is paused for this group.",
                []));
        }
        if (occupied >= group.Capacity)
        {
            reasons.Add(new(
                "GROUP_FULL",
                "The group has no available seats.",
                []));
        }

        return new(
            group.Id,
            group.OfferingId,
            group.GroupCode,
            group.Capacity,
            group.EnrolledCount,
            group.RegistrationPaused,
            group.State,
            reasons.Count == 0,
            reasons,
            group.Meetings.SelectMany(meeting => meeting.Staff.Select(staff =>
                new GroupStaffDto(
                    meeting.Id,
                    meeting.ActivityType,
                    staff.Id,
                    staff.Role,
                    staff.Name))).ToArray(),
            group.Meetings.Select(meeting => new MeetingDto(
                meeting.Id,
                meeting.ActivityType,
                (DayOfWeek)meeting.DayOfWeek,
                meeting.StartLocal,
                meeting.EndLocal,
                meeting.RoomId,
                meeting.RoomCode,
                meeting.Location)).ToArray(),
            Convert.ToBase64String(group.RowVersion));
    }

    private static IResult OfferingError(
        OfferingResult result,
        HttpContext context) =>
        result.Outcome switch
        {
            OfferingOutcome.NotFound => NotFound(
                context,
                result.ErrorCode ?? "NOT_FOUND"),
            OfferingOutcome.Conflict => Error(
                context,
                StatusCodes.Status409Conflict,
                result.ErrorCode ?? "CONFLICT",
                "The requested scheduling change conflicts with current data."),
            OfferingOutcome.ValidationError => Error(
                context,
                StatusCodes.Status400BadRequest,
                result.ErrorCode ?? "VALIDATION_ERROR",
                "The request body or parameters are invalid."),
            _ => Error(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "The scheduling request could not be completed."),
        };

    private static IResult NotFound(HttpContext context, string code) =>
        Error(
            context,
            StatusCodes.Status404NotFound,
            code,
            "The requested scheduling resource was not found.");

    private static bool IsStudentOnly(ClaimsPrincipal principal) =>
        principal.IsInRole("Student") && !principal.IsInRole("Admin");

    private static bool IsPublished(string state) =>
        string.Equals(state, "published", StringComparison.OrdinalIgnoreCase);

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
