using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Contracts.Auditing;
using ContractWindowLifecycle = StudentRegistration.Contracts.Academics.RegistrationWindowLifecycle;
using ContractWindowScope = StudentRegistration.Contracts.Academics.RegistrationWindowScope;
using DomainWindowLifecycle = StudentRegistration.Academics.Domain.RegistrationWindowLifecycleState;
using DomainWindowScope = StudentRegistration.Academics.Domain.RegistrationWindowScopeType;

namespace StudentRegistration.Academics.Application;

public sealed record AcademicCommandContext(
    string ActorReference,
    string CorrelationId)
{
    public string ActorReference { get; } = Required(ActorReference, nameof(ActorReference));
    public string CorrelationId { get; } = Required(CorrelationId, nameof(CorrelationId));

    private static string Required(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A non-empty value is required.", parameterName)
            : value.Trim();
}

public enum AcademicTermCommandOutcome
{
    Succeeded,
    Replayed,
    NotFound,
    StaleVersion,
    WindowOverlap,
    TermCodeExists,
    TermStateConflict,
    IdempotencyKeyReused,
    StorageUnavailable
}

public sealed record AcademicTermCommandResult(
    AcademicTermCommandOutcome Outcome,
    AdminTermDto? Term = null,
    string? CurrentVersion = null)
{
    public string? ErrorCode => Outcome switch
    {
        AcademicTermCommandOutcome.StaleVersion => "STALE_VERSION",
        AcademicTermCommandOutcome.WindowOverlap => "WINDOW_OVERLAP",
        AcademicTermCommandOutcome.TermCodeExists => "TERM_CODE_EXISTS",
        AcademicTermCommandOutcome.TermStateConflict => "TERM_STATE_CONFLICT",
        AcademicTermCommandOutcome.IdempotencyKeyReused => "IDEMPOTENCY_KEY_REUSED",
        AcademicTermCommandOutcome.NotFound => "NOT_FOUND",
        AcademicTermCommandOutcome.StorageUnavailable => "CONTEXT_UNAVAILABLE",
        _ => null
    };
}

public sealed class RegistrationWindowService
{
    private readonly IRegistrationWindowStore _store;
    private readonly TimeProvider _timeProvider;

    public RegistrationWindowService(
        IRegistrationWindowStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<AcademicTermCommandResult> CreateTermAsync(
        CreateTermRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var CreationClientRequestId = request.ClientRequestId;
        var CreationPayloadHash = ComputePayloadHash(request);
        if (request.Windows.Any(window =>
            window.LifecycleState is not ContractWindowLifecycle.Draft))
        {
            return new(AcademicTermCommandOutcome.TermStateConflict);
        }

        var term = new AcademicTerm(
            Guid.NewGuid(),
            request.Term.Code,
            CreationClientRequestId,
            CreationPayloadHash,
            request.Term.DisplayName,
            request.Term.TeachingStartsOn,
            request.Term.TeachingEndsOn,
            request.Term.TimeZoneId,
            request.Term.State);
        var windows = request.Windows
            .Select(window => ToDomainWindow(term.Id, window))
            .OrderBy(window => window.Id)
            .ToArray();
        var audit = Audit(
            context,
            term.Id,
            nameof(AcademicTerm),
            "academic-term-created",
            request.Reason,
            request.Source);

        var result = await _store.CreateOrReplayTermAsync(
            new CreateAcademicTermStoreCommand(term, windows, audit),
            cancellationToken);
        return FromCreationStore(result);
    }

    public async Task<AcademicTermCommandResult> UpdateTermAsync(
        Guid termId,
        UpdateTermRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default)
    {
        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term ID is required.", nameof(termId));
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        var versions = request.ExpectedWindowRowVersions.ToDictionary(
            pair => ParseId(pair.Key, nameof(request.ExpectedWindowRowVersions)),
            pair => ParseVersion(pair.Value, nameof(request.ExpectedWindowRowVersions)));
        var existingWindowIds = request.Windows
            .Where(window => window.Id is not null)
            .Select(window => ParseId(window.Id!, nameof(request.Windows)))
            .Order()
            .ToArray();
        if (!existingWindowIds.SequenceEqual(versions.Keys.Order()))
        {
            throw new ArgumentException(
                "Existing windows and expected window versions must have matching IDs.",
                nameof(request));
        }

        var command = new UpdateAcademicTermStoreCommand(
            termId,
            ParseVersion(request.ExpectedTermRowVersion, nameof(request.ExpectedTermRowVersion)),
            versions,
            request.Term,
            request.Windows,
            Audit(
                context,
                termId,
                nameof(AcademicTerm),
                "academic-term-updated",
                request.Reason,
                request.Source));
        return FromStore(await _store.UpdateTermAsync(command, cancellationToken));
    }

    public async Task<AcademicTermCommandResult> PublishAsync(
        Guid termId,
        Guid windowId,
        PublishRegistrationWindowRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default)
    {
        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term ID is required.", nameof(termId));
        }

        if (windowId == Guid.Empty)
        {
            throw new ArgumentException("A registration window ID is required.", nameof(windowId));
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        var ExpectedTermRowVersion = ParseVersion(
            request.ExpectedTermRowVersion,
            nameof(request.ExpectedTermRowVersion));
        var ExpectedWindowRowVersion = ParseVersion(
            request.ExpectedWindowRowVersion,
            nameof(request.ExpectedWindowRowVersion));
        var command = new PublishRegistrationWindowStoreCommand(
            termId,
            windowId,
            ExpectedTermRowVersion,
            ExpectedWindowRowVersion,
            Audit(
                context,
                windowId,
                nameof(RegistrationWindow),
                "registration-window-published",
                request.Reason,
                request.Source));

        return FromStore(await _store.PublishRegistrationWindowAsync(
            command,
            cancellationToken));
    }

    private AuditEventDraft Audit(
        AcademicCommandContext context,
        Guid entityId,
        string entityType,
        string action,
        string reason,
        string source) =>
        new(
            context.ActorReference,
            entityId.ToString("D"),
            action,
            entityType,
            entityId.ToString("D"),
            reason,
            BeforeSummaryJson: null,
            AfterSummaryJson: JsonSerializer.Serialize(new { source }),
            context.CorrelationId,
            _timeProvider.GetUtcNow().UtcDateTime);

    private static AcademicTermCommandResult FromCreationStore(
        AcademicTermCreationStoreResult result) => result.Outcome switch
        {
            AcademicTermCreationOutcome.Created when result.Term is not null =>
                new(AcademicTermCommandOutcome.Succeeded, result.Term),
            AcademicTermCreationOutcome.Replayed when result.Term is not null =>
                new(AcademicTermCommandOutcome.Replayed, result.Term),
            AcademicTermCreationOutcome.IdempotencyKeyReused =>
                new(AcademicTermCommandOutcome.IdempotencyKeyReused),
            AcademicTermCreationOutcome.TermCodeExists =>
                new(AcademicTermCommandOutcome.TermCodeExists),
            AcademicTermCreationOutcome.TermStateConflict =>
                new(AcademicTermCommandOutcome.TermStateConflict),
            _ => new(AcademicTermCommandOutcome.StorageUnavailable)
        };

    private static AcademicTermCommandResult FromStore(
        AcademicTermMutationStoreResult result) =>
        new(
            result.Outcome switch
            {
                AcademicTermMutationOutcome.Succeeded => AcademicTermCommandOutcome.Succeeded,
                AcademicTermMutationOutcome.NotFound => AcademicTermCommandOutcome.NotFound,
                AcademicTermMutationOutcome.StaleVersion => AcademicTermCommandOutcome.StaleVersion,
                AcademicTermMutationOutcome.WindowOverlap => AcademicTermCommandOutcome.WindowOverlap,
                AcademicTermMutationOutcome.TermCodeExists => AcademicTermCommandOutcome.TermCodeExists,
                AcademicTermMutationOutcome.TermStateConflict => AcademicTermCommandOutcome.TermStateConflict,
                _ => AcademicTermCommandOutcome.StorageUnavailable
            },
            result.Term,
            result.CurrentVersion);

    private static RegistrationWindow ToDomainWindow(
        Guid termId,
        TermWindowInput input) =>
        new(
            input.Id is null ? Guid.NewGuid() : ParseId(input.Id, nameof(input.Id)),
            termId,
            input.ScopeType switch
            {
                ContractWindowScope.AllStudents => DomainWindowScope.AllStudents,
                ContractWindowScope.Program => DomainWindowScope.Program,
                ContractWindowScope.Cohort => DomainWindowScope.Cohort,
                _ => throw new ArgumentOutOfRangeException(nameof(input.ScopeType))
            },
            input.ScopeValue,
            input.OpensAtUtc,
            input.ClosesAtUtc,
            input.LifecycleState switch
            {
                ContractWindowLifecycle.Draft => DomainWindowLifecycle.Draft,
                ContractWindowLifecycle.Published => DomainWindowLifecycle.Published,
                ContractWindowLifecycle.EmergencyClosed => DomainWindowLifecycle.EmergencyClosed,
                ContractWindowLifecycle.Superseded => DomainWindowLifecycle.Superseded,
                _ => throw new ArgumentOutOfRangeException(nameof(input.LifecycleState))
            });

    private static string ComputePayloadHash(CreateTermRequest request)
    {
        var payload = JsonSerializer.Serialize(new
        {
            request.Reason,
            request.Source,
            request.Term,
            request.Windows
        });
        return $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)))}";
    }

    private static Guid ParseId(string value, string parameterName) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new ArgumentException("A non-empty GUID is required.", parameterName);

    private static byte[] ParseVersion(string value, string parameterName)
    {
        try
        {
            var bytes = Convert.FromBase64String(value);
            return bytes.Length > 0
                ? bytes
                : throw new ArgumentException("A row version is required.", parameterName);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("A base64 row version is required.", parameterName, exception);
        }
    }
}
