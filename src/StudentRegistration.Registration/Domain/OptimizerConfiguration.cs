using System.Globalization;

namespace StudentRegistration.Registration.Domain;

public sealed record OptimizerConfiguration
{
    private static readonly ScoreFactor[] CanonicalFactorOrder =
    [
        ScoreFactor.PreferenceViolations,
        ScoreFactor.IdleMinutes,
        ScoreFactor.TeachingDays,
        ScoreFactor.StableGroupTuple
    ];

    public OptimizerConfiguration(
        string version,
        IReadOnlyList<ScoreFactor> factorOrder,
        string approvalReference)
    {
        Version = SemanticVersion(version);
        ArgumentNullException.ThrowIfNull(factorOrder);

        var copiedFactorOrder = factorOrder.ToArray();
        foreach (var factor in copiedFactorOrder)
        {
            if (!Enum.IsDefined(factor))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(factorOrder),
                    factor,
                    "Every configured factor must be canonical.");
            }
        }

        if (!copiedFactorOrder.SequenceEqual(CanonicalFactorOrder))
        {
            throw new ArgumentException(
                "The MVP optimizer uses the approved lexicographic factor order.",
                nameof(factorOrder));
        }

        FactorOrder = Array.AsReadOnly(copiedFactorOrder);
        ApprovalReference = Required(
            approvalReference,
            nameof(approvalReference));
    }

    public string Version { get; }

    public IReadOnlyList<ScoreFactor> FactorOrder { get; }

    public string ApprovalReference { get; }

    public bool Equals(OptimizerConfiguration? other) =>
        other is not null &&
        string.Equals(Version, other.Version, StringComparison.Ordinal) &&
        FactorOrder.SequenceEqual(other.FactorOrder) &&
        string.Equals(
            ApprovalReference,
            other.ApprovalReference,
            StringComparison.Ordinal);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Version, StringComparer.Ordinal);
        foreach (var factor in FactorOrder)
        {
            hash.Add(factor);
        }

        hash.Add(ApprovalReference, StringComparer.Ordinal);
        return hash.ToHashCode();
    }

    private static string SemanticVersion(string? value)
    {
        var normalized = Required(value, nameof(value));
        var components = normalized.Split('.');
        if (components.Length != 3 ||
            components.Any(component =>
                !int.TryParse(
                    component,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var parsed) ||
                !string.Equals(
                    component,
                    parsed.ToString(CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "The optimizer version must use major.minor.patch semantic versioning.",
                nameof(value));
        }

        return normalized;
    }

    private static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException(
                "A non-empty value is required.",
                parameterName)
            : normalized;
    }
}
