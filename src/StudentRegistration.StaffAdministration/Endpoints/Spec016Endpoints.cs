using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Staff;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.StaffAdministration.Application;
using StudentRegistration.StaffAdministration.Application.Ports;

namespace StudentRegistration.StaffAdministration.Endpoints;

public static class Spec016Endpoints
{
    private const string ContextReadPolicy = "Context.Read";

    public static IEndpointRouteBuilder MapSpec016Endpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/staff/assignments", Assignments)
            .RequireAuthorization(ContextReadPolicy)
            .Produces<StaffAssignmentDto[]>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/staff/timetable", Timetable)
            .RequireAuthorization(ContextReadPolicy)
            .Produces<StaffTimetableDto>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/staff/groups/{groupId:guid}/roster", Roster)
            .RequireAuthorization(ContextReadPolicy)
            .Produces<Page<RosterRowDto>>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/staff/availability", Availability)
            .RequireAuthorization(ContextReadPolicy)
            .Produces<StaffTermAvailabilityDto>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapPut("/api/staff/availability", ReplaceAvailability)
            .RequireAuthorization(ContextReadPolicy)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .Produces<AvailabilityUpdateResult>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<AvailabilityConflictDto>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }

    private static async Task<IResult> Assignments(
        HttpContext context,
        StaffWorkspaceQueries queries,
        CancellationToken cancellationToken) =>
        ToAssignments(
            context,
            await queries.GetAssignmentsAsync(context.User, cancellationToken));

    private static async Task<IResult> Timetable(
        HttpContext context,
        StaffWorkspaceQueries queries,
        CancellationToken cancellationToken) =>
        ToTimetable(
            context,
            await queries.GetTimetableAsync(context.User, cancellationToken));

    private static async Task<IResult> Roster(
        [FromRoute] Guid groupId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        HttpContext context,
        StaffWorkspaceQueries queries,
        CancellationToken cancellationToken) =>
        ToRoster(
            context,
            await queries.GetRosterAsync(
                context.User,
                groupId,
                page ?? 1,
                pageSize ?? StaffWorkspaceContract.DefaultPageSize,
                CorrelationId(context),
                cancellationToken));

    private static async Task<IResult> Availability(
        HttpContext context,
        ICurrentAcademicTermProvider academicContext,
        IStaffIdentityResolver staffIdentity,
        StaffAvailabilityFacade facade,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context.User, out var applicationUserId))
        {
            return Error(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required.");
        }
        if (!IsTeachingContext(context.User))
        {
            return Error(context, StatusCodes.Status403Forbidden, "FORBIDDEN", "The selected staff context is not authorized.");
        }
        var staffId = await staffIdentity.ResolveActiveStaffIdAsync(
            applicationUserId,
            cancellationToken);
        if (staffId is null)
        {
            return Error(context, StatusCodes.Status404NotFound, "AVAILABILITY_NOT_FOUND", "No applicable staff availability was found.");
        }

        var term = await ResolveTermId(academicContext, cancellationToken);
        if (term is null)
        {
            return Error(context, StatusCodes.Status404NotFound, "AVAILABILITY_NOT_FOUND", "No applicable staff availability was found.");
        }

        return ToAvailability(
            context,
            await facade.GetOwnAsync(staffId.Value, term.Value, cancellationToken),
            update: false);
    }

    private static async Task<IResult> ReplaceAvailability(
        [FromBody] ReplaceAvailabilityRequest request,
        HttpContext context,
        ICurrentAcademicTermProvider academicContext,
        IStaffIdentityResolver staffIdentity,
        StaffAvailabilityFacade facade,
        CancellationToken cancellationToken)
    {
        if (!TryApplicationUserId(context.User, out var applicationUserId))
        {
            return Error(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required.");
        }
        if (!IsTeachingContext(context.User))
        {
            return Error(context, StatusCodes.Status403Forbidden, "FORBIDDEN", "The selected staff context is not authorized.");
        }
        var staffId = await staffIdentity.ResolveActiveStaffIdAsync(
            applicationUserId,
            cancellationToken);
        if (staffId is null)
        {
            return Error(context, StatusCodes.Status404NotFound, "AVAILABILITY_NOT_FOUND", "No applicable staff availability was found.");
        }

        var term = await ResolveTermId(academicContext, cancellationToken);
        if (term is null)
        {
            return Error(context, StatusCodes.Status404NotFound, "AVAILABILITY_NOT_FOUND", "No applicable staff availability was found.");
        }

        if (!TryCommand(request, staffId.Value, term.Value, CorrelationId(context), out var command))
        {
            return Error(context, StatusCodes.Status400BadRequest, "AVAILABILITY_RANGE_INVALID", "The complete availability range set is invalid.");
        }

        return ToAvailability(
            context,
            await facade.ReplaceOwnAsync(command!, cancellationToken),
            update: true);
    }

    private static IResult ToAssignments(
        HttpContext context,
        StaffAssignmentsQueryResult result) => result.Outcome switch
        {
            StaffWorkspaceQueryOutcome.Succeeded when result.Assignments is not null =>
                Results.Ok(result.Assignments),
            StaffWorkspaceQueryOutcome.Unauthorized =>
                Error(context, 401, "UNAUTHORIZED", "Authentication is required."),
            StaffWorkspaceQueryOutcome.Forbidden =>
                Error(context, 403, "FORBIDDEN", "The selected staff context is not authorized."),
            _ => Error(context, 503, result.ErrorCode ?? "STAFF_SCOPE_UNAVAILABLE", "The staff workspace is temporarily unavailable.")
        };

    private static IResult ToTimetable(
        HttpContext context,
        StaffTimetableQueryResult result) => result.Outcome switch
        {
            StaffWorkspaceQueryOutcome.Succeeded when result.Timetable is not null =>
                Results.Ok(result.Timetable),
            StaffWorkspaceQueryOutcome.Unauthorized =>
                Error(context, 401, "UNAUTHORIZED", "Authentication is required."),
            StaffWorkspaceQueryOutcome.Forbidden =>
                Error(context, 403, "FORBIDDEN", "The selected staff context is not authorized."),
            _ => Error(context, 503, result.ErrorCode ?? "STAFF_SCOPE_UNAVAILABLE", "The staff timetable is temporarily unavailable.")
        };

    private static IResult ToRoster(
        HttpContext context,
        StaffRosterQueryResult result) => result.Outcome switch
        {
            StaffWorkspaceQueryOutcome.Succeeded when result.Page is not null =>
                Results.Ok(result.Page),
            StaffWorkspaceQueryOutcome.Invalid =>
                Error(context, 400, result.ErrorCode ?? "PAGE_SIZE_INVALID", "The roster request is invalid."),
            StaffWorkspaceQueryOutcome.Unauthorized =>
                Error(context, 401, "UNAUTHORIZED", "Authentication is required."),
            StaffWorkspaceQueryOutcome.Forbidden =>
                Error(context, 403, "FORBIDDEN", "The selected staff context is not authorized."),
            StaffWorkspaceQueryOutcome.NotFound =>
                Error(context, 404, "STAFF_GROUP_NOT_FOUND", "The assigned group was not found."),
            _ => Error(context, 503, result.ErrorCode ?? "ROSTER_UNAVAILABLE", "The roster is temporarily unavailable.")
        };

    private static IResult ToAvailability(
        HttpContext context,
        StaffAvailabilityPortResult result,
        bool update)
    {
        var current = result.Availability is null ? null : ToDto(result.Availability);
        return result.Outcome switch
        {
            StaffAvailabilityPortOutcome.Success when current is not null && update =>
                Results.Ok(new AvailabilityUpdateResult(current, result.ImpactAlertIds)),
            StaffAvailabilityPortOutcome.Success when current is not null => Results.Ok(current),
            StaffAvailabilityPortOutcome.NotFound =>
                Error(context, 404, "AVAILABILITY_NOT_FOUND", "No applicable staff availability was found."),
            StaffAvailabilityPortOutcome.ValidationFailed =>
                Error(context, 400, "AVAILABILITY_RANGE_INVALID", "The complete availability range set is invalid."),
            StaffAvailabilityPortOutcome.StaleVersion when current is not null =>
                Conflict(context, result, current, "STALE_VERSION", "Availability changed after this page was loaded."),
            StaffAvailabilityPortOutcome.DeadlinePassed when current is not null =>
                Conflict(context, result, current, "AVAILABILITY_DEADLINE_PASSED", "The availability deadline has passed."),
            _ => Error(context, 503, "AVAILABILITY_UNAVAILABLE", "Staff availability is temporarily unavailable.")
        };
    }

    private static IResult Conflict(
        HttpContext context,
        StaffAvailabilityPortResult result,
        StaffTermAvailabilityDto current,
        string code,
        string message) => Results.Json(
        new AvailabilityConflictDto(
            new ApiError(code, message, CorrelationId(context), currentVersion: current.RowVersion),
            current,
            result.ServerTimeUtc,
            result.DeadlineUtc ?? current.DeadlineUtc),
        statusCode: StatusCodes.Status409Conflict);

    private static StaffTermAvailabilityDto ToDto(StaffTermAvailabilitySnapshot snapshot) => new(
        snapshot.Id,
        snapshot.StaffId,
        snapshot.TermId,
        snapshot.DeadlineUtc,
        Convert.ToBase64String(snapshot.RowVersion),
        snapshot.Ranges.Select(range => new AvailabilityRangeDto(
            range.Id,
            range.DayOfWeek,
            range.StartLocal,
            range.EndLocal,
            range.IsAvailable ? "available" : "unavailable")).ToArray());

    private static async Task<Guid?> ResolveTermId(
        ICurrentAcademicTermProvider resolver,
        CancellationToken cancellationToken)
        => await resolver.ResolveCurrentTermIdAsync(cancellationToken);

    private static bool TryCommand(
        ReplaceAvailabilityRequest request,
        Guid staffId,
        Guid termId,
        string correlationId,
        out ReplaceOwnStaffAvailability? command)
    {
        command = null;
        if (request is null
            || string.IsNullOrWhiteSpace(request.ExpectedStaffTermRowVersion)
            || request.Ranges is null
            || request.Ranges.Count == 0)
        {
            return false;
        }

        byte[] version;
        try
        {
            version = Convert.FromBase64String(request.ExpectedStaffTermRowVersion);
        }
        catch (FormatException)
        {
            return false;
        }

        if (version.Length == 0)
        {
            return false;
        }

        var ranges = new List<StaffAvailabilityRangeInput>(request.Ranges.Count);
        foreach (var range in request.Ranges)
        {
            if (range.Id == Guid.Empty || range.EndLocal <= range.StartLocal)
            {
                return false;
            }

            var input = range.Kind switch
            {
                "available" => StaffAvailabilityRangeInput.Available(
                    range.Id, range.DayOfWeek, range.StartLocal, range.EndLocal),
                "unavailable" => StaffAvailabilityRangeInput.Unavailable(
                    range.Id, range.DayOfWeek, range.StartLocal, range.EndLocal),
                _ => null
            };
            if (input is null)
            {
                return false;
            }

            ranges.Add(input);
        }

        command = new ReplaceOwnStaffAvailability(
            staffId,
            termId,
            version,
            ranges,
            "staff self-service availability replacement",
            correlationId);
        return true;
    }

    private static bool TryApplicationUserId(
        ClaimsPrincipal principal,
        out Guid applicationUserId)
    {
        applicationUserId = Guid.Empty;
        return principal.Identity?.IsAuthenticated == true
            && Guid.TryParse(
                principal.FindFirstValue(ClaimTypes.NameIdentifier),
                out applicationUserId)
            && applicationUserId != Guid.Empty;
    }

    private static bool IsTeachingContext(ClaimsPrincipal principal)
    {
        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return roles.Length == 1
            && roles[0] is StaffWorkspaceContract.LecturerRole
                or StaffWorkspaceContract.TeachingAssistantRole;
    }

    private static IResult Error(
        HttpContext context,
        int status,
        string code,
        string message) => Results.Json(
        new ApiError(code, message, CorrelationId(context)),
        statusCode: status);

    private static string CorrelationId(HttpContext context) =>
        string.IsNullOrWhiteSpace(context.TraceIdentifier)
            ? context.TraceIdentifier = Guid.NewGuid().ToString("N")
            : context.TraceIdentifier;
}
