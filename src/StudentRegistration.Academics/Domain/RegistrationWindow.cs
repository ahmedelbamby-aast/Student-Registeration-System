using StudentRegistration.Contracts;

namespace StudentRegistration.Academics.Domain;

public enum RegistrationWindowScopeType
{
    AllStudents,
    Program,
    Cohort
}

public enum RegistrationWindowLifecycleState
{
    Draft,
    Published,
    EmergencyClosed,
    Superseded
}

public sealed class RegistrationWindow
{
    private RegistrationWindow()
    {
    }

    public RegistrationWindow(
        Guid id,
        Guid termId,
        RegistrationWindowScopeType scopeType,
        string? scopeValue,
        DateTime opensAtUtc,
        DateTime closesAtUtc,
        RegistrationWindowLifecycleState lifecycleState)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A registration window identifier is required.",
                nameof(id));
        }

        if (termId == Guid.Empty)
        {
            throw new ArgumentException("An academic term is required.", nameof(termId));
        }

        if (!Enum.IsDefined(scopeType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(scopeType),
                scopeType,
                "A declared registration-window scope is required.");
        }

        if (!Enum.IsDefined(lifecycleState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(lifecycleState),
                lifecycleState,
                "A declared registration-window lifecycle is required.");
        }

        EnsureUtc(opensAtUtc, nameof(opensAtUtc));
        EnsureUtc(closesAtUtc, nameof(closesAtUtc));
        if (closesAtUtc <= opensAtUtc)
        {
            throw new ArgumentException(
                "The close instant must be after the open instant.",
                nameof(closesAtUtc));
        }

        Id = id;
        TermId = termId;
        ScopeType = scopeType;
        ScopeValue = NormalizeScope(scopeType, scopeValue);
        OpensAtUtc = opensAtUtc;
        ClosesAtUtc = closesAtUtc;
        State = lifecycleState;
    }

    public Guid Id { get; private set; }

    public Guid TermId { get; private set; }

    public RegistrationWindowScopeType ScopeType { get; private set; }

    public string? ScopeValue { get; private set; }

    public DateTime OpensAtUtc { get; private set; }

    public DateTime ClosesAtUtc { get; private set; }

    public RegistrationWindowLifecycleState State { get; private set; }

    public byte[] Version { get; private set; } = [];

    public RegistrationWindowState GetComputedState(DateTime serverNowUtc)
    {
        EnsureUtc(serverNowUtc, nameof(serverNowUtc));
        if (State is not RegistrationWindowLifecycleState.Published)
        {
            return RegistrationWindowState.Closed;
        }

        if (serverNowUtc < OpensAtUtc)
        {
            return RegistrationWindowState.Upcoming;
        }

        return serverNowUtc < ClosesAtUtc
            ? RegistrationWindowState.Open
            : RegistrationWindowState.Closed;
    }

    public void Publish()
    {
        if (State is not RegistrationWindowLifecycleState.Draft)
        {
            throw new InvalidOperationException("Only a draft window can be published.");
        }

        State = RegistrationWindowLifecycleState.Published;
    }

    public void EmergencyClose()
    {
        if (State is not RegistrationWindowLifecycleState.Published)
        {
            throw new InvalidOperationException(
                "Only a published window can be closed in an emergency.");
        }

        State = RegistrationWindowLifecycleState.EmergencyClosed;
    }

    public void Supersede()
    {
        if (State is not RegistrationWindowLifecycleState.Published)
        {
            throw new InvalidOperationException(
                "Only a published window can be superseded.");
        }

        State = RegistrationWindowLifecycleState.Superseded;
    }

    private static string? NormalizeScope(
        RegistrationWindowScopeType scopeType,
        string? scopeValue)
    {
        if (scopeType is RegistrationWindowScopeType.AllStudents)
        {
            if (scopeValue is not null)
            {
                throw new ArgumentException(
                    "An all-students window cannot have a scope value.",
                    nameof(scopeValue));
            }

            return null;
        }

        var normalized = scopeValue?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "A program or cohort scope value is required.",
                nameof(scopeValue));
        }

        return normalized;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind is not DateTimeKind.Utc)
        {
            throw new ArgumentException("A UTC instant is required.", parameterName);
        }
    }
}
