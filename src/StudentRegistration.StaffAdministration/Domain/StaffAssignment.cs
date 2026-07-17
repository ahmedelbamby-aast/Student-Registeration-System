using StudentRegistration.Contracts.Scheduling;
using StudentRegistration.Contracts.Staff;

namespace StudentRegistration.StaffAdministration.Domain;

/// <summary>
/// A read-only workspace projection over Scheduling-owned assignment data.
/// It is not an aggregate or persistence entity.
/// </summary>
public sealed record StaffAssignment
{
    public StaffAssignment(
        string subjectCode,
        string subjectTitle,
        GroupDto group,
        string staffRole,
        int rosterCount)
    {
        SubjectCode = Required(subjectCode, nameof(subjectCode), 50);
        SubjectTitle = Required(subjectTitle, nameof(subjectTitle), 200);
        Group = group ?? throw new ArgumentNullException(nameof(group));
        if (group.Id == Guid.Empty)
        {
            throw new ArgumentException("A group identifier is required.", nameof(group));
        }

        if (staffRole is not StaffWorkspaceContract.LecturerRole
            and not StaffWorkspaceContract.TeachingAssistantRole)
        {
            throw new ArgumentException(
                "The assignment role must be Lecturer or TeachingAssistant.",
                nameof(staffRole));
        }

        if (rosterCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rosterCount),
                rosterCount,
                "Roster count cannot be negative.");
        }

        StaffRole = staffRole;
        RosterCount = rosterCount;
    }

    public string SubjectCode { get; }

    public string SubjectTitle { get; }

    public GroupDto Group { get; }

    public string StaffRole { get; }

    public int RosterCount { get; }

    public StaffAssignmentDto ToDto() => new(
        SubjectCode,
        SubjectTitle,
        Group,
        StaffRole,
        RosterCount);

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
