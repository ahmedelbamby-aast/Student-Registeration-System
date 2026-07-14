using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.IdentityAccess.Application;

public sealed class IdentityPasswordValidator : IIdentityPasswordValidator
{
    private readonly IdentitySecurityOptions _options;

    public IdentityPasswordValidator(IdentitySecurityOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IdentityPasswordValidationResult Validate(
        string password,
        IdentityPasswordContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = _options.ValidatePassword(password, context.ContextualTerms);
        return result.IsAccepted
            ? IdentityPasswordValidationResult.Valid
            : IdentityPasswordValidationResult.Invalid(
                result.ErrorCode ?? "PASSWORD_REJECTED");
    }
}
