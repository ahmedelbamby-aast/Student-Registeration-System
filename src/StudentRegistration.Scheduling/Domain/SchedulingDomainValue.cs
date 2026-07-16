namespace StudentRegistration.Scheduling.Domain;

internal static class SchedulingDomainValue
{
    public static void Identifier(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }

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

    public static void Defined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "A declared value is required.");
        }
    }

    public static void Utc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC instant is required.", parameterName);
        }
    }

    public static byte[] Version(byte[] value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (value.Length == 0)
        {
            throw new ArgumentException("A concurrency version is required.", parameterName);
        }

        return value.ToArray();
    }
}
