using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace StudentRegistration.IdentityAccess.Application;

/// <summary>
/// Versioned non-production credential baseline for the identity module.
/// </summary>
public sealed class IdentitySecurityOptions
{
    private const int DefaultMinimumPasswordLength = 15;
    private const int DefaultMaximumPasswordLength = 128;
    private const int DefaultMaximumFailures = 5;

    private static readonly HashSet<string> BlockedPasswords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "aastmt",
            "admin",
            "letmein",
            "password",
            "password123",
            "qwerty",
            "student",
            "welcome"
        };

    public PasswordHasherCompatibilityMode HasherCompatibilityMode { get; init; } =
        PasswordHasherCompatibilityMode.IdentityV3;

    public int PasswordHashIterations { get; init; } = 100_000;
    public int MinimumPasswordLength { get; init; } = DefaultMinimumPasswordLength;
    public int MaximumPasswordLength { get; init; } = DefaultMaximumPasswordLength;
    public int MaximumFailures { get; init; } = DefaultMaximumFailures;
    public TimeSpan LockoutDuration { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan ProofLifetime { get; init; } = TimeSpan.FromMinutes(15);
    public string PasswordBlocklistVersion { get; init; } = "DEMO-POC-2026.1";

    /// <summary>
    /// Must be disabled when an approved institutional policy replaces this demo baseline.
    /// </summary>
    public bool UseDemoCredentialPolicy { get; init; } = true;

    public string? ApprovedInstitutionalPolicyVersion { get; init; }

    public PasswordValidationResult ValidatePassword(
        string password,
        IEnumerable<string>? contextualTerms = null)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (password.Length < MinimumPasswordLength)
        {
            return PasswordValidationResult.Rejected("PASSWORD_TOO_SHORT");
        }

        if (password.Length > MaximumPasswordLength)
        {
            return PasswordValidationResult.Rejected("PASSWORD_TOO_LONG");
        }

        var normalized = password.Normalize(NormalizationForm.FormKC);
        if (BlockedPasswords.Contains(normalized))
        {
            return PasswordValidationResult.Rejected("PASSWORD_BLOCKED");
        }

        if (contextualTerms is not null
            && contextualTerms.Any(term => ContainsContextTerm(normalized, term)))
        {
            return PasswordValidationResult.Rejected("PASSWORD_CONTEXT_SPECIFIC");
        }

        return PasswordValidationResult.Accepted;
    }

    public bool IsPasswordAllowed(
        string password,
        IEnumerable<string>? contextualTerms = null) =>
        ValidatePassword(password, contextualTerms).IsAccepted;

    public void ValidateForEnvironment(string environmentName)
    {
        if (string.IsNullOrWhiteSpace(environmentName))
        {
            throw new ArgumentException(
                "An environment name is required.",
                nameof(environmentName));
        }

        ValidateBaseline();

        if (string.Equals(
                environmentName,
                Environments.Production,
                StringComparison.OrdinalIgnoreCase)
            && (UseDemoCredentialPolicy
                || string.IsNullOrWhiteSpace(ApprovedInstitutionalPolicyVersion)))
        {
            throw new InvalidOperationException(
                "IDENTITY_POLICY_NOT_APPROVED: Production requires an approved institutional credential policy.");
        }
    }

    private void ValidateBaseline()
    {
        if (HasherCompatibilityMode != PasswordHasherCompatibilityMode.IdentityV3
            || PasswordHashIterations < 100_000
            || MinimumPasswordLength < 15
            || MaximumPasswordLength < 64
            || MaximumPasswordLength < MinimumPasswordLength
            || MaximumFailures is < 1 or > 5
            || LockoutDuration < TimeSpan.FromMinutes(5)
            || ProofLifetime > TimeSpan.FromMinutes(15)
            || ProofLifetime <= TimeSpan.Zero
            || string.IsNullOrWhiteSpace(PasswordBlocklistVersion))
        {
            throw new InvalidOperationException(
                "IDENTITY_POLICY_INVALID: Credential settings weaken or invalidate the approved demo baseline.");
        }
    }

    private static bool ContainsContextTerm(string password, string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return false;
        }

        var normalizedTerm = term.Trim().Normalize(NormalizationForm.FormKC);
        return normalizedTerm.Length >= 4
            && password.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase);
    }
}

public readonly record struct PasswordValidationResult(bool IsAccepted, string? ErrorCode)
{
    public static PasswordValidationResult Accepted { get; } = new(true, null);

    public static PasswordValidationResult Rejected(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException("An error code is required.", nameof(errorCode));
        }

        return new PasswordValidationResult(false, errorCode);
    }
}
