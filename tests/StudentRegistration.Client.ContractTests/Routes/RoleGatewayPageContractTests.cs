using System.Reflection;
using System.Text.Json;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.UX;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class RoleGatewayPageContractTests
{
    private const string FixturePath =
        "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec008/AUTH-01/route-contract.json";
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/RoleGatewayPage.razor";
    private const string ApiClientPath =
        "src/StudentRegistration.Client/Features/Academics/AcademicApiClient.cs";

    [Fact]
    public void Auth01_fixture_freezes_the_route_api_states_reasons_and_named_destinations()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(FixturePath));
        var root = document.RootElement;

        Assert.Equal("frontend-fixture/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("AUTH-01", root.GetProperty("routeId").GetString());
        Assert.Equal("/", root.GetProperty("routeTemplate").GetString());
        Assert.Equal("GET", root.GetProperty("api").GetProperty("method").GetString());
        Assert.Equal(
            "/api/public/context",
            root.GetProperty("api").GetProperty("path").GetString());
        Assert.Equal(
            ["loading", "success", "service-error", "stale", "offline"],
            Strings(root, "requiredStates"));
        Assert.Equal(
            ["empty", "validation-error", "unauthorized", "session-expired"],
            Strings(root, "notApplicableStates"));
        Assert.Equal(
            ["SERVICE_UNAVAILABLE", "MAINTENANCE", "WINDOW_CHANGED"],
            Strings(root, "reasonCodes"));

        var destinations = root.GetProperty("destinations");
        Assert.Equal("/student/login", destinations.GetProperty("studentLogin").GetString());
        Assert.Equal(
            "/student/activate",
            destinations.GetProperty("studentActivation").GetString());
        Assert.Equal("/staff/login", destinations.GetProperty("staffLogin").GetString());
    }

    [Fact]
    public void Public_gateway_response_is_the_exact_privacy_safe_six_field_contract()
    {
        var properties = typeof(PublicContextDto)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                "RegistrationTermLabel",
                "RegistrationWindowState",
                "ServerTimeUtc",
                "ServiceState",
                "TeachingTermLabel",
                "TimeZoneId"
            ],
            properties);
        Assert.DoesNotContain("DisplayName", properties);
        Assert.DoesNotContain("AuthorizedRoles", properties);
        Assert.DoesNotContain("Capacity", properties);
    }

    [Fact]
    public void Auth01_route_and_api_facade_bind_only_the_public_context_owner_contract()
    {
        var page = RequiredSource(PagePath, "SPEC-008/T076");
        var apiClient = RequiredSource(ApiClientPath, "SPEC-008/T076");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/\"",
            "Role gateway",
            "AcademicApiClient",
            "GetPublicContextAsync",
            "Student login",
            "Student activation",
            "Staff login",
            "Retry public context");
        RepositoryFiles.ContainsAll(
            apiClient,
            "GET",
            "/api/public/context",
            "GetPublicContextAsync",
            "PublicContextDto",
            "ApiError");
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", page, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("SERVICE_UNAVAILABLE", RouteUiState.ServiceError)]
    [InlineData("MAINTENANCE", RouteUiState.ServiceError)]
    [InlineData("WINDOW_CHANGED", RouteUiState.Stale)]
    [InlineData("AUTH01_UNKNOWN", RouteUiState.ServiceError)]
    public void Stable_and_unknown_reason_codes_map_without_inventing_success(
        string reasonCode,
        RouteUiState expectedState)
    {
        var mapper = new UiStateMapper(new KeyTextProvider());

        var result = mapper.Map(new UiStateInput(
            RouteUiState.Success,
            serverAccepted: false,
            reasonCode,
            "AUTH-01-REF",
            [new UiStatusAction("retry", "Retry public context", "/")]));

        Assert.Equal(expectedState, result.State);
        Assert.False(result.ServerAccepted);
        Assert.Equal(reasonCode, result.Status.Code);
        Assert.Equal("AUTH-01-REF", result.Status.ReferenceId);
    }

    private static string[] Strings(JsonElement owner, string propertyName) =>
        owner.GetProperty(propertyName)
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();

    private static string RequiredSource(string repositoryPath, string deliveryTask)
    {
        var path = RepositoryFiles.PathTo(repositoryPath);
        Assert.True(
            File.Exists(path),
            $"{repositoryPath} has not been delivered; this is the expected red for {deliveryTask}.");
        return File.ReadAllText(path);
    }

    private sealed class KeyTextProvider : IUiTextProvider
    {
        public string Get(string key) => key;
    }
}
