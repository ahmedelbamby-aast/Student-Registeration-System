namespace StudentRegistration.IdentityAccess.Application;

public static class IdentityTextNormalizer
{
    public static string NormalizeUniversityId(string universityId) =>
        Normalize(universityId, nameof(universityId));

    public static string NormalizeUserName(string userName) =>
        Normalize(userName, nameof(userName));

    private static string Normalize(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim().Normalize().ToUpperInvariant();
    }
}
