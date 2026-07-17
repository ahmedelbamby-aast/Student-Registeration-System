using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using ContractTermSnapshot = StudentRegistration.Contracts.Registration.RegistrationTermSnapshotDto;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

public sealed class SqlRegistrationRecordReader(
    StudentRegistrationDbContext dbContext,
    TimeProvider timeProvider) : IRegistrationRecordReader
{
    public async Task<RegistrationRecordPageSnapshot?> ListOwnAsync(
        Guid applicationUserId, Guid? termId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var studentId = await StudentIdAsync(applicationUserId, cancellationToken);
        return studentId is null ? null : await ListAsync(
            studentId.Value, termId, page, pageSize, cancellationToken);
    }

    public async Task<RegistrationSubmissionRecord?> ReadOwnAsync(
        Guid applicationUserId, Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var studentId = await StudentIdAsync(applicationUserId, cancellationToken);
        return studentId is null ? null : await ReadAsync(
            studentId.Value, null, submissionId, cancellationToken);
    }

    public async Task<RegistrationCurrentTimetableSnapshot?> ReadCurrentAsync(
        Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        var student = await dbContext.Set<Student>().AsNoTracking()
            .Where(item => item.ApplicationUserId == applicationUserId && item.IsActive)
            .Select(item => new { item.Id, item.ProgramCode, item.Cohort })
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null) return null;

        var candidates = await dbContext.Set<AcademicTerm>().AsNoTracking()
            .Where(term => term.State == TermState.Teaching ||
                term.State == TermState.RegistrationOpen)
            .OrderBy(term => term.Id)
            .ToArrayAsync(cancellationToken);
        var teaching = candidates.Where(term => term.State == TermState.Teaching).ToArray();
        var registration = candidates.Where(term => term.State == TermState.RegistrationOpen).ToArray();
        if (teaching.Length > 1 || registration.Length > 1)
            return new(null, "none", "none", [], null, "CURRENT_TERM_AMBIGUOUS");

        var term = teaching.SingleOrDefault() ?? registration.SingleOrDefault();
        if (term is null) return new(null, "none", "none", [], null);

        var termState = term.State == TermState.Teaching ? "teaching" : "registrationOpen";
        var windowState = "none";
        string? discovery = null;
        if (term.State == TermState.RegistrationOpen)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var windows = await dbContext.Set<RegistrationWindow>().AsNoTracking()
                .Where(window => window.TermId == term.Id &&
                    (window.State == RegistrationWindowLifecycleState.Published ||
                     window.State == RegistrationWindowLifecycleState.EmergencyClosed))
                .OrderBy(window => window.Id)
                .ToArrayAsync(cancellationToken);
            var applicable = windows.Where(window => Applies(window, student.ProgramCode, student.Cohort)).ToArray();
            var open = applicable.Where(window => window.GetComputedState(now) == RegistrationWindowState.Open).ToArray();
            if (open.Length > 1)
                return new(null, "none", "none", [], null, "CURRENT_TERM_AMBIGUOUS");
            var selected = open.SingleOrDefault() ?? applicable
                .Where(window => window.GetComputedState(now) == RegistrationWindowState.Upcoming)
                .OrderBy(window => window.OpensAtUtc)
                .ThenBy(window => window.Id)
                .FirstOrDefault() ?? applicable
                .Where(window => window.GetComputedState(now) == RegistrationWindowState.Closed)
                .OrderByDescending(window => window.ClosesAtUtc)
                .ThenBy(window => window.Id)
                .FirstOrDefault();
            if (selected is not null)
            {
                windowState = selected.GetComputedState(now) switch
                {
                    RegistrationWindowState.Open => "open",
                    RegistrationWindowState.Upcoming => "scheduled",
                    _ => "closed"
                };
                if (windowState == "open") discovery = "/student/subjects";
            }
        }

        var accepted = await ReadAcceptedForTermAsync(student.Id, term, cancellationToken);
        return new(ToTerm(term), termState, windowState, accepted, discovery);
    }

    public async Task<RegistrationRecordPageSnapshot?> ListAdminAsync(
        Guid actorApplicationUserId, Guid studentId, Guid termId,
        int page, int pageSize, string correlationId,
        CancellationToken cancellationToken = default)
    {
        var exists = await ScopeExistsAsync(studentId, termId, cancellationToken);
        var result = exists ? await ListAsync(studentId, termId, page, pageSize, cancellationToken) : null;
        AddInspectionAudit(actorApplicationUserId, studentId, termId, "list",
            result is null ? "not-found" : "succeeded", correlationId, null);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<RegistrationSubmissionRecord?> ReadAdminAsync(
        Guid actorApplicationUserId, Guid studentId, Guid termId,
        Guid submissionId, string correlationId,
        CancellationToken cancellationToken = default)
    {
        var result = await ReadAsync(studentId, termId, submissionId, cancellationToken);
        AddInspectionAudit(actorApplicationUserId, studentId, termId, "detail",
            result is null ? "not-found" : "succeeded", correlationId, submissionId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<RegistrationRecordPageSnapshot> ListAsync(
        Guid studentId, Guid? termId, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var query = Rows().Where(row => row.Submission.StudentId == studentId);
        if (termId is { } filter) query = query.Where(row => row.Submission.TermId == filter);
        var total = await query.CountAsync(cancellationToken);
        var skip = checked((page - 1) * pageSize);
        var rows = await query
            .OrderByDescending(row => row.Submission.ReceivedAtUtc)
            .ThenBy(row => row.Submission.Id)
            .Skip(skip).Take(pageSize).ToArrayAsync(cancellationToken);
        return new(rows.Select(row => ToRecord(row.Submission, row.Term)).ToArray(), total);
    }

    private async Task<RegistrationSubmissionRecord?> ReadAsync(
        Guid studentId, Guid? termId, Guid submissionId,
        CancellationToken cancellationToken)
    {
        var query = Rows().Where(row => row.Submission.StudentId == studentId &&
            row.Submission.Id == submissionId);
        if (termId is { } filter) query = query.Where(row => row.Submission.TermId == filter);
        var row = await query.SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : ToRecord(row.Submission, row.Term);
    }

    private async Task<IReadOnlyList<RegistrationSubmissionRecord>> ReadAcceptedForTermAsync(
        Guid studentId, AcademicTerm term, CancellationToken cancellationToken)
    {
        var rows = await Rows().Where(row =>
                row.Submission.StudentId == studentId &&
                row.Submission.TermId == term.Id &&
                row.Submission.ProcessingState == RegistrationSubmissionState.Accepted)
            .OrderBy(row => row.Submission.ReceivedAtUtc)
            .ThenBy(row => row.Submission.Id)
            .ToArrayAsync(cancellationToken);
        return rows.Select(row => ToRecord(row.Submission, row.Term)).ToArray();
    }

    private IQueryable<SubmissionTermRow> Rows() =>
        from submission in dbContext.Set<RegistrationSubmission>().AsNoTracking()
        join term in dbContext.Set<AcademicTerm>().AsNoTracking()
            on submission.TermId equals term.Id
        where submission.ProcessingState == RegistrationSubmissionState.Accepted ||
            submission.ProcessingState == RegistrationSubmissionState.Rejected
        select new SubmissionTermRow(submission, term);

    private async Task<Guid?> StudentIdAsync(Guid applicationUserId, CancellationToken token) =>
        await dbContext.Set<Student>().AsNoTracking()
            .Where(student => student.ApplicationUserId == applicationUserId && student.IsActive)
            .Select(student => (Guid?)student.Id).SingleOrDefaultAsync(token);

    private async Task<bool> ScopeExistsAsync(Guid studentId, Guid termId, CancellationToken token) =>
        await dbContext.Set<Student>().AsNoTracking().AnyAsync(student => student.Id == studentId, token) &&
        await dbContext.Set<AcademicTerm>().AsNoTracking().AnyAsync(term => term.Id == termId, token);

    private void AddInspectionAudit(Guid actorId, Guid studentId, Guid termId,
        string endpoint, string outcome, string correlationId, Guid? submissionId)
    {
        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(), $"application-user:{actorId:D}", $"student:{studentId:D}",
            "RegistrationRecordInspected", "RegistrationRecord",
            submissionId?.ToString("D") ?? $"{studentId:D}:{termId:D}", outcome,
            null, System.Text.Json.JsonSerializer.Serialize(new
            {
                targetStudentId = studentId,
                termId,
                endpoint,
                outcome
            }), correlationId, timeProvider.GetUtcNow().UtcDateTime));
    }

    private static RegistrationSubmissionRecord ToRecord(
        RegistrationSubmission submission, AcademicTerm term) => new(
            submission.Id, submission.StudentId, submission.TermId,
            submission.ProcessingState == RegistrationSubmissionState.Accepted ? "accepted" : "rejected",
            submission.ResultCode!, submission.Reference, submission.ReceiptSnapshotJson,
            submission.DecisionSnapshotJson!, submission.ReceivedAtUtc,
            submission.CompletedAtUtc!.Value, ToTerm(term), ToTermState(term.State));

    private static ContractTermSnapshot ToTerm(AcademicTerm term) =>
        new(term.Id, term.Code, term.DisplayName, term.TimeZoneId);

    private static string ToTermState(TermState state) => state switch
    {
        TermState.Draft => "draft",
        TermState.RegistrationOpen => "registrationOpen",
        TermState.RegistrationClosed => "registrationClosed",
        TermState.Teaching => "teaching",
        TermState.Completed => "completed",
        TermState.Archived => "archived",
        _ => throw new InvalidOperationException("Unknown academic-term state.")
    };

    private static bool Applies(RegistrationWindow window, string program, string cohort) =>
        window.ScopeType switch
        {
            RegistrationWindowScopeType.AllStudents => true,
            RegistrationWindowScopeType.Program => string.Equals(window.ScopeValue, program, StringComparison.OrdinalIgnoreCase),
            RegistrationWindowScopeType.Cohort => string.Equals(window.ScopeValue, cohort, StringComparison.OrdinalIgnoreCase),
            _ => false
        };

    private sealed record SubmissionTermRow(
        RegistrationSubmission Submission,
        AcademicTerm Term);
}
