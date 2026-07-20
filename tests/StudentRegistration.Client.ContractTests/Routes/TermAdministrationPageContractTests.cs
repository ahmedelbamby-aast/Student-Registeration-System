using System.Reflection;
using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class TermAdministrationPageContractTests
{
    private const string FixturePath =
        "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec008/ADM-02/route-contract.json";
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/TermAdministrationPage.razor";
    private const string ApiClientPath =
        "src/StudentRegistration.Client/Features/Academics/AcademicApiClient.cs";

    [Fact]
    public void Adm02_fixture_freezes_the_route_and_exact_T193_to_T196_endpoints()
    {
        using var document = Fixture();
        var root = document.RootElement;

        Assert.Equal("frontend-fixture/2.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("ADM-02", root.GetProperty("routeId").GetString());
        Assert.Equal("/admin/terms", root.GetProperty("routeTemplate").GetString());
        Assert.Equal("TermAdministrationPage.razor", root.GetProperty("pageName").GetString());

        AssertApi(
            root,
            "list",
            "GET",
            "/api/admin/terms",
            200,
            "Page<AdminTermDto>",
            [
                "PAGE_SIZE_INVALID", "VALIDATION_ERROR", "UNAUTHORIZED", "FORBIDDEN",
                "SERVICE_UNAVAILABLE", "INTERNAL_ERROR"
            ]);
        AssertApi(
            root,
            "create",
            "POST",
            "/api/admin/terms",
            201,
            "AdminTermDto",
            [
                "VALIDATION_ERROR", "TERM_CODE_EXISTS", "TERM_STATE_CONFLICT",
                "IDEMPOTENCY_KEY_REUSED", "UNAUTHORIZED", "FORBIDDEN",
                "SERVICE_UNAVAILABLE", "INTERNAL_ERROR"
            ]);
        AssertApi(
            root,
            "update",
            "PUT",
            "/api/admin/terms/{termId}",
            200,
            "AdminTermDto",
            [
                "VALIDATION_ERROR", "NOT_FOUND", "STALE_VERSION", "TERM_STATE_CONFLICT",
                "WINDOW_OVERLAP", "UNAUTHORIZED", "FORBIDDEN", "SERVICE_UNAVAILABLE",
                "INTERNAL_ERROR"
            ]);
        AssertApi(
            root,
            "publish",
            "POST",
            "/api/admin/terms/{termId}/registration-windows/{windowId}/publish",
            200,
            "AdminTermDto",
            [
                "VALIDATION_ERROR", "NOT_FOUND", "STALE_VERSION", "WINDOW_OVERLAP",
                "UNAUTHORIZED", "FORBIDDEN", "SERVICE_UNAVAILABLE", "INTERNAL_ERROR"
            ]);
    }

    [Fact]
    public void Adm02_fixture_freezes_all_nine_complete_states_focus_and_live_region_rules()
    {
        using var document = Fixture();
        var root = document.RootElement;

        Assert.Equal(
            [
                "loading", "empty", "success", "validation-error", "service-error",
                "unauthorized", "session-expired", "stale", "offline"
            ],
            Strings(root, "requiredStates"));

        var actual = root.GetProperty("stateFixtures")
            .EnumerateArray()
            .Select(state => new StateFixture(
                state.GetProperty("id").GetString()!,
                state.GetProperty("state").GetString()!,
                state.GetProperty("testId").GetString()!,
                state.GetProperty("focus").GetString()!,
                state.GetProperty("liveRegion").GetString()!))
            .ToArray();

        Assert.Equal(
            [
                new("ADM-02-loading-v1", "loading", "ADM-02-COMP-STATE-LOADING", "preserve-current-or-initial-heading", "polite"),
                new("ADM-02-empty-v1", "empty", "ADM-02-COMP-STATE-EMPTY", "empty-heading", "none"),
                new("ADM-02-success-v1", "success", "ADM-02-COMP-STATE-SUCCESS", "page-heading-unless-user-set-focus", "polite"),
                new("ADM-02-validation-error-v1", "validation-error", "ADM-02-COMP-STATE-VALIDATION-ERROR", "validation-summary", "assertive"),
                new("ADM-02-service-error-v1", "service-error", "ADM-02-COMP-STATE-SERVICE-ERROR", "preserve-background-or-heading-after-navigation-failure", "polite"),
                new("ADM-02-unauthorized-v1", "unauthorized", "ADM-02-COMP-STATE-UNAUTHORIZED", "denied-heading", "assertive"),
                new("ADM-02-session-expired-v1", "session-expired", "ADM-02-COMP-STATE-SESSION-EXPIRED", "session-expired-heading", "assertive"),
                new("ADM-02-stale-v1", "stale", "ADM-02-COMP-STATE-STALE", "preserve-current-or-heading-after-submitted-command", "polite"),
                new("ADM-02-offline-v1", "offline", "ADM-02-COMP-STATE-OFFLINE", "preserve-current", "polite")
            ],
            actual);
    }

    [Fact]
    public void Adm02_fixture_freezes_create_invalid_overlap_stale_and_publish_journeys()
    {
        using var document = Fixture();
        var journeys = document.RootElement.GetProperty("journeys")
            .EnumerateArray()
            .Select(journey => new JourneyFixture(
                journey.GetProperty("id").GetString()!,
                journey.GetProperty("testId").GetString()!,
                journey.GetProperty("outcome").GetString()!))
            .ToArray();

        Assert.Equal(
            [
                new("ADM-02-create-v1", "ADM-02-E2E-PRIMARY", "server-accepted-created-term"),
                new("ADM-02-invalid-dates-v1", "ADM-02-E2E-FAILURE", "linked-validation-summary-no-command"),
                new("ADM-02-overlap-v1", "ADM-02-E2E-FAILURE", "all-conflicting-windows-no-publish"),
                new("ADM-02-concurrent-edit-v1", "ADM-02-E2E-FAILURE", "stale-review-safe-draft-preserved"),
                new("ADM-02-publish-v1", "ADM-02-E2E-PRIMARY", "server-accepted-published-window")
            ],
            journeys);

        Assert.Equal(
            [
                "ADM-02-CONTRACT-T193", "ADM-02-COMP-T194", "ADM-02-E2E-PRIMARY",
                "ADM-02-E2E-FAILURE", "ADM-02-A11Y-T195", "ADM-02-VIS-T196"
            ],
            Strings(document.RootElement, "testIds"));
    }

    [Fact]
    public void Adm02_mutations_freeze_reason_XSRF_idempotency_and_concurrency_rules()
    {
        using var document = Fixture();
        var rules = document.RootElement.GetProperty("mutationRules");

        Assert.Equal(10, rules.GetProperty("reasonMinimumLength").GetInt32());
        Assert.Equal(500, rules.GetProperty("reasonMaximumLength").GetInt32());
        Assert.Equal(200, rules.GetProperty("sourceMaximumLength").GetInt32());
        Assert.Equal(20, rules.GetProperty("maximumWindows").GetInt32());
        Assert.Equal("X-XSRF-TOKEN", rules.GetProperty("antiforgeryHeader").GetString());
        Assert.Equal(
            "StudentRegistration.antiforgery.getRequestToken",
            rules.GetProperty("antiforgeryInterop").GetString());
        Assert.True(rules.GetProperty("createClientRequestIdRequired").GetBoolean());
        Assert.True(rules.GetProperty("expectedTermRowVersionRequired").GetBoolean());
        Assert.True(rules.GetProperty("expectedWindowRowVersionsRequired").GetBoolean());
        Assert.True(rules.GetProperty("serverAcceptedRequiredForSuccess").GetBoolean());
        Assert.False(rules.GetProperty("browserClockAuthoritative").GetBoolean());
        Assert.True(rules.GetProperty("safeDraftPreservedOnStale").GetBoolean());
        Assert.True(rules.GetProperty("boundedAggregateRefetchOnStale").GetBoolean());

        AssertProperties<CreateTermRequest>("ClientRequestId", "Reason", "Source", "Term", "Windows");
        AssertProperties<UpdateTermRequest>(
            "ExpectedTermRowVersion", "ExpectedWindowRowVersions", "Reason", "Source", "Term", "Windows");
        AssertProperties<PublishRegistrationWindowRequest>(
            "ExpectedTermRowVersion", "ExpectedWindowRowVersion", "Reason", "Source");
        Assert.True(typeof(Page<>).IsGenericTypeDefinition);
    }

    [Fact]
    public void T080_page_binds_the_approved_route_states_reasons_and_server_authority()
    {
        var page = RequiredSource(PagePath, "SPEC-008/T080");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/admin/terms\"",
            "ADM-02",
            "Term administration",
            "AcademicApiClient",
            "ListAdminTermsAsync",
            "CreateAdminTermAsync",
            "UpdateAdminTermAsync",
            "PublishAdminRegistrationWindowAsync",
            "ADM-02-COMP-STATE-LOADING",
            "ADM-02-COMP-STATE-EMPTY",
            "ADM-02-COMP-STATE-SUCCESS",
            "ADM-02-COMP-STATE-VALIDATION-ERROR",
            "ADM-02-COMP-STATE-SERVICE-ERROR",
            "ADM-02-COMP-STATE-UNAUTHORIZED",
            "ADM-02-COMP-STATE-SESSION-EXPIRED",
            "ADM-02-COMP-STATE-STALE",
            "ADM-02-COMP-STATE-OFFLINE",
            "VALIDATION_ERROR",
            "TERM_CODE_EXISTS",
            "TERM_STATE_CONFLICT",
            "IDEMPOTENCY_KEY_REUSED",
            "STALE_VERSION",
            "WINDOW_OVERLAP");
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", page, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("queued success", page, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void T080_api_facade_owns_the_exact_paths_XSRF_and_server_response_boundary()
    {
        var source = RequiredSource(ApiClientPath, "SPEC-008/T080");

        RepositoryFiles.ContainsAll(
            source,
            "ListAdminTermsAsync",
            "CreateAdminTermAsync",
            "UpdateAdminTermAsync",
            "PublishAdminRegistrationWindowAsync",
            "/api/admin/terms",
            "/registration-windows/",
            "/publish",
            "X-XSRF-TOKEN",
            "StudentRegistration.antiforgery.getRequestToken",
            "CreateTermRequest",
            "UpdateTermRequest",
            "PublishRegistrationWindowRequest",
            "Page<AdminTermDto>",
            "AdminTermDto",
            "ApiError");
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
    }

    private static JsonDocument Fixture() =>
        JsonDocument.Parse(RepositoryFiles.Read(FixturePath));

    private static void AssertApi(
        JsonElement root,
        string operation,
        string method,
        string path,
        int successStatus,
        string successType,
        string[] reasonCodes)
    {
        var api = Assert.Single(
            root.GetProperty("apis").EnumerateArray(),
            item => item.GetProperty("operation").GetString() == operation);
        Assert.Equal(method, api.GetProperty("method").GetString());
        Assert.Equal(path, api.GetProperty("path").GetString());
        Assert.Equal(successStatus, api.GetProperty("successStatus").GetInt32());
        Assert.Equal(successType, api.GetProperty("successType").GetString());
        Assert.Equal(reasonCodes, Strings(api, "reasonCodes"));
    }

    private static void AssertProperties<T>(params string[] expected) =>
        Assert.Equal(
            expected.Order(StringComparer.Ordinal),
            typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));

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

    private sealed record StateFixture(
        string Id,
        string State,
        string TestId,
        string Focus,
        string LiveRegion);

    private sealed record JourneyFixture(string Id, string TestId, string Outcome);
}
