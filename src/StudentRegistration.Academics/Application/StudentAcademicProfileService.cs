using System.Text.Json;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Contracts.Auditing;

namespace StudentRegistration.Academics.Application;

public enum AcademicProfileOutcome
{
    Succeeded,
    ValidationError,
    PageSizeInvalid,
    NotFound,
    StaleVersion,
    InvalidSupersession,
    ProfileNotReady,
    StorageUnavailable
}

public sealed record AcademicProfileResult<TProfile>(
    AcademicProfileOutcome Outcome,
    TProfile? Profile = null,
    string? CurrentVersion = null)
    where TProfile : class
{
    public string? ErrorCode => Outcome switch
    {
        AcademicProfileOutcome.ValidationError => "VALIDATION_ERROR",
        AcademicProfileOutcome.PageSizeInvalid => "PAGE_SIZE_INVALID",
        AcademicProfileOutcome.NotFound => "PROFILE_NOT_FOUND",
        AcademicProfileOutcome.StaleVersion => "STALE_VERSION",
        AcademicProfileOutcome.InvalidSupersession => "INVALID_SUPERSESSION",
        AcademicProfileOutcome.ProfileNotReady => "PROFILE_NOT_READY",
        AcademicProfileOutcome.StorageUnavailable => "CONTEXT_UNAVAILABLE",
        _ => null
    };
}

public enum RegistrationBoundaryOutcome
{
    Committed,
    HoldBlocked,
    StaleVersion,
    NotFound,
    ProfileNotReady,
    StorageUnavailable
}

public sealed record RegistrationBoundaryResult(RegistrationBoundaryOutcome Outcome)
{
    public string? ErrorCode => Outcome switch
    {
        RegistrationBoundaryOutcome.HoldBlocked => "HOLD_BLOCKED",
        RegistrationBoundaryOutcome.StaleVersion => "STALE_VERSION",
        RegistrationBoundaryOutcome.NotFound => "PROFILE_NOT_FOUND",
        RegistrationBoundaryOutcome.ProfileNotReady => "PROFILE_NOT_READY",
        RegistrationBoundaryOutcome.StorageUnavailable => "CONTEXT_UNAVAILABLE",
        _ => null
    };
}

public sealed class StudentAcademicProfileService
{
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;
    private const int MaximumActiveHolds = 100;
    private const int MaximumCorrectionOperations = 20;
    private const string StudentTermBoundaryName = nameof(StudentTermAcademicState);

    private readonly IStudentAcademicProfileStore _store;
    private readonly TimeProvider _timeProvider;

    public StudentAcademicProfileService(
        IStudentAcademicProfileStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<AcademicProfileResult<StudentAcademicContextDto>> ReadOwnAsync(
        Guid applicationUserId,
        int transcriptPage = 1,
        int transcriptPageSize = DefaultPageSize,
        int provenancePage = 1,
        int provenancePageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "An application user identifier is required.",
                nameof(applicationUserId));
        }

        var request = CreateReadRequest(
            transcriptPage,
            transcriptPageSize,
            provenancePage,
            provenancePageSize);
        if (request is null)
        {
            return new(AcademicProfileOutcome.PageSizeInvalid);
        }

