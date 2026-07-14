using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Identity;

public sealed class IdentityRouteStateTests
{
    [Fact]
    public void Identity_route_mapper_covers_every_approved_page_state()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Features/Identity/IdentityRouteStateMapper.cs");

        RepositoryFiles.ContainsAll(
            source,
            "IdentityRouteStateMapper",
            "AUTH-02",
            "AUTH-03",
            "AUTH-04",
            "AUTH-05",
            "STU-08",
            "role-selection-required",
            "expired",
            "locked",
            "validation");
        Assert.DoesNotContain("ClaimsPrincipal", source, StringComparison.Ordinal);
        Assert.DoesNotContain("localStorage", source, StringComparison.OrdinalIgnoreCase);
    }
}
