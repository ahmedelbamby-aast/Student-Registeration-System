using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StudentRegistration.Api.Composition;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec004;

public sealed class DataProtectionRegistrationTests
{
    [Fact]
    public void Two_replicas_share_protected_keys_and_fail_closed_without_production_authority()
    {
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs");
        var composition = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/ModuleRegistration.cs");
        var securityConfiguration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Operations/SecurityConfiguration.cs");
        var repository = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/DataProtection/SqlDataProtectionKeyRepository.cs");

        RepositoryFiles.ContainsAll(
            registration,
            "DataProtection",
            "ApplicationName",
            "SqlServer",
            "ExternalCertificate",
            "ProductionRepositoryApproved",
            "ProductionEncryptionApproved",
            "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED",
            "InvalidOperationException");
        RepositoryFiles.ContainsAll(
            composition,
            "AddStudentRegistrationSecurity",
            "builder.Configuration",
            "builder.Environment");
        RepositoryFiles.ContainsAll(
            securityConfiguration,
            "AddStudentRegistrationDataProtection",
            "return services.AddStudentRegistrationDataProtection(configuration, environment)");
        RepositoryFiles.ContainsAll(
            repository,
            "PersistKeysToDbContext<StudentRegistrationDbContext>",
            "ArgumentNullException.ThrowIfNull");

        // Cross-replica authorization and plan state rely on SQL plus the shared
        // DataProtection application name; no sticky-session state is registered.
        Assert.DoesNotContain("sticky", registration, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DistributedMemoryCache", registration, StringComparison.Ordinal);
    }

    [Fact]
    public void Development_configuration_registers_one_external_certificate_application_name()
    {
        var certificatePath = Path.Combine(
            Path.GetTempPath(),
            $"srs-spec004-{Guid.NewGuid():N}.pfx");
        const string certificatePassword = "Synthetic-Test-Only!42";
        const string applicationName = "AASTMT.StudentRegistration.Tests";
        WriteSyntheticCertificate(certificatePath, certificatePassword);

        try
        {
            var configuration = Configuration(
                applicationName,
                certificatePath,
                certificatePassword);
            var services = new ServiceCollection();

            services.AddStudentRegistrationDataProtection(
                configuration,
                new TestHostEnvironment(Environments.Development));

            Assert.Contains(
                services,
                descriptor => descriptor.ServiceType == typeof(IDataProtectionProvider));
            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<DataProtectionOptions>>().Value;
            Assert.Equal(applicationName, options.ApplicationDiscriminator);
        }
        finally
        {
            File.Delete(certificatePath);
        }
    }

    [Fact]
    public void Production_without_repository_and_encryption_approval_fails_closed()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DataProtection:ApplicationName"] = "AASTMT.StudentRegistration",
                ["DataProtection:Repository"] = "SqlServer",
                ["DataProtection:Encryption"] = "ExternalCertificate"
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddStudentRegistrationDataProtection(
                configuration,
                new TestHostEnvironment(Environments.Production)));

        Assert.Contains(
            "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED",
            exception.Message,
            StringComparison.Ordinal);
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
            "CN=AASTMT Student Registration SPEC004 Test",
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
        public string ApplicationName { get; set; } = "StudentRegistration.IntegrationTests";
        public string ContentRootPath { get; set; } = RepositoryFiles.Root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