        var stored = await _store.ReadByApplicationUserIdAsync(
            applicationUserId,
            request,
            cancellationToken);
        return ToStudentResult(stored, request.ServerNowUtc);
    }

    public async Task<AcademicProfileResult<AdminStudentAcademicContextDto>> ReadAdminAsync(
        Guid studentId,
        Guid termId,
        int transcriptPage = 1,
        int transcriptPageSize = DefaultPageSize,
        int provenancePage = 1,
        int provenancePageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        RequireId(studentId, nameof(studentId));
        RequireId(termId, nameof(termId));
        var request = CreateReadRequest(
            transcriptPage,
            transcriptPageSize,
            provenancePage,
            provenancePageSize);
        if (request is null)
        {
            return new(AcademicProfileOutcome.PageSizeInvalid);
        }

        var stored = await _store.ReadByStudentIdAsync(
            studentId,
            termId,
            request,
            cancellationToken);
        return ToAdminResult(stored, request.ServerNowUtc);
    }

    public async Task<AcademicProfileResult<AdminStudentAcademicContextDto>> CorrectProfileAsync(
        Guid studentId,
        AcademicProfileCorrectionRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default)
    {
        RequireId(studentId, nameof(studentId));
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        if (request.Operations.Count is < 1 or > MaximumCorrectionOperations)
        {
            return new(AcademicProfileOutcome.ValidationError);
        }

        if (HasConflictingOperations(request.Operations))
        {
            return new(AcademicProfileOutcome.ValidationError);
        }

        var termId = ParseId(request.TermId, nameof(request.TermId));
        var responseRead = CreateReadRequest(1, DefaultPageSize, 1, DefaultPageSize)!;
        var mutations = request.Operations.Select(ToMutation).ToArray();
        var audit = new AuditEventDraft(
            context.ActorReference,
            studentId.ToString("D"),
            "academic-profile-corrected",
            StudentTermBoundaryName,
            termId.ToString("D"),
            request.Reason,
            BeforeSummaryJson: null,
            AfterSummaryJson: JsonSerializer.Serialize(new
            {
                operationCount = mutations.Length,
                source = request.Source
            }),
            context.CorrelationId,
            responseRead.ServerNowUtc);
        var command = new CorrectAcademicProfileStoreCommand(
            studentId,
            termId,
            ParseVersion(
                request.ExpectedStudentRowVersion,
                nameof(request.ExpectedStudentRowVersion)),
            ParseVersion(
                request.ExpectedStudentTermStateRowVersion,
                nameof(request.ExpectedStudentTermStateRowVersion)),
            request.Source,
            mutations,
            responseRead,
            audit);

        var stored = await _store.CorrectAsync(command, cancellationToken);
        return ToAdminResult(stored, responseRead.ServerNowUtc);
    }

    public async Task<RegistrationBoundaryResult> ExecuteRegistrationBoundaryAsync(
        Guid studentId,
        Guid termId,
        string expectedStudentTermStateRowVersion,
        Func<CancellationToken, Task> commitCallback,
        CancellationToken cancellationToken = default)
    {
        RequireId(studentId, nameof(studentId));
        RequireId(termId, nameof(termId));
        ArgumentNullException.ThrowIfNull(commitCallback);
        var command = new RegistrationBoundaryStoreCommand(
            studentId,
            termId,
            ParseVersion(
                expectedStudentTermStateRowVersion,
                nameof(expectedStudentTermStateRowVersion)),
            _timeProvider.GetUtcNow().UtcDateTime);
        var result = await _store.ExecuteRegistrationBoundaryAsync(
            command,
            commitCallback,
            cancellationToken);

        return new(result.Outcome switch
        {
            AcademicProfileStoreOutcome.Succeeded => RegistrationBoundaryOutcome.Committed,
            AcademicProfileStoreOutcome.HoldBlocked => RegistrationBoundaryOutcome.HoldBlocked,
            AcademicProfileStoreOutcome.StaleVersion => RegistrationBoundaryOutcome.StaleVersion,
            AcademicProfileStoreOutcome.NotFound => RegistrationBoundaryOutcome.NotFound,
            AcademicProfileStoreOutcome.ProfileNotReady =>
                RegistrationBoundaryOutcome.ProfileNotReady,
            _ => RegistrationBoundaryOutcome.StorageUnavailable
        });
    }

    private AcademicProfileReadRequest? CreateReadRequest(
        int transcriptPage,
        int transcriptPageSize,
        int provenancePage,
        int provenancePageSize)
    {
        if (!IsValidPage(transcriptPage, transcriptPageSize) ||
            !IsValidPage(provenancePage, provenancePageSize))
        {
            return null;
        }

        return new(
            transcriptPage,
            transcriptPageSize,
            provenancePage,
            provenancePageSize,
            MaximumActiveHolds + 1,
            _timeProvider.GetUtcNow().UtcDateTime);
    }

    private static AcademicProfileResult<StudentAcademicContextDto> ToStudentResult(
        AcademicProfileStoreResult stored,
        DateTime serverNowUtc)
    {
        var failure = MapFailure<StudentAcademicContextDto>(stored);
        if (failure is not null)
        {
            return failure;
        }

        var profile = stored.Profile!;
        var activeHolds = ActiveHolds(profile, serverNowUtc);
        if (activeHolds is null || profile.Provenance.TotalCount == 0)
        {
            return new(AcademicProfileOutcome.ProfileNotReady);
        }

        return new(
            AcademicProfileOutcome.Succeeded,
            new StudentAcademicContextDto(
                profile.UniversityId,
                profile.Student.ProgramCode,
                profile.Student.Cohort,
                profile.Student.CurrentGpa,
                profile.Student.EarnedCredits,
                profile.Student.Standing,
                profile.TranscriptSummary,
                profile.TranscriptAttempts,
                activeHolds.Select(ToStudentHold).ToArray(),
                profile.Student.DataVersion,
                profile.Student.DataAsOfUtc,
                profile.Provenance));
    }

    private static AcademicProfileResult<AdminStudentAcademicContextDto> ToAdminResult(
        AcademicProfileStoreResult stored,
        DateTime serverNowUtc)
    {
        var failure = MapFailure<AdminStudentAcademicContextDto>(stored);
        if (failure is not null)
        {
            return failure;
        }

        var profile = stored.Profile!;
        var activeHolds = ActiveHolds(profile, serverNowUtc);
        if (activeHolds is null ||
            profile.Provenance.TotalCount == 0 ||
            profile.StudentRowVersion is not { Length: > 0 } ||
            profile.StudentTermStateRowVersion is not { Length: > 0 })
        {
            return new(AcademicProfileOutcome.ProfileNotReady);
        }

        return new(
            AcademicProfileOutcome.Succeeded,
            new AdminStudentAcademicContextDto(
                profile.Student.Id.ToString("D"),
                profile.StudentTermState.TermId.ToString("D"),
                profile.UniversityId,
                profile.Student.ProgramCode,
                profile.Student.Cohort,
                profile.Student.CurrentGpa,
                profile.Student.EarnedCredits,
                profile.Student.Standing,
                profile.TranscriptSummary,
                profile.TranscriptAttempts,
                activeHolds.Select(ToAdminHold).ToArray(),
                profile.Student.DataVersion,
                profile.Student.DataAsOfUtc,
                profile.Provenance,
                Convert.ToBase64String(profile.StudentRowVersion),
                Convert.ToBase64String(profile.StudentTermStateRowVersion)));
    }

    private static AcademicProfileResult<TProfile>? MapFailure<TProfile>(
        AcademicProfileStoreResult stored)
        where TProfile : class
    {
        if (stored.Outcome is AcademicProfileStoreOutcome.Succeeded &&
            stored.Profile is not null)
        {
            return null;
        }

        return new(
            stored.Outcome switch
            {
                AcademicProfileStoreOutcome.NotFound => AcademicProfileOutcome.NotFound,
                AcademicProfileStoreOutcome.StaleVersion => AcademicProfileOutcome.StaleVersion,
                AcademicProfileStoreOutcome.InvalidSupersession =>
                    AcademicProfileOutcome.InvalidSupersession,
                AcademicProfileStoreOutcome.ProfileNotReady =>
                    AcademicProfileOutcome.ProfileNotReady,
                _ => AcademicProfileOutcome.StorageUnavailable
            },
            CurrentVersion: stored.CurrentVersion);
    }

    private static IReadOnlyList<StudentHold>? ActiveHolds(
        AcademicProfileSnapshot profile,
        DateTime serverNowUtc)
    {
        var active = profile.ActiveHolds
            .Where(hold => hold.IsActiveAt(serverNowUtc))
            .ToArray();
        return active.Length <= MaximumActiveHolds ? active : null;
    }

    private static AcademicHoldDto ToStudentHold(StudentHold hold) =>
        new(
            hold.TermId.ToString("D"),
            hold.Code,
            hold.Message,
            hold.BlocksRegistration,
            hold.EffectiveFromUtc,
            hold.EffectiveToUtc,
            hold.Source);

    private static AdminAcademicHoldDto ToAdminHold(StudentHold hold) =>
        new(
            hold.TermId.ToString("D"),
            hold.Code,
            hold.Message,
            hold.BlocksRegistration,
            hold.EffectiveFromUtc,
            hold.EffectiveToUtc,
            hold.Source,
            hold.Id.ToString("D"),
            hold.SourceReference);

    private static AcademicProfileMutation ToMutation(
        AcademicProfileCorrectionOperation operation) =>
        operation switch
        {
            SetGpaOperation value => new SetGpaMutation(
                value.CurrentGpa,
                value.SourceReference),
            SetEarnedCreditsOperation value => new SetEarnedCreditsMutation(
                value.EarnedCredits,
                value.SourceReference),
            SetStandingOperation value => new SetStandingMutation(
                value.StandingCode,
                value.SourceReference),
            UpsertTranscriptAttemptOperation value =>
                new AppendTranscriptAttemptMutation(
                    Guid.NewGuid(),
                    ParseOptionalId(
                        value.SupersedesAttemptId,
                        nameof(value.SupersedesAttemptId)),
                    value.CourseCode,
                    value.TermCode,
                    value.Credits,
                    value.Grade,
                    ParseStatus(value.Status),
                    value.SourceReference),
            UpsertHoldOperation value => new UpsertStudentHoldMutation(
                ParseOptionalId(value.HoldId, nameof(value.HoldId)),
                value.Code,
                value.Message,
                value.BlocksRegistration,
                value.EffectiveFromUtc,
                value.EffectiveToUtc,
                value.SourceReference),
            RemoveHoldOperation value => new RemoveStudentHoldMutation(
                ParseId(value.HoldId, nameof(value.HoldId)),
                value.SourceReference),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                "Unsupported academic-profile correction operation.")
        };

    private static bool HasConflictingOperations(
        IReadOnlyList<AcademicProfileCorrectionOperation> operations)
    {
        var hasGpa = false;
        var hasEarnedCredits = false;
        var hasStanding = false;
        var holdIds = new HashSet<Guid>();
        var supersededAttemptIds = new HashSet<Guid>();

        foreach (var operation in operations)
        {
            switch (operation)
            {
                case SetGpaOperation:
                    if (hasGpa)
                    {
                        return true;
                    }

                    hasGpa = true;
                    break;
                case SetEarnedCreditsOperation:
                    if (hasEarnedCredits)
                    {
                        return true;
                    }

                    hasEarnedCredits = true;
                    break;
                case SetStandingOperation:
                    if (hasStanding)
                    {
                        return true;
                    }

                    hasStanding = true;
                    break;
                case UpsertHoldOperation { HoldId: not null } hold:
                    if (!TryAddNonEmptyGuid(holdIds, hold.HoldId))
                    {
                        return true;
                    }

                    break;
                case RemoveHoldOperation hold:
                    if (!TryAddNonEmptyGuid(holdIds, hold.HoldId))
                    {
                        return true;
                    }

                    break;
                case UpsertTranscriptAttemptOperation { SupersedesAttemptId: not null }
                    attempt:
                    if (!TryAddNonEmptyGuid(
                            supersededAttemptIds,
                            attempt.SupersedesAttemptId))
                    {
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    private static bool TryAddNonEmptyGuid(ISet<Guid> identifiers, string value) =>
        Guid.TryParse(value, out var identifier) &&
        identifier != Guid.Empty &&
        identifiers.Add(identifier);

    private static TranscriptAttemptStatus ParseStatus(string status) => status switch
    {
        "in-progress" => TranscriptAttemptStatus.InProgress,
        "passed" => TranscriptAttemptStatus.Passed,
        "failed" => TranscriptAttemptStatus.Failed,
        "withdrawn" => TranscriptAttemptStatus.Withdrawn,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    private static bool IsValidPage(int page, int pageSize) =>
        page >= 1 && pageSize is >= 1 and <= MaximumPageSize;

    private static void RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A non-empty identifier is required.", parameterName);
        }
    }

    private static Guid ParseId(string value, string parameterName) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new ArgumentException("A non-empty GUID is required.", parameterName);

    private static Guid? ParseOptionalId(string? value, string parameterName) =>
        value is null ? null : ParseId(value, parameterName);

    private static byte[] ParseVersion(string value, string parameterName)
    {
        try
        {
            var version = Convert.FromBase64String(value);
            return version.Length > 0
                ? version
                : throw new ArgumentException("A row version is required.", parameterName);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                "A base64 row version is required.",
                parameterName,
                exception);
        }
    }
}
