namespace StudentRegistration.StaffAdministration.Application.Ports;

public enum StaffRosterAuditOutcome
{
    Succeeded,
    Denied,
    Failed
}

public sealed record StaffRosterAuditEntry(
    Guid ActorApplicationUserId,
    Guid GroupId,
    string Purpose,
    StaffRosterAuditOutcome Outcome,
    int RowCount,
    string CorrelationId);

/// <summary>
/// Writes privacy-safe roster access metadata. Roster rows and student values
/// are deliberately absent from this boundary.
/// </summary>
public interface IStaffWorkspaceAuditWriter
{
    Task WriteRosterAccessAsync(
        StaffRosterAuditEntry entry,
        CancellationToken cancellationToken = default);
}
