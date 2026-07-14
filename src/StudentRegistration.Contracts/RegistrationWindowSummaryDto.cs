namespace StudentRegistration.Contracts;

public sealed record RegistrationWindowSummaryDto
{
    public RegistrationWindowSummaryDto(
        string id,
        RegistrationWindowState state,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        string rowVersion)
    {
        if (!Enum.IsDefined(state) || state is RegistrationWindowState.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A concrete registration-window state is required.");
        }

        EnsureUtc(opensAtUtc, nameof(opensAtUtc));
        EnsureUtc(closesAtUtc, nameof(closesAtUtc));
        if (closesAtUtc <= opensAtUtc)
        {
            throw new ArgumentException(
                "The close instant must be after the open instant.",
                nameof(closesAtUtc));
        }

        Id = Required(id, nameof(id), 100);
        State = state;
        OpensAtUtc = opensAtUtc;
        ClosesAtUtc = closesAtUtc;
        RowVersion = Required(rowVersion, nameof(rowVersion), 128);
    }

    public string Id { get; }

    public RegistrationWindowState State { get; }

    public DateTime OpensAtUtc { get; }

    public DateTime ClosesAtUtc { get; }

    public string RowVersion { get; }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC timestamp is required.", parameterName);
        }
    }

    private static string Required(string? value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
        {
            throw new ArgumentException(
                $"A non-empty value no longer than {maximumLength} characters is required.",
                parameterName);
        }

        return value;
    }
}
