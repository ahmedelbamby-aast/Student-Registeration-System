using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Staff;

namespace StudentRegistration.Client.Features.Staff;

/// <summary>
/// Same-origin transport for the assignment-scoped staff workspace.
/// Staff identity, role, term, and assignment scope are always derived by the server.
/// </summary>
public sealed class StaffApiClient
{
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";
    private const string AntiforgeryInterop =
        "StudentRegistration.antiforgery.getRequestToken";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime? _javascript;

    public StaffApiClient(HttpClient httpClient)
        : this(httpClient, null)
    {
    }

    public StaffApiClient(HttpClient httpClient, IJSRuntime? javascript)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _javascript = javascript;
    }

    public Task<StaffApiResult<IReadOnlyList<StaffAssignmentDto>>> GetAssignmentsAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<StaffAssignmentDto>>(
            "/api/staff/assignments",
            cancellationToken);

    public Task<StaffApiResult<StaffTimetableDto>> GetTimetableAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<StaffTimetableDto>("/api/staff/timetable", cancellationToken);

    public Task<StaffApiResult<Page<RosterRowDto>>> GetRosterAsync(
        Guid groupId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize is < 1 or > StaffWorkspaceContract.MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        return GetAsync<Page<RosterRowDto>>(
            $"/api/staff/groups/{RequiredId(groupId, nameof(groupId)):D}/roster" +
            $"?page={page}&pageSize={pageSize}",
            cancellationToken);
    }

    public Task<StaffApiResult<StaffTermAvailabilityDto>> GetAvailabilityAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<StaffTermAvailabilityDto>(
            "/api/staff/availability",
            cancellationToken);

    public async Task<StaffApiResult<AvailabilityUpdateResult>> ReplaceAvailabilityAsync(
        ReplaceAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var message = new HttpRequestMessage(
            HttpMethod.Put,
            "/api/staff/availability")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        if (_javascript is not null)
        {
            var token = await _javascript.InvokeAsync<string>(
                AntiforgeryInterop,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                message.Headers.TryAddWithoutValidation(AntiforgeryHeader, token);
            }
        }

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<AvailabilityUpdateResult>(response, cancellationToken);
    }

    private async Task<StaffApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private static async Task<StaffApiResult<T>> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    JsonOptions,
                    cancellationToken);
                return value is null
                    ? StaffApiResult<T>.Failure(null, response.StatusCode)
                    : StaffApiResult<T>.Success(value, response.StatusCode);
            }
            catch (Exception exception) when (
                exception is HttpRequestException or NotSupportedException or
                    JsonException or ArgumentException)
            {
                return StaffApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            try
            {
                var conflict = await response.Content.ReadFromJsonAsync<AvailabilityConflictDto>(
                    JsonOptions,
                    cancellationToken);
                if (conflict is not null)
                {
                    return StaffApiResult<T>.Failure(
                        conflict.Error,
                        response.StatusCode,
                        conflict);
                }
            }
            catch (Exception exception) when (
                exception is HttpRequestException or NotSupportedException or
                    JsonException or ArgumentException)
            {
                // Fall through to the canonical safe error reader.
            }
        }

        return StaffApiResult<T>.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private static async Task<ApiError?> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiError>(
                JsonOptions,
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or NotSupportedException or
                JsonException or ArgumentException)
        {
            return null;
        }
    }

    private static Guid RequiredId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("A non-empty identifier is required.", parameterName)
            : value;
}

public sealed record StaffApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    HttpStatusCode StatusCode,
    AvailabilityConflictDto? Conflict)
{
    public static StaffApiResult<T> Success(T value, HttpStatusCode statusCode) =>
        new(true, value, null, statusCode, null);

    public static StaffApiResult<T> Failure(
        ApiError? error,
        HttpStatusCode statusCode,
        AvailabilityConflictDto? conflict = null) =>
        new(false, default, error, statusCode, conflict);
}
