using Microsoft.Extensions.Hosting;
using StudentRegistration.Api.Composition;

namespace StudentRegistration.Api.Operations;

/// <summary>
/// Defines the bounded POC security-composition entry point.
/// </summary>
/// <remarks>
/// POC secret values come from .NET User Secrets or environment variables,
/// and the generated local certificate outside Git supplies key protection.
/// The production secret provider remains undecided and this entry point does
/// not grant production authority.
///
/// Generated student or staff plaintext is not accepted here. SPEC-007 owns
/// that boundary and may pass it only as transient input to the canonical
/// ASP.NET Identity credential hasher before discarding it.
/// </remarks>
public static class SecurityConfiguration
{
    private const string CertificatePathKey = "DataProtection:CertificatePath";
    private const string CertificatePasswordKey = "DataProtection:CertificatePassword";

    public static IServiceCollection AddStudentRegistrationSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        RequirePocSecretInput(configuration, CertificatePathKey);
        RequirePocSecretInput(configuration, CertificatePasswordKey);

        // Reuse the single SPEC-004 implementation so SQL key persistence,
        // external-certificate protection, and production authority remain
        // fail closed in one place.
        return services.AddStudentRegistrationDataProtection(configuration, environment);
    }

    private static void RequirePocSecretInput(
        IConfiguration configuration,
        string key)
    {
        if (string.IsNullOrWhiteSpace(configuration[key]))
        {
            throw new InvalidOperationException(
                $"POC_SECURITY_INPUT_REQUIRED: Missing {key}.");
        }
    }
}
