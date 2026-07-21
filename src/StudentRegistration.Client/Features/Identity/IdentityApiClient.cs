using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;

namespace StudentRegistration.Client.Features.Identity;

public sealed class IdentityApiClient
{
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";
    private const string AntiforgeryInterop =
        "StudentRegistration.antiforgery.getRequestToken";

    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true
        };

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _javascript;

    public IdentityApiClient(HttpClient httpClient, IJSRuntime javascript)
    {
        _httpClient = httpClient;
        _javascript = javascript;
    }

    public Task<IdentityApiResult<SessionDto>> LoginStudentAsync(
        StudentLoginRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<StudentLoginRequest, SessionDto>(
            HttpMethod.Post,
            "api/auth/student/login",
            request,
            cancellationToken);

    public Task<IdentityApiResult<SessionDto>> ActivateStudentAsync(
        ActivateStudentRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<ActivateStudentRequest, SessionDto>(
            HttpMethod.Post,
            "api/auth/student/activate",
            request,
            cancellationToken);

    public Task<IdentityApiResult<SessionDto>> LoginStaffAsync(
        StaffLoginRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<StaffLoginRequest, SessionDto>(
            HttpMethod.Post,
            "api/auth/staff/login",
            request,
            cancellationToken);

    public Task<IdentityApiResult> RequestRecoveryAsync(
        RecoveryRequest request,
        CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(
            HttpMethod.Post,
            "api/auth/recovery/request",
            request,
            HttpStatusCode.Accepted,
            cancellationToken);

    public Task<IdentityApiResult> CompleteRecoveryAsync(
        RecoveryCompleteRequest request,
        CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(
            HttpMethod.Post,
            "api/auth/recovery/complete",
            request,
            HttpStatusCode.NoContent,
            cancellationToken);

    public async Task<IdentityApiResult<SessionDto>> GetSessionAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "api/auth/session",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<SessionDto>(response, cancellationToken);
    }

    public Task<IdentityApiResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(
            HttpMethod.Post,
            "api/auth/password/change",
            request,
            HttpStatusCode.NoContent,
            cancellationToken);

    public Task<IdentityApiResult> RevokeAllSessionsAsync(
        CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync<object?>(
            HttpMethod.Post,
            "api/auth/sessions/revoke-all",
            null,
            HttpStatusCode.NoContent,
            cancellationToken);

    public Task<IdentityApiResult> LogoutAsync(
        CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync<object?>(
            HttpMethod.Post,
            "api/auth/logout",
            null,
            HttpStatusCode.NoContent,
            cancellationToken);

    public async Task<IdentityApiResult<Page<IdentityUserSummaryDto>>> ListAdminUsersAsync(
        string? search,
        int page,
        int pageSize = 20,
        string sort = "displayName,id",
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            $"sort={Uri.EscapeDataString(sort)}"
        };
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        using var response = await _httpClient.GetAsync(
            $"api/admin/users?{string.Join('&', query)}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<Page<IdentityUserSummaryDto>>(response, cancellationToken);
    }

    public Task<IdentityApiResult<IdentityImportBatchDto>> CreateAdminImportAsync(
        IdentityImportRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<IdentityImportRequest, IdentityImportBatchDto>(
            HttpMethod.Post,
            "api/admin/users/imports",
            request,
            cancellationToken);

    public async Task<IdentityApiResult<IdentityImportBatchDto>> GetAdminImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/admin/users/imports/{importId:D}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<IdentityImportBatchDto>(response, cancellationToken);
    }

    public Task<IdentityApiResult<IdentityImportBatchDto>> PublishAdminImportAsync(
        Guid importId,
        IdentityImportPublishRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<IdentityImportPublishRequest, IdentityImportBatchDto>(
            HttpMethod.Post,
            $"api/admin/users/imports/{importId:D}/publish",
            request,
            cancellationToken);

    public Task<IdentityApiResult<IdentityUserSummaryDto>> ChangeAdminUserStatusAsync(
        Guid userId,
        UserStatusRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<UserStatusRequest, IdentityUserSummaryDto>(
            HttpMethod.Patch,
            $"api/admin/users/{userId:D}/status",
            request,
            cancellationToken);

    public Task<IdentityApiResult<IdentityUserSummaryDto>> ReplaceAdminUserRolesAsync(
        Guid userId,
        UserRolesRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<UserRolesRequest, IdentityUserSummaryDto>(
            HttpMethod.Put,
            $"api/admin/users/{userId:D}/roles",
            request,
            cancellationToken);

    private async Task<IdentityApiResult<TResponse>> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var message = await CreateMutationAsync(
            method,
            path,
            request,
            cancellationToken);
        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private async Task<IdentityApiResult> SendWithoutResponseAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        HttpStatusCode expectedStatus,
        CancellationToken cancellationToken)
    {
        using var message = await CreateMutationAsync(
            method,
            path,
            request,
            cancellationToken);
        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (response.StatusCode == expectedStatus)
        {
            return IdentityApiResult.Success();
        }

        return IdentityApiResult.Failure(
            await ReadErrorAsync(response, cancellationToken),
            response.StatusCode);
    }

    private async Task<HttpRequestMessage> CreateMutationAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(method, path);
        if (request is not null)
        {
            message.Content = JsonContent.Create(request);
        }

        var requestToken = await _javascript.InvokeAsync<string>(
            AntiforgeryInterop,
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(requestToken))
        {
            message.Headers.TryAddWithoutValidation(AntiforgeryHeader, requestToken);
        }

        return message;
    }

    private static async Task<IdentityApiResult<T>> ReadAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var value = await response.Content.ReadFromJsonAsync<T>(
                    ResponseJsonOptions,
                    cancellationToken: cancellationToken);
                return value is null
                    ? IdentityApiResult<T>.Failure(null, response.StatusCode)
                    : IdentityApiResult<T>.Success(value);
            }
            catch (HttpRequestException)
            {
                return IdentityApiResult<T>.Failure(null, response.StatusCode);
            }
            catch (NotSupportedException)
            {
                return IdentityApiResult<T>.Failure(null, response.StatusCode);
            }
            catch (JsonException)
            {
                return IdentityApiResult<T>.Failure(null, response.StatusCode);
            }
            catch (ArgumentException)
            {
                return IdentityApiResult<T>.Failure(null, response.StatusCode);
            }
        }

        return IdentityApiResult<T>.Failure(
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
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public sealed record IdentityApiResult(
    bool IsSuccess,
    ApiError? Error,
    HttpStatusCode StatusCode)
{
    public static IdentityApiResult Success() => new(true, null, HttpStatusCode.OK);

    public static IdentityApiResult Failure(ApiError? error, HttpStatusCode statusCode) =>
        new(false, error, statusCode);
}

public sealed record IdentityApiResult<T>(
    bool IsSuccess,
    T? Value,
    ApiError? Error,
    HttpStatusCode StatusCode)
{
    public static IdentityApiResult<T> Success(T value) =>
        new(true, value, null, HttpStatusCode.OK);

    public static IdentityApiResult<T> Failure(ApiError? error, HttpStatusCode statusCode) =>
        new(false, default, error, statusCode);
}
