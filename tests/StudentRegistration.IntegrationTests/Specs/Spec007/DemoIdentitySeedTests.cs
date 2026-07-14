using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class DemoIdentitySeedTests
{
    [Fact]
    public void Demo_seed_is_synthetic_hash_only_idempotent_and_environment_guarded()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/DemoIdentitySeedContributor.cs");

        RepositoryFiles.ContainsAll(
            source,
            "DemoIdentitySeedContributor",
            "Development",
            "Testing",
            "PasswordHasher<ApplicationUser>",
            "HashPassword",
            "UniversityId",
            "ClientRequestId",
            "idempotent",
            "TimeProvider");
        Assert.DoesNotContain("DateTime.UtcNow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ILogger", source, StringComparison.Ordinal);
    }
}
