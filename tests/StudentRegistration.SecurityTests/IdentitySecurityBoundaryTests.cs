using StudentRegistration.TestSupport;

namespace StudentRegistration.SecurityTests;

public sealed class IdentitySecurityBoundaryTests
{
    [Fact]
    public void Identity_cookie_and_antiforgery_configuration_is_strict_and_non_persistent()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/IdentitySecurityRegistration.cs");

        RepositoryFiles.ContainsAll(
            source,
            "__Host-StudentRegistration.Session",
            "HttpOnly = true",
            "CookieSecurePolicy.Always",
            "SameSiteMode.Strict",
            "TimeSpan.FromMinutes(60)",
            "SlidingExpiration = false",
            "XSRF-TOKEN",
            "X-XSRF-TOKEN",
            "UseAntiforgery");
        Assert.DoesNotContain("LocalStorage", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SessionStorage", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Recovery_delivery_port_is_provider_neutral_and_production_requires_an_approved_adapter()
    {
        var port = RepositoryFiles.Read(
            "src/StudentRegistration.IdentityAccess/Application/Ports/IAccountRecoveryProofDelivery.cs");
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/IdentitySecurityRegistration.cs");

        RepositoryFiles.ContainsAll(
            port,
            "public interface IAccountRecoveryProofDelivery",
            "DeliverAsync",
            "RecoveryProofDeliveryRequest",
            "CancellationToken");
        RepositoryFiles.ContainsAll(
            registration,
            "IAccountRecoveryProofDelivery",
            "Testing",
            "Development",
            "Production",
            "approved");
        Assert.DoesNotContain("challengeToken", registration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Identity_security_state_is_shared_durable_and_telemetry_remains_privacy_safe()
    {
        var mapping = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/IdentityAccessModelConfiguration.cs");
        var telemetry = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Operations/ObservabilityExtensions.cs");

        RepositoryFiles.ContainsAll(
            mapping,
            "SecurityStamp",
            "AccessFailedCount",
            "LockoutEndUtc",
            "AuthenticationAbuseState",
            "AccountRecoveryChallenge",
            "IsRowVersion");
        Assert.DoesNotContain("UniversityId", telemetry, StringComparison.Ordinal);
        Assert.DoesNotContain("UserName", telemetry, StringComparison.Ordinal);
        Assert.DoesNotContain("IPAddress", telemetry, StringComparison.Ordinal);
        Assert.DoesNotContain("challengeToken", telemetry, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Browser_sources_never_persist_identity_credentials_or_session_tokens()
    {
        var clientRoot = RepositoryFiles.PathTo("src/StudentRegistration.Client");
        var sourceFiles = Directory.EnumerateFiles(clientRoot, "*", SearchOption.AllDirectories)
            .Where(path =>
                !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                Path.GetExtension(path) is ".cs" or ".razor" or ".js")
            .ToArray();

        Assert.NotEmpty(sourceFiles);
        foreach (var path in sourceFiles)
        {
            var source = File.ReadAllText(path);
            Assert.DoesNotContain("localStorage.setItem", source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sessionStorage.setItem", source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Authorization: Bearer", source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Abuse_subject_hmac_key_is_shared_locally_ignored_and_external_in_production()
    {
        var providers = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/IdentityAbuseKeyProviders.cs");
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/IdentitySecurityRegistration.cs");
        var gitIgnore = RepositoryFiles.Read(".gitignore");

        RepositoryFiles.ContainsAll(
            providers,
            "RandomNumberGenerator.GetBytes",
            ".local",
            "identity-abuse-hmac.key",
            "FileMode.CreateNew",
            "AbuseSubjectHmacKey",
            "environment.IsProduction()",
            "IDENTITY_ABUSE_KEY_REQUIRED");
        RepositoryFiles.ContainsAll(
            registration,
            "DevelopmentIdentityAbuseKeyProvider",
            "TestingIdentityAbuseKeyProvider",
            "ConfiguredIdentityAbuseKeyProvider",
            "abuseKey.GetKey().Length < 32");
        Assert.Contains(".local/", gitIgnore, StringComparison.Ordinal);
        Assert.DoesNotContain("ILogger", providers, StringComparison.Ordinal);
    }
}
