using System.Text.Json;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

internal static class IdentityRouteContractAssertions
{
    internal static void AssertRoute(
        string fixturePath,
        string pagePath,
        string routeId,
        string routeTemplate,
        string mapperRecord,
        params (string Method, string Path, string ClientMethod)[] apis)
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(fixturePath));
        var root = document.RootElement;
        Assert.Equal("frontend-fixture/2.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal(routeId, root.GetProperty("routeId").GetString());
        Assert.Equal(routeTemplate, root.GetProperty("routeTemplate").GetString());
        Assert.Contains("SERVICE_UNAVAILABLE", Strings(root, "reasonCodes"));
        Assert.Contains("RATE_LIMITED", Strings(root, "reasonCodes"));

        var fixtureApis = root.GetProperty("apis").EnumerateArray().ToArray();
        Assert.Equal(apis.Length, fixtureApis.Length);
        var client = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityApiClient.cs");
        foreach (var expected in apis)
        {
            Assert.Contains(
                fixtureApis,
                api => api.GetProperty("method").GetString() == expected.Method
                    && api.GetProperty("path").GetString() == expected.Path);
            RepositoryFiles.ContainsAll(client, expected.ClientMethod, expected.Path.TrimStart('/'));
        }

        var page = RepositoryFiles.Read(pagePath);
        RepositoryFiles.ContainsAll(
            page,
            $"@page \"{routeTemplate}\"",
            mapperRecord,
            "ApplyFailure",
            "SERVICE_UNAVAILABLE");
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
    }

    internal static void AssertRejectedStateNeverBecomesSuccess(
        string record,
        string serviceState,
        string reasonCode)
    {
        var result = IdentityRouteStateMapper.Map(
            record,
            serviceState,
            serverAccepted: false,
            reasonCode);

        Assert.NotEqual(StudentRegistration.Client.UX.RouteUiState.Success, result.UiState);
        Assert.Equal(reasonCode, result.ReasonCode);
    }

    private static string[] Strings(JsonElement root, string propertyName) =>
        root.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
}
