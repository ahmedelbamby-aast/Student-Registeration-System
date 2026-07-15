using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts;

namespace StudentRegistration.Academics.Application;

public sealed record AcademicContextOptions
{
    public AcademicContextOptions(string timeZoneId)
    {
        TimeZoneId = RequireIanaTimeZone(timeZoneId);
    }

    public string TimeZoneId { get; }

    private static string RequireIanaTimeZone(string value)
    {
        var timeZoneId = value?.Trim();
        if (string.IsNullOrWhiteSpace(timeZoneId) ||
            timeZoneId.Length > 100 ||
            !timeZoneId.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A valid IANA timezone identifier is required.",
                nameof(value));
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ArgumentException(
                "A known IANA timezone identifier is required.",
                nameof(value),
                exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ArgumentException(
                "A valid IANA timezone identifier is required.",
                nameof(value),
                exception);
        }

        return timeZoneId;
    }
}

public sealed record AcademicContextResult
{
    public AcademicContextResult(
        DateTime serverTimeUtc,
        string timeZoneId,
        TermSummaryDto? teachingTerm,
        TermSummaryDto? registrationTerm,
        RegistrationWindowState registrationWindowState,
        RegistrationWindowSummaryDto? registrationWindow,
        ServiceState serviceState)
    {
        ServerTimeUtc = serverTimeUtc;
        TimeZoneId = timeZoneId;
        TeachingTerm = teachingTerm;
        RegistrationTerm = registrationTerm;
        RegistrationWindowState = registrationWindowState;
        RegistrationWindow = registrationWindow;
        ServiceState = serviceState;
    }

    public DateTime ServerTimeUtc { get; }

    public string TimeZoneId { get; }

    public TermSummaryDto? TeachingTerm { get; }

    public TermSummaryDto? RegistrationTerm { get; }

    public RegistrationWindowState RegistrationWindowState { get; }

    public RegistrationWindowSummaryDto? RegistrationWindow { get; }

    public ServiceState ServiceState { get; }
}

public sealed class AcademicContextUnavailableException : Exception
{
    public const string ErrorCode = "CONTEXT_UNAVAILABLE";

    public AcademicContextUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public string Code => ErrorCode;

    public AcademicContextResult? PartialContext => null;
}

public sealed class AcademicContextResolver
{
    private readonly IAcademicContextReader _reader;
    private readonly TimeProvider _timeProvider;
    private readonly AcademicContextOptions _options;

    public AcademicContextResolver(
        IAcademicContextReader reader,
        TimeProvider timeProvider,
        AcademicContextOptions options)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<AcademicContextResult> ResolveAsync(
        AcademicStudentScope? studentScope = null,
        CancellationToken cancellationToken = default) =>
        ResolveCoreAsync(studentScope, cancellationToken);

    public async Task<PublicContextDto> ResolvePublicAsync(
        CancellationToken cancellationToken = default)
    {
        var context = await ResolveCoreAsync(studentScope: null, cancellationToken);
        return new PublicContextDto(
            context.ServerTimeUtc,
            context.TimeZoneId,
            context.TeachingTerm?.Label,
            context.RegistrationTerm?.Label,
            context.RegistrationWindowState,
            context.ServiceState);
    }

