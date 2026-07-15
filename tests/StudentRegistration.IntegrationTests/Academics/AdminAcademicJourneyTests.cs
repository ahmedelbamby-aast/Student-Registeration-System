using System.Reflection;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Academics;

public sealed class AdminAcademicJourneyTests
{
    private const string ContractPath =
        "specs/008-academic-term-student-profile/contracts/api.md";
    private const string ServicePath =
        "src/StudentRegistration.Academics/Application/AdminAcademicManagementService.cs";

    [Fact]
    public void Frozen_admin_lists_are_bounded_scoped_and_deterministically_sorted()
    {
        var contract = RepositoryFiles.Read(ContractPath);

        RepositoryFiles.ContainsAll(
            contract,
            "Shared pages default to page 1/size 20 and permit at most 100 items.",
            "Optional query (3..50), page, pageSize, state and allow-listed sort",
            "Default sort is `code,id`",
            "Required `termId` and query (3..50), page/pageSize and allow-listed sort",
            "Default sort is `universityId,studentId`",
            "it is not an unrestricted student dump");

        var source = RepositoryFiles.Read(ServicePath);
        RepositoryFiles.ContainsAll(
            source,
            "DefaultPageSize = 20",
            "MaximumPageSize = 100",
            "MinimumSearchLength = 3",
            "MaximumSearchLength = 50",
            "ListTerms",
            "ListStudents",
            "ThenBy");
    }

    [Fact]
    public void Owner_commands_preserve_provenance_versions_and_exact_permission_partition()
    {
        var contract = RepositoryFiles.Read(ContractPath);

        RepositoryFiles.ContainsAll(
            contract,
            "CreateTermRequest",
            "clientRequestId",
            "UpdateTermRequest",
            "expectedTermRowVersion",
            "expectedWindowRowVersions",
            "PublishRegistrationWindowRequest",
            "expectedWindowRowVersion",
            "AcademicProfileCorrectionRequest",
            "expectedStudentRowVersion",
            "expectedStudentTermStateRowVersion",
            "reason",
            "source",
            "Term commands require `AcademicTerms.Manage`",
            "profile commands require the separately governed `AcademicProfiles.Manage` permission");

        var service = RequiredAdminService();
        var publicMethods = service.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        Assert.Contains(publicMethods, method => method.Name.Contains("ListTerms", StringComparison.Ordinal));
        Assert.Contains(publicMethods, method => method.Name.Contains("ListStudents", StringComparison.Ordinal));
        Assert.Contains(publicMethods, method => method.Name.Contains("Term", StringComparison.Ordinal));
        Assert.Contains(publicMethods, method => method.Name.Contains("Profile", StringComparison.Ordinal));
    }

    [Fact]
    public void Admin_service_depends_on_narrow_application_ports_not_ef_or_infrastructure()
    {
        var service = RequiredAdminService();
        var constructor = Assert.Single(service.GetConstructors());

        Assert.Contains(
            constructor.GetParameters(),
            parameter => parameter.ParameterType.IsInterface &&
                parameter.ParameterType.Namespace ==
                    "StudentRegistration.Academics.Application.Ports");
        Assert.DoesNotContain(
            constructor.GetParameters(),
            parameter =>
                (parameter.ParameterType.Namespace ?? string.Empty).Contains(
                    "Infrastructure",
                    StringComparison.Ordinal) ||
                parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));

        var source = RepositoryFiles.Read(ServicePath);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.SqlServer", source, StringComparison.Ordinal);
        RepositoryFiles.ContainsAll(
            source,
            "Application.Ports",
            "CancellationToken");
    }

    [Fact]
    public void Existing_owner_suites_remain_the_single_atomic_rejection_evidence()
    {
        var windowSuite = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Academics/RegistrationWindowConcurrencyTests.cs");
        var profileSuite = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Academics/ProfileHoldConcurrencyTests.cs");
        var contract = RepositoryFiles.Read(ContractPath);

        RepositoryFiles.ContainsAll(
            windowSuite,
            "StaleVersion",
            "WindowOverlap",
            "changes_nothing",
            "AuditEntries",
            "CommitCount");
        RepositoryFiles.ContainsAll(
            profileSuite,
            "both_versions",
            "stale_input_changes_nothing",
            "SuccessfulAuditCount");
        RepositoryFiles.ContainsAll(
            contract,
            "Cancellation before commit rolls back audit and every mutation.",
            "Transient failures commit no partial state.",
            "atomic privacy-safe audit");
    }

    private static Type RequiredAdminService()
    {
        var service = Assembly.Load("StudentRegistration.Academics").GetType(
            "StudentRegistration.Academics.Application.AdminAcademicManagementService");
        Assert.True(
            service is not null,
            "AdminAcademicManagementService has not delivered the bounded Admin owner journey.");
        return service!;
    }
}
