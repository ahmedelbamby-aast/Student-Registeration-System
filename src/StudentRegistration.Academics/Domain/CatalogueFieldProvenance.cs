namespace StudentRegistration.Academics.Domain;

public enum CatalogueSourceKind
{
    OfficialSource = 1,
    SyntheticDemo = 2,
}

public sealed class CatalogueFieldProvenance
{
    private CatalogueFieldProvenance()
    {
    }

    public CatalogueFieldProvenance(
        string sourceReference,
        DateOnly accessedOn,
        CatalogueSourceKind sourceKind,
        IEnumerable<string> syntheticFields)
    {
        if (accessedOn == default)
        {
            throw new ArgumentException("An access date is required.", nameof(accessedOn));
        }

        if (!Enum.IsDefined(sourceKind))
        {
            throw new ArgumentOutOfRangeException(nameof(sourceKind));
        }

        SourceReference = DomainValue.Required(sourceReference, nameof(sourceReference));
        AccessedOn = accessedOn;
        SourceKind = sourceKind;
        SyntheticFields = (syntheticFields ?? throw new ArgumentNullException(nameof(syntheticFields)))
            .Select(field => DomainValue.Required(field, nameof(syntheticFields)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(field => field, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public string SourceReference { get; private set; } = string.Empty;

    public DateOnly AccessedOn { get; private set; }

    public CatalogueSourceKind SourceKind { get; private set; }

    public IReadOnlyList<string> SyntheticFields { get; private set; } = [];
}

internal static class DomainValue
{
    public static string Required(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return normalized;
    }

    public static string Code(string? value, string parameterName) =>
        Required(value, parameterName).Normalize().ToUpperInvariant();

    public static void Identifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }

    public static void Utc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC instant is required.", parameterName);
        }
    }
}
