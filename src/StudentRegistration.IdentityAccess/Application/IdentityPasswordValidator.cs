using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class IdentityPasswordValidator : IIdentityPasswordValidator
{
    private readonly IdentitySecurityOptions _options;

    public IdentityPasswordValidator(IdentitySecurityOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IdentityPasswordValidationResult Validate(string password)
    {
        var result = _options.ValidatePassword(password);
        return result.IsAccepted
            ? IdentityPasswordValidationResult.Valid
            : IdentityPasswordValidationResult.Invalid(
                result.ErrorCode ?? "PASSWORD_REJECTED");
    }
}
