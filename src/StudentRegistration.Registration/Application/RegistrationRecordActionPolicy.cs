namespace StudentRegistration.Registration.Application;

public static class RegistrationRecordActionPolicy
{
    public static IReadOnlyList<string> AllowedActions { get; } = Array.Empty<string>();

    public static bool IsAllowed(string action) => action switch
    {
        "drop" or "withdrawal" or "correction" => false,
        _ => false
    };

    // Drop, Withdrawal, and Correction remain excluded until separately approved.
}
