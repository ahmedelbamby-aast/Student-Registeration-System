using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

/// <summary>
/// SQL-authoritative approval queue and decision adapter. Staff scope is
/// resolved before a submission line is read, and every decision/finalization
/// runs inside one serializable transaction.
/// </summary>
public sealed class SqlRegistrationApprovalStore(
    StudentRegistrationDbContext dbContext,
    IRegistrationPlanContextReader planContextReader,
    SqlSeatAllocator seatAllocator,
    TimeProvider timeProvider) : IRegistrationApprovalStore
{
    public async Task<RegistrationApprovalResult> ListAsync(
        RegistrationApprovalActor actor,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var allowedGroups = await AllowedGroupsAsync(actor, cancellationToken);
        if (!actor.DecideAll && allowedGroups.Length == 0)
        {
            return new(RegistrationApprovalOutcome.Found, Page: new([], page, pageSize, 0));
        }

        var query =
            from submission in dbContext.Set<RegistrationSubmission>().AsNoTracking()
            join line in dbContext.Set<RegistrationSubmissionLine>().AsNoTracking()
                on submission.Id equals line.SubmissionId
            where submission.ProcessingState == RegistrationSubmissionState.PendingApproval
                && line.State == RegistrationSubmissionLineState.PendingApproval
                && (actor.DecideAll || allowedGroups.Contains(line.GroupId))
            select new
            {
                SubmissionId = submission.Id,
                LineId = line.Id,
                submission.ReceivedAtUtc,
                line.CourseCode,
            };
        var total = await query.CountAsync(cancellationToken);
        var keys = await query
            .OrderBy(row => row.ReceivedAtUtc)
            .ThenBy(row => row.CourseCode)
            .ThenBy(row => row.LineId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        var rows = new List<RegistrationApprovalQueueRowDto>(keys.Length);
        foreach (var key in keys)
        {
            var row = await BuildRowAsync(
                actor,
                allowedGroups,
                key.SubmissionId,
                key.LineId,
                cancellationToken);
            if (row is not null)
            {
                rows.Add(row);
            }
        }

        return new(
            RegistrationApprovalOutcome.Found,
            Page: new(rows, page, pageSize, total));
    }

    public async Task<RegistrationApprovalResult> FindAsync(
        RegistrationApprovalActor actor,
        Guid submissionId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var allowedGroups = await AllowedGroupsAsync(actor, cancellationToken);
        var row = await BuildRowAsync(
            actor,
            allowedGroups,
            submissionId,
            lineId,
            cancellationToken);
        return row is null
            ? new(RegistrationApprovalOutcome.NotFound)
            : new(RegistrationApprovalOutcome.Found, Row: row);
    }

    public async Task<RegistrationApprovalResult> DecideAsync(
        RegistrationApprovalActor actor,
        Guid submissionId,
        Guid lineId,
        DecideRegistrationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowedGroups = await AllowedGroupsAsync(actor, cancellationToken);
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<RegistrationApprovalResult>(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
            var submission = await dbContext.Set<RegistrationSubmission>()
                .Include(item => item.Lines)
                .SingleOrDefaultAsync(item => item.Id == submissionId, cancellationToken);
            var line = submission?.Lines.SingleOrDefault(item => item.Id == lineId);
            if (submission is null || line is null ||
                submission.ProcessingState is not RegistrationSubmissionState.PendingApproval ||
                !IsAuthorized(actor, allowedGroups, line.GroupId))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(RegistrationApprovalOutcome.NotFound);
            }

            var expectedSubmission = request.ExpectedSubmissionRowVersion.Trim();
            var currentSubmission = SubmissionVersion(submission);
            var currentLine = Convert.ToBase64String(line.Version);
            if (!string.Equals(expectedSubmission, currentSubmission, StringComparison.Ordinal) ||
                !string.Equals(request.ExpectedLineRowVersion.Trim(), currentLine, StringComparison.Ordinal))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(
                    RegistrationApprovalOutcome.Conflict,
                    ErrorCode: "STALE_VERSION",
                    CurrentVersion: currentLine);
            }

            var normalizedDecision = request.Decision.Trim().ToLowerInvariant();
            var payloadHash = Hash(
                $"{normalizedDecision}|{request.Reason.Trim()}|{submissionId:D}|{lineId:D}");
            var existingDecision = await dbContext.Set<RegistrationApprovalDecision>()
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.SubmissionLineId == lineId, cancellationToken);
            if (existingDecision is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
                if (existingDecision.ActorId == actor.ApplicationUserId &&
                    existingDecision.ClientRequestId == request.ClientRequestId &&
                    string.Equals(existingDecision.PayloadHash, payloadHash, StringComparison.Ordinal))
                {
                    return new(
                        RegistrationApprovalOutcome.Updated,
                        Registration: await BuildResultAsync(submissionId, cancellationToken));
                }

                return new(
                    RegistrationApprovalOutcome.Conflict,
                    ErrorCode: "APPROVAL_ALREADY_DECIDED",
                    CurrentVersion: currentLine);
            }

            var policy = await dbContext.Set<PolicySet>()
                .AsNoTracking()
                .Where(item => item.TermId == submission.TermId &&
                    item.State == PolicySetState.Published)
                .OrderByDescending(item => item.EffectiveFromUtc)
                .ThenBy(item => item.Id)
                .Select(item => new { item.Id, item.VersionCode })
                .FirstOrDefaultAsync(cancellationToken);
            if (policy is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(
                    RegistrationApprovalOutcome.Unavailable,
                    ErrorCode: "POLICY_UNAVAILABLE");
            }

            Guid? assignmentScope = actor.DecideAll ? null : line.GroupId;
            var decisionValue = normalizedDecision == "approve"
                ? RegistrationApprovalDecisionValue.Approved
                : RegistrationApprovalDecisionValue.Rejected;
            dbContext.Add(new RegistrationApprovalDecision(
                Guid.NewGuid(),
                line.Id,
                actor.ApplicationUserId,
                actor.Role,
                decisionValue,
                request.Reason.Trim(),
                assignmentScope,
                request.ClientRequestId,
                payloadHash,
                policy.Id,
                policy.VersionCode,
                Guid.NewGuid().ToString("N"),
                timeProvider.GetUtcNow().UtcDateTime));

            if (decisionValue is RegistrationApprovalDecisionValue.Rejected)
            {
                line.Reject();
                await ReleasePlanAsync(submission, "SUBJECT_REJECTED", cancellationToken);
                submission.CompleteRejected(
                    "SUBJECT_REJECTED",
                    JsonSerializer.Serialize(new { lineId, decision = "rejected" }),
                    timeProvider.GetUtcNow().UtcDateTime);
            }
            else
            {
                line.Approve();
                var lastLine = submission.Lines.All(item =>
                    item.State is RegistrationSubmissionLineState.Approved);
                if (lastLine)
                {
                    await AcceptPlanAsync(submission, policy.Id, policy.VersionCode, cancellationToken);
                }
            }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
                return new(
                    RegistrationApprovalOutcome.Updated,
                    Registration: await BuildResultAsync(submissionId, cancellationToken));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                dbContext.ChangeTracker.Clear();
                return new(
                    RegistrationApprovalOutcome.Conflict,
                    ErrorCode: "STALE_VERSION");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                dbContext.ChangeTracker.Clear();
                return new(
                    RegistrationApprovalOutcome.Conflict,
                    ErrorCode: "APPROVAL_CONFLICT");
            }
        });
    }

    private async Task AcceptPlanAsync(
        RegistrationSubmission submission,
        Guid policySetId,
        string policyVersion,
        CancellationToken cancellationToken)
    {
        var groupIds = submission.Lines.Select(item => item.GroupId).OrderBy(id => id).ToArray();
        var snapshot = await planContextReader.ReadAsync(
            submission.StudentId,
            submission.TermId,
            groupIds,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);
        if (snapshot is null || snapshot.Groups.Count != groupIds.Length)
        {
            throw new InvalidOperationException(
                "FINAL_REVALIDATION_FAILED: The approved plan no longer has complete authoritative context.");
        }

        await seatAllocator.ConvertHoldsToEnrollmentsAsync(groupIds, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var holds = await dbContext.Set<RegistrationSeatHold>()
            .Where(item => submission.Lines.Select(line => line.Id)
                .Contains(item.SubmissionLineId) &&
                item.State == RegistrationSeatHoldState.Active)
            .ToArrayAsync(cancellationToken);
        foreach (var hold in holds)
        {
            hold.Consume(now);
        }

        foreach (var line in submission.Lines)
        {
            dbContext.Add(new Enrollment(
                Guid.NewGuid(),
                submission.StudentId,
                line.OfferingId,
                line.GroupId,
                submission.Id,
                EnrollmentState.Active,
                now));
        }

        var term = await dbContext.Set<AcademicTerm>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == submission.TermId, cancellationToken);
        var groups = snapshot.Groups.Select(ToGroup).ToArray();
        var receipt = new RegistrationReceiptSnapshotDto(
            new(term.Id, term.Code, term.DisplayName, term.TimeZoneId),
            groups,
            submission.RequestedCredits,
            policySetId,
            policyVersion,
            submission.ReceivedAtUtc);
        submission.CompleteAccepted(
            "REGISTERED",
            $"REG-{submission.Id.ToString("N")[..12].ToUpperInvariant()}",
            JsonSerializer.Serialize(receipt),
            JsonSerializer.Serialize(new { result = "REGISTERED", policyVersion }),
            now);
    }

    private async Task ReleasePlanAsync(
        RegistrationSubmission submission,
        string reason,
        CancellationToken cancellationToken)
    {
        var active = await dbContext.Set<RegistrationSeatHold>()
            .Where(item => submission.Lines.Select(line => line.Id)
                .Contains(item.SubmissionLineId) &&
                item.State == RegistrationSeatHoldState.Active)
            .ToArrayAsync(cancellationToken);
        await seatAllocator.ReleaseHoldsAsync(
            active.Select(item => item.GroupId).ToArray(),
            cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var hold in active)
        {
            hold.Release(reason, now);
        }
    }

    private async Task<RegistrationApprovalQueueRowDto?> BuildRowAsync(
        RegistrationApprovalActor actor,
        IReadOnlyCollection<Guid> allowedGroups,
        Guid submissionId,
        Guid lineId,
        CancellationToken cancellationToken)
    {
        var pair = await (
                from submission in dbContext.Set<RegistrationSubmission>().AsNoTracking()
                join line in dbContext.Set<RegistrationSubmissionLine>().AsNoTracking()
                    on submission.Id equals line.SubmissionId
                where submission.Id == submissionId && line.Id == lineId &&
                    submission.ProcessingState == RegistrationSubmissionState.PendingApproval &&
                    (actor.DecideAll || allowedGroups.Contains(line.GroupId))
                select new { submission, line })
            .SingleOrDefaultAsync(cancellationToken);
        if (pair is null)
        {
            return null;
        }

        var student = await (
                from academic in dbContext.Set<Student>().AsNoTracking()
                join user in dbContext.Set<ApplicationUser>().AsNoTracking()
                    on academic.ApplicationUserId equals user.Id
                where academic.Id == pair.submission.StudentId
                select new
                {
                    UniversityId = user.UniversityId ?? user.UserName,
                    DisplayName = user.UserName,
                    academic.CurrentGpa
                })
            .SingleAsync(cancellationToken);
        var windowClose = await dbContext.Set<RegistrationWindow>()
            .AsNoTracking()
            .Where(item => item.TermId == pair.submission.TermId &&
                item.State == RegistrationWindowLifecycleState.Published)
            .MaxAsync(item => (DateTime?)item.ClosesAtUtc, cancellationToken)
            ?? pair.submission.ReceivedAtUtc;
        var group = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == pair.line.GroupId, cancellationToken);
        return new(
            pair.submission.Id,
            new(
                pair.line.Id,
                pair.line.OfferingId,
                pair.line.GroupId,
                pair.line.CourseCode,
                pair.line.SubjectTitle,
                pair.line.Credits,
                "pendingApproval",
                new(
                    group.Capacity,
                    group.EnrolledCount,
                    group.HeldSeatCount,
                    Math.Max(0, group.Capacity - group.EnrolledCount - group.HeldSeatCount)),
                Convert.ToBase64String(pair.line.Version)),
            student.UniversityId,
            student.DisplayName,
            pair.submission.RequestedCredits,
            student.CurrentGpa,
            pair.submission.RequestedCredits > 18m,
            pair.submission.ReceivedAtUtc,
            windowClose,
            SubmissionVersion(pair.submission));
    }

    private async Task<Guid[]> AllowedGroupsAsync(
        RegistrationApprovalActor actor,
        CancellationToken cancellationToken)
    {
        if (actor.DecideAll)
        {
            return [];
        }

        var staff = await dbContext.Set<Staff>()
            .AsNoTracking()
            .Where(item => item.ApplicationUserId == actor.ApplicationUserId && item.IsActive)
            .Select(item => (Guid?)item.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (staff is null)
        {
            return [];
        }

        var role = actor.Role is RegistrationApprovalActorRole.Lecturer
            ? TeachingRole.Lecturer
            : TeachingRole.TeachingAssistant;
        return await dbContext.Set<GroupStaffAssignment>()
            .AsNoTracking()
            .Where(item => item.StaffId == staff.Value && item.TeachingRole == role)
            .Select(item => item.GroupId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    private static bool IsAuthorized(
        RegistrationApprovalActor actor,
        IReadOnlyCollection<Guid> groups,
        Guid groupId) => actor.DecideAll || groups.Contains(groupId);

    private async Task<RegistrationFinalResult> BuildResultAsync(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var submission = await dbContext.Set<RegistrationSubmission>()
            .AsNoTracking()
            .Include(item => item.Lines)
            .SingleAsync(item => item.Id == submissionId, cancellationToken);
        var decisions = await dbContext.Set<RegistrationApprovalDecision>()
            .AsNoTracking()
            .Where(item => submission.Lines.Select(line => line.Id)
                .Contains(item.SubmissionLineId))
            .ToDictionaryAsync(item => item.SubmissionLineId, cancellationToken);
        var groupIds = submission.Lines.Select(item => item.GroupId).ToArray();
        var capacities = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(item => groupIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);
        var lines = submission.Lines.Select(line =>
        {
            var group = capacities[line.GroupId];
            decisions.TryGetValue(line.Id, out var decision);
            return new RegistrationSubmissionLineDto(
                line.Id,
                line.OfferingId,
                line.GroupId,
                line.CourseCode,
                line.SubjectTitle,
                line.Credits,
                LineState(line.State),
                new(
                    group.Capacity,
                    group.EnrolledCount,
                    group.HeldSeatCount,
                    Math.Max(0, group.Capacity - group.EnrolledCount - group.HeldSeatCount)),
                Convert.ToBase64String(line.Version),
                decision is null
                    ? null
                    : new(
                        decision.Decision == RegistrationApprovalDecisionValue.Approved
                            ? "approved"
                            : "rejected",
                        decision.ActorRole.ToString(),
                        decision.Reason,
                        decision.DecidedAtUtc));
        }).ToArray();
        var receipt = string.IsNullOrWhiteSpace(submission.ReceiptSnapshotJson)
            ? null
            : JsonSerializer.Deserialize<RegistrationReceiptSnapshotDto>(
                submission.ReceiptSnapshotJson);
        return new(
            submission.Id,
            SubmissionState(submission.ProcessingState),
            submission.ResultCode ?? "PENDING_APPROVAL",
            receipt?.Groups ?? [],
            submission.ReceivedAtUtc,
            submission.CompletedAtUtc,
            receipt?.PolicySetId ?? Guid.Empty,
            receipt?.PolicyVersion ?? string.Empty,
            string.Empty,
            submission.Reference,
            receipt,
            submission.Origin == RegistrationSubmissionOrigin.FirstTermAutomatic
                ? "firstTermAutomatic"
                : "studentSelfService",
            submission.RequestedCredits,
            lines);
    }

    private static RegistrationGroupSnapshotDto ToGroup(RegistrationPlanGroupSnapshot group) =>
        new(
            group.OfferingId,
            group.CourseCode,
            group.SubjectTitle,
            group.GroupId,
            group.GroupCode,
            group.Credits,
            group.Meetings.Select(meeting => new RegistrationMeetingSnapshotDto(
                meeting.MeetingId,
                meeting.ActivityKind,
                (int)meeting.DayOfWeek,
                meeting.StartLocal.ToString("HH:mm"),
                meeting.EndLocal.ToString("HH:mm"),
                meeting.RoomCode,
                meeting.Location,
                [])).ToArray());

    private static string SubmissionVersion(RegistrationSubmission submission) =>
        Convert.ToBase64String(BitConverter.GetBytes(submission.UpdatedAtUtc.Ticks));

    private static string SubmissionState(RegistrationSubmissionState state) => state switch
    {
        RegistrationSubmissionState.PendingApproval => "pendingApproval",
        RegistrationSubmissionState.Accepted => "accepted",
        RegistrationSubmissionState.Rejected => "rejected",
        RegistrationSubmissionState.Expired => "expired",
        _ => "processing",
    };

    private static string LineState(RegistrationSubmissionLineState state) => state switch
    {
        RegistrationSubmissionLineState.PendingApproval => "pendingApproval",
        RegistrationSubmissionLineState.Approved => "approved",
        RegistrationSubmissionLineState.Rejected => "rejected",
        RegistrationSubmissionLineState.Expired => "expired",
        _ => "pendingApproval",
    };

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

}
