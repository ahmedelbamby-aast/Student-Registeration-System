using StudentRegistration.Academics.Application;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;
using StudentRegistration.IdentityAccess.Application;

namespace StudentRegistration.Api.Composition;

public sealed record AcademicSessionContextAdapterOptions
{
    public AcademicSessionContextAdapterOptions(string supportReferencePath)
    {
        var path = supportReferencePath?.Trim();
        if (string.IsNullOrWhiteSpace(path) ||
            !path.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains('?') ||
            path.Contains('#'))
        {
            throw new ArgumentException(
                "A canonical application-relative support path is required.",
                nameof(supportReferencePath));
        }

        SupportReferencePath = path;
    }

    public string SupportReferencePath { get; }
}

public sealed class AcademicSessionContextAdapter(
    SessionLifecycleService sessionLifecycleService,
    AcademicSessionContextAdapterOptions options) : IAcademicSessionContextAdapter
{
    private readonly SessionLifecycleService _sessionLifecycleService =
        sessionLifecycleService ?? throw new ArgumentNullException(nameof(sessionLifecycleService));
    private readonly AcademicSessionContextAdapterOptions _options =
        options ?? throw new ArgumentNullException(nameof(options));

    public async Task<AppContextDto?> ComposeAsync(
        Guid applicationUserId,
        string? activeRole,
        AcademicContextResult academicContext,
        CancellationToken cancellationToken = default)
    {
        if (applicationUserId == Guid.Empty)
        {
            return null;
        }

        ArgumentNullException.ThrowIfNull(academicContext);
        var result = await _sessionLifecycleService.GetSessionAsync(
            applicationUserId,
            activeRole,
            cancellationToken);
        if (!result.Succeeded ||
            result.UserId != applicationUserId ||
            string.IsNullOrWhiteSpace(result.DisplayName) ||
            result.ExpiresAtUtc is null ||
            result.AuthorizedRoles.Count != 1 ||
            !string.Equals(result.AuthorizedRoles[0], result.ActiveRole, StringComparison.Ordinal))
        {
            return null;
        }

        var session = new SessionDto(
            result.DisplayName,
            result.AuthorizedRoles,
            result.ActiveRole,
            "active",
            result.ExpiresAtUtc.Value);

        return new AppContextDto(
            academicContext.ServerTimeUtc,
            academicContext.TimeZoneId,
            academicContext.TeachingTerm,
            academicContext.RegistrationTerm,
            academicContext.RegistrationWindowState,
            academicContext.RegistrationWindow,
            academicContext.ServiceState,
            session.DisplayName,
            session.Roles,
            session.ActiveRole,
            ToSessionState(session.SessionState),
            session.ExpiresAtUtc,
            _options.SupportReferencePath);
    }

    private static SessionState ToSessionState(string value) => value switch
    {
        "active" => SessionState.Active,
        "expiring" => SessionState.Expiring,
        _ => throw new InvalidOperationException("The identity session state is unsupported.")
    };
}
