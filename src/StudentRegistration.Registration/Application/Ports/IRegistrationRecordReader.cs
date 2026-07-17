using StudentRegistration.Contracts.Registration;
using ContractRegistrationTermSnapshotDto = StudentRegistration.Contracts.Registration.RegistrationTermSnapshotDto;

namespace StudentRegistration.Registration.Application.Ports;

public sealed record RegistrationSubmissionRecord(
    Guid SubmissionId,
    Guid StudentId,
    Guid TermId,
    string State,
    string ResultCode,
    string? Reference,
    string? ReceiptSnapshotJson,
    string DecisionSnapshotJson,
    DateTime SubmittedAtUtc,
    DateTime CompletedAtUtc,
    ContractRegistrationTermSnapshotDto FallbackTerm,
    string TermState = "archived");

public sealed record RegistrationRecordPageSnapshot(
    IReadOnlyList<RegistrationSubmissionRecord> Items,
    int TotalCount);

public sealed record RegistrationCurrentTimetableSnapshot(
    ContractRegistrationTermSnapshotDto? Term,
    string TermState,
    string RegistrationWindowState,
    IReadOnlyList<RegistrationSubmissionRecord> AcceptedSubmissions,
    string? SubjectDiscoveryPath,
    string? ErrorCode = null);

public interface IRegistrationRecordReader
{
    Task<RegistrationRecordPageSnapshot?> ListOwnAsync(
        Guid applicationUserId, Guid? termId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<RegistrationSubmissionRecord?> ReadOwnAsync(
        Guid applicationUserId, Guid submissionId,
        CancellationToken cancellationToken = default);

    Task<RegistrationCurrentTimetableSnapshot?> ReadCurrentAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default);

    Task<RegistrationRecordPageSnapshot?> ListAdminAsync(
        Guid actorApplicationUserId, Guid studentId, Guid termId,
        int page, int pageSize, string correlationId,
        CancellationToken cancellationToken = default);

    Task<RegistrationSubmissionRecord?> ReadAdminAsync(
        Guid actorApplicationUserId, Guid studentId, Guid termId,
        Guid submissionId, string correlationId,
        CancellationToken cancellationToken = default);
}
