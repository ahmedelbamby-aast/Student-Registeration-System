using System.Net;
using System.Net.Http.Json;
using StudentRegistration.Contracts.Registration;
using StudentRegistration.IdentityAccess.Application.Authorization;

namespace StudentRegistration.QualityTests.Specs.Spec015;

[Collection(Spec015SqlEvidenceCollection.Name)]
public sealed class NFR_2EvidenceTests(Spec015SqlEvidenceFixture fixture)
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Actual_endpoints_enforce_student_ownership_and_admin_role_scope_with_audit()
    {
        var otherPath = $"/api/student/registrations/{fixture.OtherSubmissionId:D}";
        using (var owner = fixture.CreateClient(
            fixture.TargetApplicationUserId,
            RolePolicies.Student,
            RolePolicies.RegistrationRecordsReadOwn))
        using (var crossOwner = await owner.GetAsync(otherPath))
        {
            Assert.Equal(HttpStatusCode.NotFound, crossOwner.StatusCode);
            Assert.DoesNotContain(
                "REG-OTHER",
                await crossOwner.Content.ReadAsStringAsync(),
                StringComparison.Ordinal);
        }

        var adminPath =
            $"/api/admin/students/{fixture.OtherStudentId:D}/terms/{fixture.PerformanceTermId:D}/registrations/{fixture.OtherSubmissionId:D}";
        using (var admin = fixture.CreateClient(
            Guid.Parse("15000000-0000-0000-0000-000000000501"),
            RolePolicies.Admin,
            RolePolicies.RegistrationRecordsRead))
        using (var response = await admin.GetAsync(adminPath))
        {
            response.EnsureSuccessStatusCode();
            var detail = await response.Content.ReadFromJsonAsync<RegistrationDetailDto>();
            Assert.Equal(fixture.OtherSubmissionId, detail!.Receipt!.SubmissionId);
        }
        Assert.Equal(1, await fixture.CountInspectionAuditsAsync(
            fixture.OtherStudentId,
            fixture.PerformanceTermId,
            "detail"));

        await AssertForbiddenAsync(RolePolicies.Lecturer, RolePolicies.ContextRead, adminPath);
        await AssertForbiddenAsync(RolePolicies.TeachingAssistant, RolePolicies.ContextRead, adminPath);
        await AssertForbiddenAsync(RolePolicies.Admin, string.Empty, adminPath);
        await AssertForbiddenAsync(RolePolicies.Student, string.Empty, otherPath);
    }

    private async Task AssertForbiddenAsync(
        string role,
        string permission,
        string path)
    {
        using var client = fixture.CreateClient(Guid.NewGuid(), role, permission);
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
