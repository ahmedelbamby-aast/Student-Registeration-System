using System.Security.Claims;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;

namespace StudentRegistration.ApplicationTests.Specs.Spec015;

public sealed class Endpoint01And04BehaviorTests
{
    [Fact]
    public async Task Lists_validate_paging_and_pass_only_authenticated_or_explicit_admin_scope()
    {
        var reader = new CapturingReader();
        var queries = new RegistrationRecordQueries(reader, new RegistrationReceiptService());
        var invalid = await queries.ListOwnAsync(Principal("Student"), 1, 101, null);
        Assert.Equal(RegistrationRecordQueryOutcome.Invalid, invalid.Outcome);
        Assert.Equal("PAGE_SIZE_INVALID", invalid.ErrorCode);

        var student = await queries.ListOwnAsync(Principal("Student"), 1, 20, null);
        Assert.Equal(RegistrationRecordQueryOutcome.Succeeded, student.Outcome);
        Assert.Equal(ReaderCall.OwnList, reader.LastCall);

        var admin = await queries.ListAdminAsync(
            Principal("Admin"), Guid.NewGuid(), Guid.NewGuid(), 1, 20, "corr-1");
        Assert.Equal(RegistrationRecordQueryOutcome.Succeeded, admin.Outcome);
        Assert.Equal(ReaderCall.AdminList, reader.LastCall);
        Assert.Equal("corr-1", reader.CorrelationId);
    }

    internal static ClaimsPrincipal Principal(string role) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, role)],
        "test"));

    internal sealed class CapturingReader : IRegistrationRecordReader
    {
        public ReaderCall LastCall { get; private set; }
        public string? CorrelationId { get; private set; }

        public Task<RegistrationRecordPageSnapshot?> ListOwnAsync(Guid applicationUserId, Guid? termId, int page, int pageSize, CancellationToken cancellationToken = default)
        { LastCall = ReaderCall.OwnList; return Task.FromResult<RegistrationRecordPageSnapshot?>(new([], 0)); }
        public Task<RegistrationSubmissionRecord?> ReadOwnAsync(Guid applicationUserId, Guid submissionId, CancellationToken cancellationToken = default)
        { LastCall = ReaderCall.OwnDetail; return Task.FromResult<RegistrationSubmissionRecord?>(null); }
        public Task<RegistrationCurrentTimetableSnapshot?> ReadCurrentAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
        { LastCall = ReaderCall.Current; return Task.FromResult<RegistrationCurrentTimetableSnapshot?>(new(null, "none", "none", [], null)); }
        public Task<RegistrationRecordPageSnapshot?> ListAdminAsync(Guid actorApplicationUserId, Guid studentId, Guid termId, int page, int pageSize, string correlationId, CancellationToken cancellationToken = default)
        { LastCall = ReaderCall.AdminList; CorrelationId = correlationId; return Task.FromResult<RegistrationRecordPageSnapshot?>(new([], 0)); }
        public Task<RegistrationSubmissionRecord?> ReadAdminAsync(Guid actorApplicationUserId, Guid studentId, Guid termId, Guid submissionId, string correlationId, CancellationToken cancellationToken = default)
        { LastCall = ReaderCall.AdminDetail; CorrelationId = correlationId; return Task.FromResult<RegistrationSubmissionRecord?>(null); }
    }

    internal enum ReaderCall { None, OwnList, OwnDetail, Current, AdminList, AdminDetail }
}
