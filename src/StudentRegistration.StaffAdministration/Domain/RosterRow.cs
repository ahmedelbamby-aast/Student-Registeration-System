using StudentRegistration.Contracts.Staff;

namespace StudentRegistration.StaffAdministration.Domain;

/// <summary>
/// The deliberately minimal, read-only assigned-group roster projection.
/// </summary>
public sealed record RosterRow
{
    public RosterRow(
        string universityId,
        string displayName,
        string enrollmentState)
    {
        UniversityId = Required(universityId, nameof(universityId), 50);
        DisplayName = Required(displayName, nameof(displayName), 200);
        if (!string.Equals(
                enrollmentState,
                StaffWorkspaceContract.ActiveEnrollmentState,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Only active enrollment rows belong in the staff roster projection.",
                nameof(enrollmentState));
        }

        EnrollmentState = StaffWorkspaceContract.ActiveEnrollmentState;
    }

    public string UniversityId { get; }

    public string DisplayName { get; }

    public string EnrollmentState { get; }

    public RosterRowDto ToDto() => new(UniversityId, DisplayName, EnrollmentState);

    private static string Required(string value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                normalized.Length,
                $"The value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}
