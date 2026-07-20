using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Contracts;

namespace StudentRegistration.Client.Features.Registration;

public sealed class RegistrationApprovalApiClient(
    HttpClient httpClient,
    IJSRuntime javascript)
{
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";
    private const string AntiforgeryInterop =
        "StudentRegistration.antiforgery.getRequestToken";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public Task<AcademicApiResult<RegistrationApprovalPageDto>> ListAsync(
        bool admin,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetAsync<RegistrationApprovalPageDto>(
            $"{Prefix(admin)}?state=pending&page={page}&pageSize={pageSize}",
            cancellationToken);

    public async Task<AcademicApiResult<JsonElement>> DecideAsync(
        bool admin,
        Guid submissionId,
        Guid lineId,
        ApprovalDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{Prefix(admin)}/{submissionId:D}/lines/{lineId:D}/decision")
        {
            Content = JsonContent.Create(request),
        };
        var token = await javascript.InvokeAsync<string>(
            AntiforgeryInterop,
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            message.Headers.TryAddWithoutValidation(AntiforgeryHeader, token);
        }

        using var response = await httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<JsonElement>(response, cancellationToken);
    }

    private async Task<AcademicApiResult<T>> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private static async Task<AcademicApiResult<T>> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken);
                return value is null
                    ? AcademicApiResult<T>.Failure(null, response.StatusCode)
                    : AcademicApiResult<T>.Success(value, response.StatusCode);
            }
            catch (JsonException)
            {
                return AcademicApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        ApiError? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<ApiError>(Json, cancellationToken);
        }
        catch (JsonException)
        {
            // The page still receives the authoritative status code.
        }

        return AcademicApiResult<T>.Failure(error, response.StatusCode);
    }

    private static string Prefix(bool admin) => admin
        ? "/api/admin/registration-approvals"
        : "/api/staff/registration-approvals";
}

public sealed record ApprovalDecisionRequest(
    string Decision,
    string Reason,
    string ExpectedSubmissionRowVersion,
    string ExpectedLineRowVersion,
    Guid ClientRequestId);

public sealed record RegistrationApprovalPageDto(
    IReadOnlyList<RegistrationApprovalQueueRowDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record RegistrationApprovalQueueRowDto(
    Guid SubmissionId,
    RegistrationApprovalLineDto Line,
    string StudentUniversityId,
    string StudentDisplayName,
    decimal RequestedCredits,
    decimal CurrentCgpa,
    bool Overload,
    DateTime SubmittedAtUtc,
    DateTime WindowClosesAtUtc,
    string SubmissionVersion);

public sealed record RegistrationApprovalLineDto(
    Guid LineId,
    Guid OfferingId,
    Guid GroupId,
    string CourseCode,
    string SubjectTitle,
    decimal Credits,
    string State,
    RegistrationApprovalCapacityDto Capacity,
    string RowVersion);

public sealed record RegistrationApprovalCapacityDto(
    int Capacity,
    int EnrolledCount,
    int HeldSeatCount,
    int AvailableSeatCount);
