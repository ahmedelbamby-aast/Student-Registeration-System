namespace StudentRegistration.StaffAdministration.Application.Ports;

public interface IAdminAuditReader
{
    Task<AuditEventPageDto> ReadAsync(
        AdminAuditReadRequest request,
        CancellationToken cancellationToken);
}
