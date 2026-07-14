namespace StudentRegistration.IdentityAccess.Application.Ports;

public interface IIdentityPasswordValidator
{
    IdentityPasswordValidationResult Validate(
        string password,
        IdentityPasswordContext context);
}

public sealed record IdentityPasswordContext(
    string? UniversityId,
    string? UserName,
    string? DisplayName)
{
    public static IdentityPasswordContext Empty { get; } = new(null, null, null);

    public IEnumerable<string> ContextualTerms
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(UniversityId))
            {
                yield return UniversityId;
            }

            if (!string.IsNullOrWhiteSpace(UserName))
            {
                yield return UserName;
            }

            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                yield return DisplayName;
            }
        }
    }
}

public sealed record IdentityPasswordValidationResult(bool IsValid, string? ErrorCode)
{
    public static readonly IdentityPasswordValidationResult Valid = new(true, null);

    public static IdentityPasswordValidationResult Invalid(string errorCode) =>
        new(false, errorCode);
}
