namespace StudentRegistration.IdentityAccess.Domain;

internal static class IdentityDomainGuard
{
    public static string Required(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"Value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }

    public static string? Optional(string? value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Required(value, parameterName, maximumLength);
    }
}
