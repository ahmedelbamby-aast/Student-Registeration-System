namespace StudentRegistration.IdentityAccess.Application;

public static class IdentityAuthenticationDefaults
{
    public const string AuthenticationScheme = "StudentRegistration.Identity";
    public const string SecurityStampClaim = "identity_security_stamp";
    public const string ActiveRoleClaim = "identity_active_role";
}
