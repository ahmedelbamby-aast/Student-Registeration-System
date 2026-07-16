using System.Net.Http.Json;
using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Operations;

namespace StudentRegistration.Client.Features.Operations;

public sealed class OperationsApiClient(HttpClient httpClient)
{
    public const string HealthPath = "/api/health";

    public async Task<OperationsApiResult<HealthSummary>> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            HealthPath,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (response.IsSuccessStatusCode ||
            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            try
            {
                var summary = await response.Content.ReadFromJsonAsync<HealthSummary>(
                    cancellationToken: cancellationToken);
                if (summary is not null)
                {
                    return OperationsApiResult<HealthSummary>.Success(summary);
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (JsonException)
            {
            }
        }

        ApiError? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<ApiError>(
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (JsonException)
        {
        }

        return OperationsApiResult<HealthSummary>.Failure(error);
    }
}

public sealed record OperationsApiResult<T>(bool IsSuccess, T? Value, ApiError? Error)
{
    public static OperationsApiResult<T> Success(T value) => new(true, value, null);

    public static OperationsApiResult<T> Failure(ApiError? error) =>
        new(false, default, error);
}
