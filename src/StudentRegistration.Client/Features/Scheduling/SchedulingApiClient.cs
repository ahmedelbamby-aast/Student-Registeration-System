using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Scheduling;

namespace StudentRegistration.Client.Features.Scheduling;

public sealed class SchedulingApiClient
{
    private const string AdminOfferingsPath = "/api/admin/offerings";
    private const string OfferingsPath = "/api/offerings";
    private const string AdminGroupsPath = "/api/admin/groups";
    private const string RoomsPath = "/api/admin/rooms";
    private const string AvailabilityPath = "/api/admin/staff-availability";
    private const string AlertsPath = "/api/admin/schedule-impact-alerts";
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";
    private const string AntiforgeryInterop =
        "StudentRegistration.antiforgery.getRequestToken";

    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime? _javascript;

    public SchedulingApiClient(HttpClient httpClient)
        : this(httpClient, null)
    {
    }

    public SchedulingApiClient(HttpClient httpClient, IJSRuntime? javascript)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _javascript = javascript;
    }

    public Task<AcademicApiResult<Page<CourseOfferingSummaryDto>>> ListOfferingsAsync(
        int page = 1,
        int pageSize = 20,
        string sort = "courseCode,id",
        string? query = null,
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<CourseOfferingSummaryDto>>(
            Query(
                AdminOfferingsPath,
                ("page", page.ToString()),
                ("pageSize", pageSize.ToString()),
                ("sort", sort),
                ("query", query)),
            cancellationToken);

    public Task<AcademicApiResult<CourseOfferingDto>> GetOfferingAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default) =>
        GetAsync<CourseOfferingDto>(
            $"{OfferingsPath}/{RequiredId(offeringId, nameof(offeringId)):D}",
            cancellationToken);

    public Task<AcademicApiResult<CourseOfferingDto>> CreateOfferingAsync(
        CreateOfferingRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CreateOfferingRequest, CourseOfferingDto>(
            HttpMethod.Post,
            AdminOfferingsPath,
            request,
            cancellationToken);

    public Task<AcademicApiResult<GroupDto>> UpdateGroupAsync(
        Guid groupId,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<UpdateGroupRequest, GroupDto>(
            HttpMethod.Put,
            $"{AdminGroupsPath}/{RequiredId(groupId, nameof(groupId)):D}",
            request,
            cancellationToken);

    public Task<AcademicApiResult<OfferingValidationResult>> ValidateOfferingAsync(
        Guid offeringId,
        ValidateOfferingRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<ValidateOfferingRequest, OfferingValidationResult>(
            HttpMethod.Post,
            $"{AdminOfferingsPath}/{RequiredId(offeringId, nameof(offeringId)):D}/validate",
            request,
            cancellationToken);

    public Task<AcademicApiResult<CourseOfferingDto>> PublishOfferingAsync(
        Guid offeringId,
        PublishOfferingRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<PublishOfferingRequest, CourseOfferingDto>(
            HttpMethod.Post,
            $"{AdminOfferingsPath}/{RequiredId(offeringId, nameof(offeringId)):D}/publish",
            request,
            cancellationToken);

    public Task<AcademicApiResult<Page<RoomDto>>> ListRoomsAsync(
        int page = 1,
        int pageSize = 20,
        string sort = "code,id",
        string? query = null,
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<RoomDto>>(
            Query(
                RoomsPath,
                ("page", page.ToString()),
                ("pageSize", pageSize.ToString()),
                ("sort", sort),
                ("query", query)),
            cancellationToken);

    public Task<AcademicApiResult<RoomDto>> CreateRoomAsync(
        CreateRoomRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<CreateRoomRequest, RoomDto>(
            HttpMethod.Post,
            RoomsPath,
            request,
            cancellationToken);

    public Task<AcademicApiResult<RoomDto>> UpdateRoomAsync(
        Guid roomId,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<UpdateRoomRequest, RoomDto>(
            HttpMethod.Put,
            $"{RoomsPath}/{RequiredId(roomId, nameof(roomId)):D}",
            request,
            cancellationToken);

    public Task<AcademicApiResult<Page<StaffTermAvailabilityDto>>> ListAvailabilityAsync(
        Guid termId,
        int page = 1,
        int pageSize = 20,
        string sort = "staffName,id",
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<StaffTermAvailabilityDto>>(
            Query(
                AvailabilityPath,
                ("termId", RequiredId(termId, nameof(termId)).ToString("D")),
                ("page", page.ToString()),
                ("pageSize", pageSize.ToString()),
                ("sort", sort)),
            cancellationToken);

    public Task<AcademicApiResult<Page<ScheduleImpactAlertDto>>> ListAlertsAsync(
        int page = 1,
        int pageSize = 20,
        string sort = "detectedAtUtc-desc,id",
        CancellationToken cancellationToken = default) =>
        GetAsync<Page<ScheduleImpactAlertDto>>(
            Query(
                AlertsPath,
                ("page", page.ToString()),
                ("pageSize", pageSize.ToString()),
                ("sort", sort)),
            cancellationToken);

    public Task<AcademicApiResult<ScheduleImpactAlertDto>> RevalidateAlertAsync(
        Guid alertId,
        RevalidateScheduleImpactAlertRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<RevalidateScheduleImpactAlertRequest, ScheduleImpactAlertDto>(
            HttpMethod.Post,
            $"{AlertsPath}/{RequiredId(alertId, nameof(alertId)):D}/revalidate",
            request,
            cancellationToken);

    public Task<AcademicApiResult<ScheduleImpactAlertDto>> ResolveAlertAsync(
        Guid alertId,
        ResolveScheduleImpactAlertRequest request,
        CancellationToken cancellationToken = default) =>
        SendMutationAsync<ResolveScheduleImpactAlertRequest, ScheduleImpactAlertDto>(
            HttpMethod.Post,
            $"{AlertsPath}/{RequiredId(alertId, nameof(alertId)):D}/resolve",
            request,
            cancellationToken);

    private async Task<AcademicApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<AcademicApiResult<TResponse>> SendMutationAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var message = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(request)
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
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<AcademicApiResult<T>> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    ResponseJsonOptions,
                    cancellationToken);
                return value is null
                    ? AcademicApiResult<T>.Failure(null, response.StatusCode)
                    : AcademicApiResult<T>.Success(value, response.StatusCode);
            }
            catch (Exception exception) when (
                exception is HttpRequestException
                    or NotSupportedException
                    or JsonException
                    or ArgumentException)
            {
                return AcademicApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        return AcademicApiResult<T>.Failure(
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
                ResponseJsonOptions,
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
                or NotSupportedException
                or JsonException
                or ArgumentException)
        {
            return null;
        }
    }

    private static string Query(string path, params (string Name, string? Value)[] values)
    {
        var query = values
            .Where(value => !string.IsNullOrWhiteSpace(value.Value))
            .Select(value =>
                $"{Uri.EscapeDataString(value.Name)}={Uri.EscapeDataString(value.Value!)}");
        return $"{path}?{string.Join("&", query)}";
    }

    private static Guid RequiredId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("A non-empty identifier is required.", parameterName)
            : value;
}
