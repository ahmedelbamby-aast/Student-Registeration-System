using System.Reflection;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_9Tests
{
    [Fact]
    public void Frozen_contract_contains_the_complete_adm02_and_adm04_owner_journeys()
    {
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");
        RepositoryFiles.ContainsAll(
            contract,
            "04 | `GET /api/admin/terms`",
            "05 | `POST /api/admin/terms`",
            "06 | `PUT /api/admin/terms/{termId}`",
            "07 | `POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish`",
            "08 | `GET /api/admin/students`",
            "09 | `GET /api/admin/students/{studentId}/academic-context`",
            "10 | `PATCH /api/admin/students/{studentId}/academic-profile`",
            "Default sort is `code,id`",
            "Required `termId` and query (3..50)",
            "`AcademicTerms.Manage`",
            "`AcademicProfiles.Manage`",
            "409 STALE_VERSION",
            "Transient failures commit no partial state",
            "atomic privacy-safe audit");
        Assert.DoesNotContain("/api/admin/audit", contract, StringComparison.Ordinal);

        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("specs/008-academic-term-student-profile/requirements.md"),
            "AC-9: Complete Admin term and profile journeys",
            "bounded response or audited mutation",
            "nothing and return their stable reason",
            "never runs without a term ID plus bounded search query");
    }

    [Fact]
    public void Commands_expose_only_the_required_versions_provenance_and_bounded_operations()
    {
        var contracts = Assembly.Load("StudentRegistration.Contracts");

        HasExactProperties(
            RequiredType(
                contracts,
                "StudentRegistration.Contracts.Academics.CreateTermRequest"),
            "ClientRequestId", "Reason", "Source", "Term", "Windows");
        HasExactProperties(
            RequiredType(
                contracts,
                "StudentRegistration.Contracts.Academics.UpdateTermRequest"),
            "ExpectedTermRowVersion", "ExpectedWindowRowVersions", "Reason",
            "Source", "Term", "Windows");
        HasExactProperties(
            RequiredType(
                contracts,
                "StudentRegistration.Contracts.Academics.PublishRegistrationWindowRequest"),
            "ExpectedTermRowVersion", "ExpectedWindowRowVersion", "Reason", "Source");
        HasExactProperties(
            RequiredType(
                contracts,
                "StudentRegistration.Contracts.Academics.AcademicProfileCorrectionRequest"),
            "TermId", "ExpectedStudentRowVersion", "ExpectedStudentTermStateRowVersion",
            "Reason", "Source", "Operations");
    }

    [Fact]
    public void Owner_handlers_use_independent_permissions_and_atomic_stale_safe_services()
    {
        var academics = Assembly.Load("StudentRegistration.Academics");
        var endpoints = RequiredType(
            academics,
            "StudentRegistration.Academics.Endpoints.Spec008Endpoints");
        _ = RequiredType(
            academics,
            "StudentRegistration.Academics.Application.RegistrationWindowService");
        _ = RequiredType(
            academics,
            "StudentRegistration.Academics.Application.AdminAcademicManagementService");
        _ = RequiredType(
            academics,
            "StudentRegistration.Academics.Application.StudentAcademicProfileService");
        Assert.NotNull(
            endpoints.GetMethod("MapSpec008Endpoints", BindingFlags.Static | BindingFlags.Public));

        var endpointSource = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Endpoints/Spec008Endpoints.cs");
        RepositoryFiles.ContainsAll(
            endpointSource,
            "/api/admin/terms",
            "/api/admin/terms/{termId}",
            "/api/admin/terms/{termId}/registration-windows/{windowId}/publish",
            "/api/admin/students",
            "/api/admin/students/{studentId}/academic-context",
            "/api/admin/students/{studentId}/academic-profile",
            "AcademicTerms.Manage",
            "AcademicProfiles.Manage",
            "RequireAntiforgery",
            "STALE_VERSION",
            "WINDOW_OVERLAP");
        Assert.DoesNotContain("/api/admin/audit", endpointSource, StringComparison.Ordinal);

        var termService = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/RegistrationWindowService.cs");
        RepositoryFiles.ContainsAll(
            termService,
            "ExpectedTermRowVersion",
            "ExpectedWindowRowVersion",
            "ExpectedWindowRowVersions",
            "CreationClientRequestId",
            "Audit",
            "StaleVersion",
            "WindowOverlap");

        var adminService = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/AdminAcademicManagementService.cs");
        RepositoryFiles.ContainsAll(
            adminService,
            "DefaultPageSize = 20",
            "MaximumPageSize = 100",
            "MinimumQueryLength = 3",
            "MaximumQueryLength = 50");

        var profileService = RepositoryFiles.Read(
            "src/StudentRegistration.Academics/Application/StudentAcademicProfileService.cs");
        RepositoryFiles.ContainsAll(
            profileService,
            "TermId",
            "ExpectedStudentRowVersion",
            "ExpectedStudentTermStateRowVersion",
            "MaximumCorrectionOperations = 20",
            "Audit",
            "StaleVersion");
    }

    private static Type RequiredType(Assembly assembly, string fullName)
    {
        var type = assembly.GetType(fullName);
        Assert.True(type is not null, $"Required AC-9 runtime type has not been delivered: {fullName}");
        return type!;
    }

    private static void HasExactProperties(Type type, params string[] expected)
    {
        Assert.Equal(
            expected.Order(StringComparer.Ordinal),
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Empty(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
    }
}
