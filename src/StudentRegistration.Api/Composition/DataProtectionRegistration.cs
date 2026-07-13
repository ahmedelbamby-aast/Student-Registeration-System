using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using StudentRegistration.Infrastructure.SqlServer.DataProtection;

namespace StudentRegistration.Api.Composition;

public static class DataProtectionRegistration
{
    private const string ConfigurationPrefix = "DataProtection:";
    private const string SqlServerRepository = "SqlServer";
    private const string ExternalCertificate = "ExternalCertificate";

    public static IServiceCollection AddStudentRegistrationDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        ValidateStorageAndEncryption(configuration);
        ValidateProductionAuthority(configuration, environment);

        var applicationName = Required(configuration, "ApplicationName");
        var certificatePath = Required(configuration, "CertificatePath");
        var certificatePassword = Required(configuration, "CertificatePassword");
        if (!Path.IsPathFullyQualified(certificatePath) || !File.Exists(certificatePath))
        {
            throw ConfigurationError("The external certificate is unavailable.");
        }

        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            certificatePassword,
            X509KeyStorageFlags.EphemeralKeySet);
        if (!certificate.HasPrivateKey)
        {
            certificate.Dispose();
            throw ConfigurationError("The external certificate has no private key.");
        }

        services.AddSingleton<X509Certificate2>(_ => certificate);
        // One application name makes the SQL key ring interoperable across replicas.
        services
            .AddDataProtection()
            .SetApplicationName(applicationName)
            .PersistKeysToSqlServer()
            .ProtectKeysWithCertificate(certificate);

        return services;
    }

    private static void ValidateStorageAndEncryption(IConfiguration configuration)
    {
        if (!string.Equals(
                configuration[$"{ConfigurationPrefix}Repository"],
                SqlServerRepository,
                StringComparison.Ordinal) ||
            !string.Equals(
                configuration[$"{ConfigurationPrefix}Encryption"],
                ExternalCertificate,
                StringComparison.Ordinal))
        {
            throw ConfigurationError(
                "Repository must be SqlServer and encryption must be ExternalCertificate.");
        }
    }

    private static void ValidateProductionAuthority(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        var repositoryApproved = IsTrue(configuration, "ProductionRepositoryApproved");
        var encryptionApproved = IsTrue(configuration, "ProductionEncryptionApproved");
        if (!repositoryApproved || !encryptionApproved)
        {
            throw new InvalidOperationException(
                "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED: " +
                "Security/DevOps approval is required for both repository and encryption settings.");
        }
    }

    private static string Required(IConfiguration configuration, string name)
    {
        var value = configuration[$"{ConfigurationPrefix}{name}"];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ConfigurationError($"Required setting is missing: {name}.");
        }

        return value;
    }

    private static bool IsTrue(IConfiguration configuration, string name) =>
        bool.TryParse(configuration[$"{ConfigurationPrefix}{name}"], out var value) && value;

    private static InvalidOperationException ConfigurationError(string detail) =>
        new($"DATA_PROTECTION_CONFIGURATION_INVALID: {detail}");
}
