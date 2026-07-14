namespace StudentRegistration.IdentityAccess.Application.Ports;

public interface IIdentityPasswordValidator
{
    IdentityPasswordValidationResult Validate(string password);
}

public sealed record IdentityPasswordValidationResult(bool IsValid, string? ErrorCode)
{
    public static readonly IdentityPasswordValidationResult Valid = new(true, null);

    public static IdentityPasswordValidationResult Invalid(string errorCode) =>
        new(false, errorCode);
}
