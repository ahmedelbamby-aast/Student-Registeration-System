using System.Reflection;
using System.Text.Json;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.UX;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StudentDashboardPageContractTests
{
    private const string FixturePath =
        "tests/StudentRegistration.Client.ContractTests/Fixtures/Spec008/STU-01/route-contract.json";
    private const string PagePath =
        "src/StudentRegistration.Client/Pages/StudentDashboardPage.razor";
    private const string ApiClientPath =
        "src/StudentRegistration.Client/Features/Academics/AcademicApiClient.cs";

    [Fact]
    public void Stu01_fixture_freezes_route_apis_all_states_reasons_journeys_and_destinations()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(FixturePath));
        var root = document.RootElement;

        Assert.Equal("frontend-fixture/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("STU-01", root.GetProperty("routeId").GetString());
        Assert.Equal("/student", root.GetProperty("routeTemplate").GetString());
        Assert.Equal("StudentDashboardPage.razor", root.GetProperty("pageName").GetString());
        Assert.Equal(
            [
                ("GET", "/api/context", "AppContextDto", "SPEC-008"),
                ("GET", "/api/students/me/academic-context", "StudentAcademicContextDto", "SPEC-008"),
                ("GET", "/api/student/registrations/current/timetable", "not-pinned", "SPEC-015")
            ],
            root.GetProperty("apis").EnumerateArray()
                .Select(api => (
                    api.GetProperty("method").GetString(),
                    api.GetProperty("path").GetString(),
                    api.GetProperty("successType").GetString(),
                    api.GetProperty("owner").GetString()))
                .ToArray());
        Assert.Equal(
            [
                "loading", "empty", "success", "validation-error", "service-error",
                "unauthorized", "session-expired", "stale", "offline"
            ],
            Strings(root, "requiredStates"));

        var reasons = root.GetProperty("reasonMappings");
        Assert.Equal("validation-error", reasons.GetProperty("REGISTRATION_HOLD").GetString());
        Assert.Equal("validation-error", reasons.GetProperty("PROFILE_NOT_READY").GetString());
        Assert.Equal("stale", reasons.GetProperty("WINDOW_CLOSED").GetString());
        Assert.Equal("unauthorized", reasons.GetProperty("UNAUTHORIZED").GetString());
        Assert.Equal("session-expired", reasons.GetProperty("SESSION_EXPIRED").GetString());
        Assert.Equal("service-error", reasons.GetProperty("CONTEXT_UNAVAILABLE").GetString());
        Assert.Equal("service-error", root.GetProperty("unknownReasonFallback").GetString());

        Assert.Equal(
            [
                "STU-01-open-v1", "STU-01-upcoming-v1", "STU-01-closed-v1",
                "STU-01-no-term-v1", "STU-01-hold-v1",
                "STU-01-incomplete-profile-v1"
            ],
            root.GetProperty("journeys").EnumerateArray()
                .Select(journey => journey.GetProperty("fixture").GetString()!)
                .ToArray());

        var destinations = root.GetProperty("destinations");
        Assert.Equal("/student/subjects", destinations.GetProperty("start").GetString());
        Assert.Equal("/student/schedule", destinations.GetProperty("resume").GetString());
        Assert.Equal(
            "/student/registrations",
            destinations.GetProperty("registrations").GetString());
        Assert.Equal("/student/account", destinations.GetProperty("account").GetString());
        Assert.Equal(
            "unavailable",
            root.GetProperty("downstreamTimetable").GetProperty("regionState").GetString());
        Assert.True(
            root.GetProperty("downstreamTimetable")
                .GetProperty("mustNotClaimLiveSuccess")
                .GetBoolean());
    }

    [Fact]
    public void Student_academic_response_is_the_canonical_bounded_owner_contract()
    {
        var properties = typeof(StudentAcademicContextDto)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                "ActiveHolds", "Cohort", "CurrentGpa", "DataAsOfUtc", "DataVersion",
                "EarnedCredits", "ProgramCode", "Provenance", "Standing",
                "TranscriptAttempts", "TranscriptSummary", "UniversityId"
            ],
            properties);
        Assert.DoesNotContain("Password", properties);
        Assert.DoesNotContain("ApplicationUserId", properties);
        Assert.DoesNotContain("StudentRowVersion", properties);
    }

    [Fact]
    public void Academic_client_exposes_only_the_two_available_authenticated_reads_for_stu01()
    {
        var apiClient = RepositoryFiles.Read(ApiClientPath);

        RepositoryFiles.ContainsAll(
            apiClient,
            "AppContextMethod",
            "AppContextPath",
            "\"/api/context\"",
            "GetAppContextAsync",
            "AcademicContextMethod",
            "AcademicContextPath",
            "\"/api/students/me/academic-context\"",
            "GetStudentAcademicContextAsync",
            "AppContextDto",
            "StudentAcademicContextDto");
        Assert.DoesNotContain(
            "GetCurrentTimetableAsync",
            apiClient,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Stu01_route_binds_authoritative_context_and_keeps_timetable_contributor_honest()
    {
        var page = RequiredSource(PagePath, "SPEC-008/T078");

        RepositoryFiles.ContainsAll(
            page,
            "@page \"/student\"",
            "Student dashboard",
            "AcademicApiClient",
            "GetAppContextAsync",
            "GetStudentAcademicContextAsync",
            "STU-01-CONTRACT-T148",
            "data-route-id=\"STU-01\"",
            "Current timetable is unavailable until SPEC-015 is delivered.",
            "/api/student/registrations/current/timetable",
            "data-testid=\"current-timetable-unavailable\"");
        Assert.DoesNotContain("DateTime.Now", page, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.UtcNow", page, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sample timetable", page, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mock timetable", page, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("REGISTRATION_HOLD", RouteUiState.ValidationError)]
    [InlineData("PROFILE_NOT_READY", RouteUiState.ValidationError)]
    [InlineData("WINDOW_CLOSED", RouteUiState.Stale)]
    [InlineData("STALE_VERSION", RouteUiState.Stale)]
    [InlineData("WINDOW_CHANGED", RouteUiState.Stale)]
    [InlineData("UNAUTHORIZED", RouteUiState.Unauthorized)]
    [InlineData("FORBIDDEN", RouteUiState.Unauthorized)]
    [InlineData("SESSION_EXPIRED", RouteUiState.SessionExpired)]
    [InlineData("CONTEXT_UNAVAILABLE", RouteUiState.ServiceError)]
    [InlineData("SERVICE_UNAVAILABLE", RouteUiState.ServiceError)]
    [InlineData("MAINTENANCE", RouteUiState.ServiceError)]
    [InlineData("STU01_UNKNOWN", RouteUiState.ServiceError)]
    public void Stable_and_unknown_stu01_reasons_preserve_code_and_never_invent_success(
        string reasonCode,
        RouteUiState expectedState)
    {
        var mapper = new UiStateMapper(new KeyTextProvider());

        var result = mapper.Map(new UiStateInput(
            RouteUiState.Success,
            serverAccepted: false,
            reasonCode,
            "STU-01-SAFE-REF",
            [new UiStatusAction("refresh", "Refresh dashboard", "/student")]));

        Assert.Equal(expectedState, result.State);
        Assert.False(result.ServerAccepted);
        Assert.Equal(reasonCode, result.Status.Code);
        Assert.Equal("STU-01-SAFE-REF", result.Status.ReferenceId);
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
