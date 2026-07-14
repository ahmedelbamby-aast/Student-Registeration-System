using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class StaffModelTests
{
    [Fact]
    public void Staff_is_pre_provisioned_and_links_exactly_one_application_user()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/Staff.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class Staff",
            "public Guid Id { get;",
            "public Guid ApplicationUserId { get;",
            "public string StaffNumber { get;",
            "public string DisplayName { get;",
            "public bool IsActive { get;");
        Assert.DoesNotContain("Password", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Role", source, StringComparison.Ordinal);
    }
}
