using System.Collections.ObjectModel;

namespace StudentRegistration.Contracts.Operations;

public sealed record OperationalMetric
{
    public const int MaxNameLength = 128;
    public const int MaxDimensions = 8;
    public const int MaxDimensionKeyLength = 64;
    public const int MaxDimensionValueLength = 128;

    public OperationalMetric(
        string name,
        double value,
        DateTime observedAtUtc,
        IReadOnlyDictionary<string, string>? dimensions = null)
    {
        Name = ValidateName(name, nameof(name), MaxNameLength);

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "A finite metric value is required.");
        }

        if (observedAtUtc.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "The observation timestamp must use UTC.",
                nameof(observedAtUtc));
        }

        Value = value;
        ObservedAtUtc = observedAtUtc;
        Dimensions = CopyDimensions(dimensions);
    }

    public string Name { get; }

    public double Value { get; }

    public DateTime ObservedAtUtc { get; }

    public IReadOnlyDictionary<string, string> Dimensions { get; }

    private static IReadOnlyDictionary<string, string> CopyDimensions(
        IReadOnlyDictionary<string, string>? dimensions)
    {
        if (dimensions is not null && dimensions.Count > MaxDimensions)
        {
            throw new ArgumentException(
                $"At most {MaxDimensions} metric dimensions are allowed.",
                nameof(dimensions));
        }

        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        if (dimensions is not null)
        {
            foreach (var dimension in dimensions)
            {
                var key = ValidateName(
                    dimension.Key,
                    nameof(dimensions),
                    MaxDimensionKeyLength);
                var dimensionValue = dimension.Value;
                if (string.IsNullOrWhiteSpace(dimensionValue)
                    || dimensionValue.Length > MaxDimensionValueLength
                    || dimensionValue.Any(char.IsControl))
                {
                    throw new ArgumentException(
                        $"Dimension values must contain 1 to {MaxDimensionValueLength} printable characters.",
                        nameof(dimensions));
                }

                copy.Add(key, dimensionValue);
            }
        }

        return new ReadOnlyDictionary<string, string>(copy);
    }

    private static string ValidateName(
        string value,
        string parameterName,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > maxLength
            || !IsAsciiLetter(value[0])
            || value.Any(character => !IsNameCharacter(character)))
        {
            throw new ArgumentException(
                $"Value must start with an ASCII letter and contain at most {maxLength} letters, digits, '.', '_' or '-'.",
                parameterName);
        }

        return value;
    }

    private static bool IsNameCharacter(char value) =>
        IsAsciiLetter(value)
        || value is >= '0' and <= '9'
        || value is '.' or '_' or '-';

    private static bool IsAsciiLetter(char value) =>
        value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}
