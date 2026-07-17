using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.Registration.Application;

namespace StudentRegistration.Registration.Endpoints;

public static class Spec015Endpoints
{
    private const string RegistrationRecordsReadOwn = "RegistrationRecords.ReadOwn";
    private const string RegistrationRecordsRead = "RegistrationRecords.Read";

    public static IEndpointRouteBuilder MapSpec015Endpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/student/registrations", ListOwn)
            .RequireAuthorization(RegistrationRecordsReadOwn)
            .Produces<Page<RegistrationHistoryRowDto>>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/student/registrations/current/timetable", CurrentTimetable)
            .RequireAuthorization(RegistrationRecordsReadOwn)
            .Produces<RegistrationTimetableDto>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet("/api/student/registrations/{submissionId:guid}", ReadOwn)
            .RequireAuthorization(RegistrationRecordsReadOwn)
            .Produces<RegistrationDetailDto>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet(
                "/api/admin/students/{studentId:guid}/terms/{termId:guid}/registrations",
                ListAdmin)
            .RequireAuthorization(RegistrationRecordsRead)
            .Produces<Page<RegistrationHistoryRowDto>>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);

        endpoints.MapGet(
                "/api/admin/students/{studentId:guid}/terms/{termId:guid}/registrations/{submissionId:guid}",
                ReadAdmin)
            .RequireAuthorization(RegistrationRecordsRead)
            .Produces<RegistrationDetailDto>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);
        return endpoints;
    }

    private static async Task<IResult> ListOwn(
        [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] Guid? termId,
        HttpContext context, RegistrationRecordQueries queries,
        CancellationToken cancellationToken) => ToList(context,
            await queries.ListOwnAsync(context.User, page ?? 1, pageSize ?? 20,
                termId, cancellationToken));

    private static async Task<IResult> ReadOwn(
        [FromRoute] Guid submissionId, HttpContext context,
        RegistrationRecordQueries queries, CancellationToken cancellationToken) =>
        ToDetail(context, await queries.ReadOwnAsync(
            context.User, submissionId, cancellationToken));

    private static async Task<IResult> CurrentTimetable(
        HttpContext context, RegistrationRecordQueries queries,
        CancellationToken cancellationToken) => ToTimetable(context,
            await queries.ReadCurrentAsync(context.User, cancellationToken));

    private static async Task<IResult> ListAdmin(
        [FromRoute] Guid studentId, [FromRoute] Guid termId,
        [FromQuery] int? page, [FromQuery] int? pageSize,
        HttpContext context, RegistrationRecordQueries queries,
        CancellationToken cancellationToken) => ToList(context,
            await queries.ListAdminAsync(context.User, studentId, termId,
                page ?? 1, pageSize ?? 20, CorrelationId(context), cancellationToken));

    private static async Task<IResult> ReadAdmin(
        [FromRoute] Guid studentId, [FromRoute] Guid termId,
        [FromRoute] Guid submissionId, HttpContext context,
        RegistrationRecordQueries queries, CancellationToken cancellationToken) =>
        ToDetail(context, await queries.ReadAdminAsync(context.User, studentId,
            termId, submissionId, CorrelationId(context), cancellationToken));

    private static IResult ToList(HttpContext context, RegistrationRecordListResult result) =>
        result.Outcome switch
        {
            RegistrationRecordQueryOutcome.Succeeded when result.Page is not null => Results.Ok(result.Page),
            RegistrationRecordQueryOutcome.Invalid => Error(context, StatusCodes.Status400BadRequest, result.ErrorCode!, "The paging or term filter is invalid."),
            RegistrationRecordQueryOutcome.Unauthorized => Error(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required."),
            RegistrationRecordQueryOutcome.NotFound => Error(context, StatusCodes.Status404NotFound, "REGISTRATION_NOT_FOUND", "The registration record was not found."),
            _ => Error(context, StatusCodes.Status503ServiceUnavailable, "REGISTRATION_RECORD_UNAVAILABLE", "Registration records are temporarily unavailable.")
        };

    private static IResult ToDetail(HttpContext context, RegistrationRecordDetailResult result) =>
        result.Outcome switch
        {
            RegistrationRecordQueryOutcome.Succeeded when result.Detail is not null => Results.Ok(result.Detail),
            RegistrationRecordQueryOutcome.Unauthorized => Error(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required."),
            RegistrationRecordQueryOutcome.NotFound => Error(context, StatusCodes.Status404NotFound, "REGISTRATION_NOT_FOUND", "The registration record was not found."),
            _ => Error(context, StatusCodes.Status503ServiceUnavailable, "REGISTRATION_RECORD_UNAVAILABLE", "Registration records are temporarily unavailable.")
        };

    private static IResult ToTimetable(HttpContext context, RegistrationTimetableResult result) =>
        result.Outcome switch
        {
            RegistrationRecordQueryOutcome.Succeeded when result.Timetable is not null => Results.Ok(result.Timetable),
            RegistrationRecordQueryOutcome.Unauthorized => Error(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required."),
            RegistrationRecordQueryOutcome.NotFound => Error(context, StatusCodes.Status404NotFound, "STUDENT_NOT_FOUND", "The student record was not found."),
            RegistrationRecordQueryOutcome.Conflict => Error(context, StatusCodes.Status409Conflict, result.ErrorCode!, "The current academic term is ambiguous."),
            _ => Error(context, StatusCodes.Status503ServiceUnavailable, "REGISTRATION_RECORD_UNAVAILABLE", "The current timetable is temporarily unavailable.")
        };

    private static IResult Error(HttpContext context, int status, string code, string message) =>
        Results.Json(new ApiError(code, message, CorrelationId(context)), statusCode: status);

    private static string CorrelationId(HttpContext context) =>
        string.IsNullOrWhiteSpace(context.TraceIdentifier)
            ? context.TraceIdentifier = Guid.NewGuid().ToString("N")
            : context.TraceIdentifier;
}
