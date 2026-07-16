namespace StudentRegistration.Registration.Domain;

public sealed record EligibilityReason
{
    public EligibilityReason(
        string code,
        bool passed,
        bool blocking,
        string message,
        string? requiredValue,
        string? currentValue,
        Guid policySetId,
        string policyVersion,
        string sourceReference,
        DateOnly sourceAccessedOn,
        string approvedBy,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        bool overridePossible,
        string? supportReferencePath)
    {
        if (blocking && overridePossible)
        {
            throw new ArgumentException(
                "A blocking demo decision cannot advertise an override.",
                nameof(overridePossible));
        }

        if (effectiveFromUtc.Kind is not DateTimeKind.Utc ||
            effectiveToUtc is { Kind: not DateTimeKind.Utc })
        {
            throw new ArgumentException("Policy effective instants must be UTC.");
        }

        if (effectiveToUtc is { } end && end <= effectiveFromUtc)
        {
            throw new ArgumentException(
                "Policy effective end must be after its start.",
                nameof(effectiveToUtc));
        }

        Code = EligibilityDomainGuard.Required(code, nameof(code));
        Passed = passed;
        Blocking = blocking;
        Message = EligibilityDomainGuard.Required(message, nameof(message));
        RequiredValue = EligibilityDomainGuard.Optional(requiredValue);
        CurrentValue = EligibilityDomainGuard.Optional(currentValue);
        PolicySetId = policySetId;
        PolicyVersion = EligibilityDomainGuard.Required(
            policyVersion,
            nameof(policyVersion));
        SourceReference = EligibilityDomainGuard.Required(
            sourceReference,
            nameof(sourceReference));
        SourceAccessedOn = sourceAccessedOn == default
            ? throw new ArgumentException(
                "A source access date is required.",
                nameof(sourceAccessedOn))
            : sourceAccessedOn;
        ApprovedBy = EligibilityDomainGuard.Required(
            approvedBy,
            nameof(approvedBy));
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        OverridePossible = overridePossible;
        SupportReferencePath = EligibilityDomainGuard.Optional(
            supportReferencePath);
    }

    public string Code { get; }

    public bool Passed { get; }

    public bool Blocking { get; }

    public string Message { get; }

    public string? RequiredValue { get; }

    public string? CurrentValue { get; }

    public Guid PolicySetId { get; }

    public string PolicyVersion { get; }

    public string SourceReference { get; }

    public DateOnly SourceAccessedOn { get; }

    public string ApprovedBy { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public bool OverridePossible { get; }

    public string? SupportReferencePath { get; }
}

internal static class EligibilityDomainGuard
{
    public static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException(
                "A non-empty value is required.",
                parameterName)
            : normalized;
    }

    public static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static IReadOnlyList<T> Copy<T>(
        IReadOnlyList<T> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        if (values.Any(value => value is null))
        {
            throw new ArgumentException(
                "A projection collection cannot contain null values.",
                parameterName);
        }

        return Array.AsReadOnly(values.ToArray());
    }
}