    private async Task<AcademicContextResult> ResolveCoreAsync(
        AcademicStudentScope? studentScope,
        CancellationToken cancellationToken)
    {
        var serverNowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            var snapshot = await _reader.ResolveContextAsync(
                studentScope,
                cancellationToken);
            ArgumentNullException.ThrowIfNull(snapshot);
            var terms = snapshot.Terms;

            var teachingTerm = SingleCurrentTerm(terms, TermState.Teaching);
            var registrationTerm = SingleCurrentTerm(terms, TermState.RegistrationOpen);
            EnsureConfiguredTimeZone(teachingTerm);
            EnsureConfiguredTimeZone(registrationTerm);

            if (registrationTerm is null)
            {
                return new AcademicContextResult(
                    serverNowUtc,
                    _options.TimeZoneId,
                    ToSummary(teachingTerm),
                    registrationTerm: null,
                    RegistrationWindowState.None,
                    registrationWindow: null,
                    ServiceState.Available);
            }

            var selectedWindow = SelectWindow(
                snapshot.PublishedWindows.Where(window =>
                    window.TermId == registrationTerm.Id &&
                    (studentScope is null || window.AppliesTo(studentScope))),
                serverNowUtc);
            var registrationWindowState = selectedWindow is null
                ? RegistrationWindowState.None
                : StateAt(selectedWindow, serverNowUtc);
            var registrationWindow = selectedWindow is null
                ? null
                : new RegistrationWindowSummaryDto(
                    selectedWindow.Id.ToString(),
                    registrationWindowState,
                    selectedWindow.OpensAtUtc,
                    selectedWindow.ClosesAtUtc,
                    Convert.ToBase64String(selectedWindow.RowVersion));

            return new AcademicContextResult(
                serverNowUtc,
                _options.TimeZoneId,
                ToSummary(teachingTerm),
                ToSummary(registrationTerm),
                registrationWindowState,
                registrationWindow,
                ServiceState.Available);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AcademicContextUnavailableException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw ContextUnavailable(
                "The authoritative academic context could not be resolved.",
                exception);
        }
    }

    private static AcademicTermContextRecord? SingleCurrentTerm(
        IReadOnlyList<AcademicTermContextRecord> terms,
        TermState state)
    {
        var matches = terms.Where(term => term.State == state).ToArray();
        if (matches.Length > 1)
        {
            throw ContextUnavailable(
                $"More than one {state} academic term is configured.");
        }

        return matches.SingleOrDefault();
    }

    private void EnsureConfiguredTimeZone(AcademicTermContextRecord? term)
    {
        if (term is not null &&
            !string.Equals(
                term.TimeZoneId,
                _options.TimeZoneId,
                StringComparison.Ordinal))
        {
            throw ContextUnavailable(
                "A current academic term uses a different institutional timezone.");
        }
    }

    private static RegistrationWindowContextRecord? SelectWindow(
        IEnumerable<RegistrationWindowContextRecord> source,
        DateTime serverNowUtc)
    {
        var windows = source.ToArray();
        var open = windows
            .Where(window => StateAt(window, serverNowUtc) is RegistrationWindowState.Open)
            .OrderBy(window => window.OpensAtUtc)
            .ThenBy(window => window.Id)
            .ToArray();
        if (open.Length > 1)
        {
            throw ContextUnavailable(
                "More than one published registration window is open.");
        }

        if (open.Length == 1)
        {
            return open[0];
        }

        var upcoming = windows
            .Where(window => StateAt(window, serverNowUtc) is RegistrationWindowState.Upcoming)
            .OrderBy(window => window.OpensAtUtc)
            .ThenBy(window => window.Id)
            .FirstOrDefault();
        if (upcoming is not null)
        {
            return upcoming;
        }

        return windows
            .Where(window => StateAt(window, serverNowUtc) is RegistrationWindowState.Closed)
            .OrderByDescending(window => window.ClosesAtUtc)
            .ThenBy(window => window.Id)
            .FirstOrDefault();
    }

    private static RegistrationWindowState StateAt(
        RegistrationWindowContextRecord window,
        DateTime serverNowUtc) =>
        serverNowUtc < window.OpensAtUtc
            ? RegistrationWindowState.Upcoming
            : serverNowUtc < window.ClosesAtUtc
                ? RegistrationWindowState.Open
                : RegistrationWindowState.Closed;

    private static TermSummaryDto? ToSummary(AcademicTermContextRecord? term) =>
        term is null
            ? null
            : new TermSummaryDto(
                term.Id.ToString(),
                term.Code,
                term.Label,
                term.State,
                Convert.ToBase64String(term.RowVersion));

    private static AcademicContextUnavailableException ContextUnavailable(
        string message,
        Exception? innerException = null) =>
        new(message, innerException);
}
