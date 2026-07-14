using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StudentRegistration.Api.Operations;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SecurityTests;

public sealed class SecretAndDataProtectionTests
{
    [Fact]
    public void Missing_poc_certificate_secret_fails_closed_without_echoing_a_value()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataProtection:ApplicationName"] = "AASTMT.StudentRegistration.Tests",
                ["DataProtection:Repository"] = "SqlServer",
                ["DataProtection:Encryption"] = "ExternalCertificate",
                ["DataProtection:CertificatePath"] = Path.GetFullPath("missing-test-certificate.pfx")
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddStudentRegistrationSecurity(
                configuration,
                new TestHostEnvironment(Environments.Development)));

        Assert.Contains("POC_SECURITY_INPUT_REQUIRED", exception.Message, StringComparison.Ordinal);
        Assert.Contains("DataProtection:CertificatePassword", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("=", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_poc_replicas_register_the_existing_sql_backed_certificate_protected_key_ring()
    {
        var certificatePath = Path.Combine(
            Path.GetTempPath(),
            $"srs-spec018-{Guid.NewGuid():N}.pfx");
        const string certificatePassword = "Synthetic-Certificate-Test-Only!42";
        const string applicationName = "AASTMT.StudentRegistration.SecurityTests";
        WriteSyntheticCertificate(certificatePath, certificatePassword);

        try
        {
            var configuration = Configuration(
                applicationName,
                certificatePath,
                certificatePassword);
            var discriminators = new List<string?>();

            for (var replica = 0; replica < 2; replica++)
            {
                var services = new ServiceCollection();
                services.AddStudentRegistrationSecurity(
                    configuration,
                    new TestHostEnvironment(Environments.Development));

                Assert.Contains(
                    services,
                    descriptor => descriptor.ServiceType == typeof(IDataProtectionProvider));
                using var provider = services.BuildServiceProvider();
                discriminators.Add(
                    provider.GetRequiredService<IOptions<DataProtectionOptions>>()
                        .Value.ApplicationDiscriminator);
            }

            Assert.Equal(new[] { applicationName, applicationName }, discriminators);

            var securityConfiguration = RepositoryFiles.Read(
                "src/StudentRegistration.Api/Operations/SecurityConfiguration.cs");
            var existingRegistration = RepositoryFiles.Read(
                "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs");
            RepositoryFiles.ContainsAll(
                securityConfiguration,
                "AddStudentRegistrationDataProtection",
                "User Secrets",
                "environment variables",
                "generated local certificate outside Git",
                "production secret provider remains undecided");
            RepositoryFiles.ContainsAll(
                existingRegistration,
                "PersistKeysToSqlServer",
                "ProtectKeysWithCertificate",
                "SetApplicationName");
            Assert.DoesNotContain(
                "PersistKeysToFileSystem",
                securityConfiguration,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "DistributedMemoryCache",
                securityConfiguration,
                StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(certificatePath);
        }
    }

    [Fact]
    public void Production_without_existing_repository_and_encryption_authority_remains_fail_closed()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataProtection:ApplicationName"] = "AASTMT.StudentRegistration",
                ["DataProtection:Repository"] = "SqlServer",
                ["DataProtection:Encryption"] = "ExternalCertificate",
                ["DataProtection:CertificatePath"] = Path.GetFullPath("unavailable-production.pfx"),
                ["DataProtection:CertificatePassword"] = "Transient-Test-Input"
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddStudentRegistrationSecurity(
                configuration,
                new TestHostEnvironment(Environments.Production)));

        Assert.Contains(
            "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Repository_tracks_no_poc_secret_or_certificate_material()
    {
        var trackedFiles = SecurityRepositoryScan.TrackedRelativePaths();
        var forbiddenNames = new[] { ".env", "secrets.json" };
        var forbiddenExtensions = new[] { ".pfx", ".p12", ".pem", ".key" };

        Assert.DoesNotContain(
            trackedFiles,
            path => forbiddenNames.Contains(
                Path.GetFileName(path),
                StringComparer.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            trackedFiles,
            path => forbiddenExtensions.Contains(
                Path.GetExtension(path),
                StringComparer.OrdinalIgnoreCase));

        foreach (var relativePath in trackedFiles.Where(path =>
                     Path.GetFileName(path).StartsWith("appsettings", StringComparison.OrdinalIgnoreCase) &&
                     Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)))
        {
            Assert.DoesNotContain(
                "CertificatePassword",
                RepositoryFiles.Read(relativePath),
                StringComparison.OrdinalIgnoreCase);
        }

        var ignore = RepositoryFiles.Read(".gitignore");
        RepositoryFiles.ContainsAll(
            ignore,
            ".env",
            "secrets.json",
            ".local/",
            "credentials/",
            "*.pfx",
            "*.p12",
            "*.pem",
            "*.key");
    }

    [Fact(Skip =
        "Activation condition: SPEC-007 must deliver executable cookie/session and security-stamp runtime before cross-replica authentication can be claimed.")]
    public void Cross_replica_authentication_session_survives_without_sticky_routing()
    {
    }

    [Fact(Skip =
        "Activation condition: SPEC-013 must deliver the executable protected option-token runtime before a cross-replica token round trip can be claimed.")]
    public void Cross_replica_protected_option_token_round_trip_succeeds()
    {
    }

    private static IConfiguration Configuration(
        string applicationName,
        string certificatePath,
        string certificatePassword) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataProtection:ApplicationName"] = applicationName,
                ["DataProtection:Repository"] = "SqlServer",
                ["DataProtection:Encryption"] = "ExternalCertificate",
                ["DataProtection:CertificatePath"] = certificatePath,
                ["DataProtection:CertificatePassword"] = certificatePassword
            })
            .Build();

    private static void WriteSyntheticCertificate(string path, string password)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=AASTMT Student Registration SPEC018 Test",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1));
        File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, password));
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "StudentRegistration.SecurityTests";
        public string ContentRootPath { get; set; } = RepositoryFiles.Root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
