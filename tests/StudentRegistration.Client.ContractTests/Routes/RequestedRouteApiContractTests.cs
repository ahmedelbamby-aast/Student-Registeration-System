using System.Reflection;
using StudentRegistration.Client.Features.Registration;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.ContractTests.Routes;

public sealed class StudentRoadmapRouteApiContractTests
{
    [Fact]
    public void Stu_09_uses_the_typed_self_roadmap_read_contract()
    {
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Academics/CatalogueApiClient.cs");
        RepositoryFiles.ContainsAll(client, "StudentRoadmapPath = \"/api/students/me/roadmap\"",
            "Task<AcademicApiResult<StudentRoadmapDto>> GetStudentRoadmapAsync");
        Assert.Equal(["CatalogueVersion", "Cohort", "ProgramCode", "Terms"],
            typeof(StudentRoadmapDto).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name).Order(StringComparer.Ordinal).ToArray());
    }
}

public sealed class AdminApprovalRouteApiContractTests
{
    [Fact]
    public void Adm_10_uses_admin_queue_and_version_bound_decision_contracts() =>
        AssertApprovalContract("/api/admin/registration-approvals");

    internal static void AssertApprovalContract(string path)
    {
        var client = RepositoryFiles.Read("src/StudentRegistration.Client/Features/Registration/RegistrationApprovalApiClient.cs");
        RepositoryFiles.ContainsAll(client, path, "state=pending&page={page}&pageSize={pageSize}",
            "ExpectedSubmissionRowVersion", "ExpectedLineRowVersion", "ClientRequestId", "X-XSRF-TOKEN");
        Assert.Contains(nameof(RegistrationApprovalPageDto.Items),
            typeof(RegistrationApprovalPageDto).GetProperties().Select(property => property.Name));
    }
}

public sealed class StaffApprovalRouteApiContractTests
{
    [Fact]
    public void Stf_05_uses_staff_queue_and_the_same_version_bound_decision_contract() =>
        AdminApprovalRouteApiContractTests.AssertApprovalContract("/api/staff/registration-approvals");
}
