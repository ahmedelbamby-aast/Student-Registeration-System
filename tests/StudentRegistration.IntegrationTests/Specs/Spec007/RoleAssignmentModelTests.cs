using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class RoleAssignmentModelTests
{
    [Fact]
    public void Role_assignment_has_bounded_effective_scope_actor_and_rowversion()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Domain/RoleAssignment.cs");

        RepositoryFiles.ContainsAll(
            source,
            "public sealed class RoleAssignment",
            "public Guid ApplicationUserId { get;",
            "public string RoleCode { get;",
            "public DateTime EffectiveFromUtc { get;",
            "public DateTime? EffectiveToUtc { get;",
            "public string AssignedByReference { get;",
            "public byte[] Version { get;",
            "IsEffectiveAt");
        Assert.DoesNotContain("ClaimsPrincipal", source, StringComparison.Ordinal);
    }
}
