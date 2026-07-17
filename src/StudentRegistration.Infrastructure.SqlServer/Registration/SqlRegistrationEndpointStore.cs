using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Registration;

/// <summary>
/// Thin SQL adapter for the SPEC-014 HTTP boundary. It composes the existing
/// transaction coordinator, scoped submission store, and seat allocator; it
/// does not introduce a second transaction or a second idempotency record.
/// </summary>
public sealed class SqlRegistrationEndpointStore(
    StudentRegistrationDbContext dbContext,
    IRegistrationPlanContextReader planContextReader,
    RegistrationSubmissionStore submissionStore,
    SqlSeatAllocator seatAllocator,
    TimeProvider timeProvider) :
    IRegistrationEndpointStore,
    IRegistrationLocalTransactionStore
{
    private string? _pendingRejectionCode;
    private RegistrationPlanContextSnapshot? _currentSnapshot;

    public async Task<RegistrationCommandContext?> ResolveCommandContextAsync(
        Guid applicationUserId,
        Guid termId,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty || termId == Guid.Empty)
        {
            return null;
        }

        var student = await (
                from candidate in dbContext.Set<Student>().AsNoTracking()
                join state in dbContext.Set<StudentTermAcademicState>().AsNoTracking()
                    on new { StudentId = candidate.Id, TermId = termId }
                    equals new { state.StudentId, state.TermId }
                where candidate.ApplicationUserId == applicationUserId && candidate.IsActive
                select new
                {
                    candidate.Id,
                    candidate.ProgramCode,
                    candidate.Cohort,
                    StateVersion = state.Version
                })
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null || student.StateVersion.Length == 0)
        {
            return null;
        }

        var windows = await dbContext.Set<RegistrationWindow>()
            .AsNoTracking()
            .Where(window => window.TermId == termId &&
                (window.State == RegistrationWindowLifecycleState.Published ||
                 window.State == RegistrationWindowLifecycleState.EmergencyClosed))
            .ToArrayAsync(cancellationToken);
        var window = windows
            .OrderByDescending(candidate => candidate.ScopeType == RegistrationWindowScopeType.Cohort)
            .ThenByDescending(candidate => candidate.ScopeType == RegistrationWindowScopeType.Program)
            .ThenBy(candidate => candidate.Id)
            .FirstOrDefault(candidate =>
                candidate.ScopeType == RegistrationWindowScopeType.AllStudents ||
                candidate.ScopeType == RegistrationWindowScopeType.Program &&
                    string.Equals(candidate.ScopeValue, student.ProgramCode,
                        StringComparison.OrdinalIgnoreCase) ||
                candidate.ScopeType == RegistrationWindowScopeType.Cohort &&
                    string.Equals(candidate.ScopeValue, student.Cohort,
                        StringComparison.OrdinalIgnoreCase));
        if (window is null || window.Version.Length == 0)
        {
            return null;
        }

        var resolved = new ResolvedRegistrationContext(
            applicationUserId,
            student.Id,
            termId,
            Convert.ToBase64String(window.Version),
            window.OpensAtUtc,
            window.ClosesAtUtc,
            window.State == RegistrationWindowLifecycleState.EmergencyClosed);
        return new(resolved, Convert.ToBase64String(student.StateVersion));
    }

    public async Task<RegistrationEndpointResult> SubmitAsync(
        RegistrationCommand command,
        RegistrationTransactionCoordinator coordinator,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(coordinator);
        var payloadHash = RegistrationSubmissionStore.ComputeCanonicalPayloadHash(
            $"{command.PlanId:D}|{command.ExpectedPlanRowVersion}|{command.ClientRequestId:D}");
        var scope = new RegistrationRequestScope(
            command.StudentId,
            command.TermId,
            command.ClientRequestId);

        var visible = await submissionStore.ReplayCommittedAsync(
            scope,
            payloadHash,
            cancellationToken);
        var visibleResult = ToVisibleResult(visible, command.TermId, command.ClientRequestId);
        if (visibleResult is not null)
        {
            return visibleResult;
        }

        var plan = await dbContext.Set<RegistrationPlan>()
            .AsNoTracking()
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == command.PlanId &&
                candidate.StudentId == command.StudentId &&
                candidate.TermId == command.TermId,
                cancellationToken);
        if (plan is null || plan.Items.Count == 0)
        {
            return Conflict("PLAN_CHANGED");
        }

        _currentSnapshot = await planContextReader.ReadAsync(
            command.StudentId,
            command.TermId,
            plan.Items.Select(item => item.SelectedGroupId).ToArray(),
            command.ReceivedAtUtc,
            cancellationToken);
        if (_currentSnapshot is null ||
            _currentSnapshot.Groups.Count != plan.Items.Count)
        {
            return Conflict("POLICY_CHANGED");
        }

        var studentTermVersion = _currentSnapshot.AcademicContextVersion;
        var scopeCode = await dbContext.Set<Student>()
            .AsNoTracking()
            .Where(student => student.Id == command.StudentId)
            .Select(student => student.ProgramCode)
            .SingleAsync(cancellationToken);
        var transactionPlan = new RegistrationTransactionPlan(
            command.StudentId,
            command.TermId,
            studentTermVersion,
            command.PlanId,
            command.ExpectedPlanRowVersion,
            command.ExpectedRegistrationContextVersion,
            command.ReceivedAtUtc,
            scopeCode,
            scopeCode,
            plan.Items.Select(item => item.SelectedGroupId).ToArray());

        RegistrationEndpointResult? transactionResult = null;
        _pendingRejectionCode = null;
        var boundary = await coordinator.ExecuteRegistrationAsync(
            transactionPlan,
            async token =>
            {
                var claim = await submissionStore.ClaimInsideTransactionAsync(
                    scope, payloadHash, command.ReceivedAtUtc, token);
                var existing = ToVisibleResult(claim, command.TermId, command.ClientRequestId);
                if (existing is not null)
                {
                    transactionResult = existing;
                    return;
                }

                var submission = claim.Submission!;
                if (_pendingRejectionCode is not null)
                {
                    await FinalizeRejectedAsync(
                        submission, _pendingRejectionCode, command, token);
                    transactionResult = Conflict(_pendingRejectionCode);
                    return;
                }

                var allocation = await seatAllocator.AllocateAllOrRejectAsync(
                    transactionPlan.GroupIds, token);
                if (!allocation.IsAccepted)
                {
                    var reason = allocation.ReasonCode ?? "GROUP_FULL";
                    await FinalizeRejectedAsync(submission, reason, command, token);
                    transactionResult = Conflict(reason);
                    return;
                }

                foreach (var group in _currentSnapshot.Groups)
                {
                    dbContext.Add(new Enrollment(
                        Guid.NewGuid(),
                        command.StudentId,
                        group.OfferingId,
                        group.GroupId,
                        submission.Id,
                        EnrollmentState.Active,
                        command.ReceivedAtUtc));
                }

                var groups = _currentSnapshot.Groups.Select(ToGroup).ToArray();
                var term = await dbContext.Set<AcademicTerm>()
                    .AsNoTracking()
                    .SingleAsync(item => item.Id == command.TermId, token);
                var receipt = new RegistrationReceiptSnapshotDto(
                    new(term.Id, term.Code, term.DisplayName, term.TimeZoneId),
                    groups,
                    _currentSnapshot.Groups.Sum(group => group.Credits),
                    _currentSnapshot.PolicySetId,
                    _currentSnapshot.PolicyVersion,
                    command.ReceivedAtUtc);
                var decision = Decision("REGISTERED", command);
                var completedAt = timeProvider.GetUtcNow().UtcDateTime;
                await submissionStore.FinalizeAcceptedAsync(
                    submission,
                    "REGISTERED",
                    NewReference(),
                    JsonSerializer.Serialize(receipt),
                    JsonSerializer.Serialize(decision),
                    completedAt,
                    token);
                transactionResult = new(
                    RegistrationEndpointOutcome.Created,
                    ToFinal(submission, receipt, decision, groups));
            },
            cancellationToken);

        if (boundary.Outcome is RegistrationBoundaryOutcome.Committed &&
            transactionResult is not null)
        {
            return transactionResult;
        }

        return boundary.Outcome switch
        {
            RegistrationBoundaryOutcome.HoldBlocked => Conflict("HOLD_BLOCKED"),
            RegistrationBoundaryOutcome.StaleVersion => Conflict("PLAN_CHANGED"),
            RegistrationBoundaryOutcome.NotFound => new(
                RegistrationEndpointOutcome.NotFound,
                ErrorCode: "REGISTRATION_CONTEXT_NOT_FOUND"),
            _ => new(RegistrationEndpointOutcome.Unavailable,
                ErrorCode: "REGISTRATION_UNAVAILABLE"),
        };
    }

    public async Task<RegistrationEndpointResult> LookupAsync(
        Guid applicationUserId,
        Guid termId,
        Guid clientRequestId,
        CancellationToken cancellationToken = default)
    {
        var context = await ResolveCommandContextAsync(
            applicationUserId, termId, cancellationToken);
        if (context is null || clientRequestId == Guid.Empty)
        {
            return new(RegistrationEndpointOutcome.NotFound, ErrorCode: "REQUEST_NOT_FOUND");
        }

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "SET LOCK_TIMEOUT 500;", cancellationToken);
            var submission = await dbContext.Set<RegistrationSubmission>()
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate =>
                    candidate.StudentId == context.ResolvedContext.StudentId &&
                    candidate.TermId == termId &&
                    candidate.ClientRequestId == clientRequestId,
                    cancellationToken);
            return submission?.IsFinal == true
                ? new(RegistrationEndpointOutcome.Replayed,
                    ToStoredFinal(submission))
                : new(RegistrationEndpointOutcome.NotFound,
                    ErrorCode: "REQUEST_NOT_FOUND");
        }
        catch (Microsoft.Data.SqlClient.SqlException exception)
            when (exception.Number == 1222)
        {
            return Processing(termId, clientRequestId);
        }
        finally
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "SET LOCK_TIMEOUT -1;", CancellationToken.None);
        }
    }

    public async Task LockRegistrationContextAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        _ = await dbContext.Set<RegistrationWindow>()
            .FromSqlInterpolated($"""
                SELECT * FROM [academics].[RegistrationWindows] WITH (UPDLOCK, HOLDLOCK)
                WHERE [TermId] = {termId}
                """)
            .ToArrayAsync(cancellationToken);
    }

    public async Task LockSerializablePublicationScopeRangeAsync(
        RegistrationPublicationScope scope,
        string normalizedScopeCode,
        CancellationToken cancellationToken)
    {
        if (scope is RegistrationPublicationScope.Catalogue)
        {
            _ = await dbContext.Set<CatalogueVersion>()
                .FromSqlInterpolated($"""
                    SELECT * FROM [academic].[CatalogueVersions] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [ScopeCode] = {normalizedScopeCode}
                    """)
                .ToArrayAsync(cancellationToken);
            return;
        }

        _ = await dbContext.Set<PolicySet>()
            .FromSqlInterpolated($"""
                SELECT * FROM [academic].[PolicySets] WITH (UPDLOCK, HOLDLOCK)
                WHERE [ScopeCode] = {normalizedScopeCode}
                """)
            .ToArrayAsync(cancellationToken);
    }

    public async Task LockSectionGroupVersionsAsync(
        IReadOnlyList<Guid> sortedGroupIds,
        CancellationToken cancellationToken)
    {
        foreach (var groupId in sortedGroupIds)
        {
            _ = await dbContext.Set<SectionGroup>()
                .FromSqlInterpolated($"""
                    SELECT * FROM [scheduling].[SectionGroups] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [Id] = {groupId}
                    """)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }

    public async Task ReReadAndValidateAsync(
        RegistrationFinalValidation validation,
        CancellationToken cancellationToken)
    {
        _pendingRejectionCode = null;
        var plan = await dbContext.Set<RegistrationPlan>()
            .AsNoTracking()
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == validation.PlanId &&
                candidate.StudentId == validation.StudentId &&
                candidate.TermId == validation.TermId,
                cancellationToken);
        if (plan is null ||
            !string.Equals(Convert.ToBase64String(plan.Version),
                validation.ExpectedPlanRowVersion, StringComparison.Ordinal))
        {
            _pendingRejectionCode = "PLAN_CHANGED";
            return;
        }

        if (plan.ReviewBlocked || plan.Conflicts.Count > 0)
        {
            _pendingRejectionCode = "SCHEDULE_CONFLICT";
            return;
        }

        var maximumCredits = await dbContext.Set<Student>()
            .Where(student => student.Id == validation.StudentId)
            .Select(student => student.CurrentGpa < 2m ? 12m : 18m)
            .SingleAsync(cancellationToken);
        if (plan.TotalCredits < 9m || plan.TotalCredits > maximumCredits)
        {
            _pendingRejectionCode = "POLICY_CHANGED";
            return;
        }

        var groups = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(group => validation.GroupIds.Contains(group.Id))
            .ToArrayAsync(cancellationToken);
        if (groups.Length != validation.GroupIds.Count ||
            groups.Any(group => !group.IsSelectable))
        {
            _pendingRejectionCode = "GROUP_FULL";
        }
    }

    private async Task FinalizeRejectedAsync(
        RegistrationSubmission submission,
        string reason,
        RegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var decision = Decision(reason, command);
        await submissionStore.FinalizeRejectedAsync(
            submission,
            reason,
            JsonSerializer.Serialize(decision),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);
    }

    private DecisionSnapshot Decision(string resultCode, RegistrationCommand command) =>
        new(
            _currentSnapshot!.PolicyVersion,
            _currentSnapshot.AcademicContextVersion,
            command.ExpectedPlanRowVersion,
            _currentSnapshot.Groups.ToDictionary(
                group => group.GroupId,
                group => group.GroupVersion),
            resultCode);

    private static RegistrationEndpointResult? ToVisibleResult(
        SubmissionClaimResult claim,
        Guid termId,
        Guid clientRequestId) => claim.Status switch
        {
            SubmissionClaimStatus.Replayed when claim.Submission is not null =>
                new(RegistrationEndpointOutcome.Replayed,
                    ToStoredFinal(claim.Submission)),
            SubmissionClaimStatus.InProgress => Processing(termId, clientRequestId),
            SubmissionClaimStatus.PayloadMismatch => Conflict("IDEMPOTENCY_KEY_REUSED"),
            _ => null,
        };

    private static RegistrationEndpointResult Processing(
        Guid termId,
        Guid clientRequestId) => new(
            RegistrationEndpointOutcome.Processing,
            InProgress: new(
                clientRequestId,
                "processing",
                1,
                $"/api/student/terms/{termId:D}/registrations/by-request/{clientRequestId:D}"));

    private static RegistrationEndpointResult Conflict(string code) =>
        new(RegistrationEndpointOutcome.Conflict, ErrorCode: code);

    private static RegistrationFinalResult ToStoredFinal(
        RegistrationSubmission submission)
    {
        var receipt = string.IsNullOrWhiteSpace(submission.ReceiptSnapshotJson)
            ? null
            : JsonSerializer.Deserialize<RegistrationReceiptSnapshotDto>(
                submission.ReceiptSnapshotJson);
        var decision = JsonSerializer.Deserialize<DecisionSnapshot>(
            submission.DecisionSnapshotJson!);
        return ToFinal(
            submission,
            receipt,
            decision!,
            receipt?.Groups ?? []);
    }

    private static RegistrationFinalResult ToFinal(
        RegistrationSubmission submission,
        RegistrationReceiptSnapshotDto? receipt,
        DecisionSnapshot decision,
        IReadOnlyList<RegistrationGroupSnapshotDto> groups) => new(
            submission.Id,
            submission.ProcessingState == RegistrationSubmissionState.Accepted
                ? "accepted"
                : "rejected",
            submission.ResultCode!,
            groups,
            submission.ReceivedAtUtc,
            submission.CompletedAtUtc!.Value,
            receipt?.PolicySetId ?? Guid.Empty,
            receipt?.PolicyVersion ?? decision.PolicyVersion,
            decision.PlanVersion,
            submission.Reference,
            receipt);

    private static RegistrationGroupSnapshotDto ToGroup(
        RegistrationPlanGroupSnapshot group) => new(
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
                Staff(meeting))).ToArray());

    private static IReadOnlyList<RegistrationMeetingStaffSnapshotDto> Staff(
        RegistrationPlanMeetingSnapshot meeting)
    {
        var staff = new List<RegistrationMeetingStaffSnapshotDto>();
        if (!string.IsNullOrWhiteSpace(meeting.LecturerName))
        {
            staff.Add(new("Lecturer", meeting.LecturerName));
        }

        staff.AddRange(meeting.TeachingAssistantNames.Select(name =>
            new RegistrationMeetingStaffSnapshotDto("TeachingAssistant", name)));
        return staff;
    }

    private static string NewReference() =>
        $"REG-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
}
