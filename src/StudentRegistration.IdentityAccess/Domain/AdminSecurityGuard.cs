namespace StudentRegistration.IdentityAccess.Domain;

public sealed class AdminSecurityGuard
{
    public const int SingletonId = 1;

    private AdminSecurityGuard()
    {
    }

    public AdminSecurityGuard(int id = SingletonId)
    {
        if (id != SingletonId)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "Only the singleton Admin security guard is valid.");
        }

        Id = id;
    }

    public int Id { get; private set; }
    public byte[] Version { get; private set; } = [];
}
