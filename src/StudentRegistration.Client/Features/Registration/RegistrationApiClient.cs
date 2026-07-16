using System.Net.Http.Json;
using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Registration;

namespace StudentRegistration.Client.Features.Registration;

/// <summary>
/// Read-only client for SPEC-011 discovery. Eligibility, filtering, paging,
/// capacity, and selectability remain authoritative on the server.
/// </summary>
public sealed class RegistrationApiClient
{
    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true
        };

    private readonly HttpClient _httpClient;

    public RegistrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<RegistrationApiResult<Page<OfferingEligibilityDto>>> ListOfferingsAsync(
        Guid termId,
        OfferingDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return GetAsync<Page<OfferingEligibilityDto>>(
            Query(
                $"/api/student/terms/{RequiredId(termId, nameof(termId)):D}/offerings",
                ("q", query.Search),
                ("eligibility", query.Eligibility),
                ("credits", query.Credits),
                ("day", query.Day),
                ("availability", query.Availability),
                ("sort", query.Sort),
                ("page", query.Page.ToString()),
                ("pageSize", query.PageSize.ToString())),
            cancellationToken);
    }

    public Task<RegistrationApiResult<OfferingEligibilityDto>> GetEligibilityAsync(
        Guid offeringId,
        CancellationToken cancellationToken = default) =>
        GetAsync<OfferingEligibilityDto>(
            $"/api/student/offerings/{RequiredId(offeringId, nameof(offeringId)):D}/eligibility",
            cancellationToken);

    private async Task<RegistrationApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    ResponseJsonOptions,
                    cancellationToken);
                return value is null
                    ? RegistrationApiResult<T>.Failure(null, response.StatusCode)
                    : RegistrationApiResult<T>.Success(
                        value,
                        response.StatusCode,
                        ReadMetadata(response));
            }
            catch (Exception exception) when (
                exception is HttpRequestException
                    or NotSupportedException
                    or JsonException
                    or ArgumentException)
            {
                return RegistrationApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        return RegistrationApiResult<T>.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private static RegistrationResponseMetadata ReadMetadata(HttpResponseMessage response) =>
        new(
            Header(response, "X-Eligibility-Policy-Version"),
            Header(response, "X-Eligibility-Reason-Codes"),
            Header(response, "X-Registration-Window-State"),
            Header(response, "X-Support-Reference-Path"));

    private static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values)
            ? string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)))
            : null;

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

public sealed record OfferingDiscoveryQuery(
    string? Search,
    string Eligibility,
    string? Credits,
    string? Day,
    string Availability,
    string Sort,
    int Page = 1,
    int PageSize = 20);

public sealed record RegistrationResponseMetadata(
    string? PolicyVersion,
    string? ReasonCodes,
    string? RegistrationWindowState,
    string? SupportReferencePath);

public sealed record RegistrationApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    System.Net.HttpStatusCode StatusCode,
    RegistrationResponseMetadata? Metadata)
{
    public static RegistrationApiResult<T> Success(
        T value,
        System.Net.HttpStatusCode statusCode,
        RegistrationResponseMetadata metadata) =>
        new(true, value, null, statusCode, metadata);

    public static RegistrationApiResult<T> Failure(
        ApiError? error,
        System.Net.HttpStatusCode statusCode) =>
        new(false, default, error, statusCode, null);
}
