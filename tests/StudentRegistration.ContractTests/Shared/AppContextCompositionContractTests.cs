using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class AppContextCompositionContractTests
{
    [Fact]
    public void Composition_contract_has_one_owner_per_contribution_and_no_partial_success()
    {
        var contract = ReadContract();

        RepositoryFiles.ContainsAll(
            contract,
            "SPEC-006 owns the response schemas",
            "SPEC-007 owns identity, session, and authorized-role contribution",
            "SPEC-008 owns authoritative time, term, window, and service contribution",
            "RegistrationWindowSummaryDto",
            "SPEC-008 owns both context handlers",
            "503 `CONTEXT_UNAVAILABLE`",
            "no partial success",
            "browser never composes");
    }

    [Fact]
    public void Authoritative_null_is_distinct_from_a_missing_contributor()
    {
        var contract = ReadContract();

        RepositoryFiles.ContainsAll(
            contract,
            "authoritative null",
            "registrationWindowState = none",
            "missing or failed contributor",
            "not use null to hide a contributor failure");

        var valid = CreateContext(
            registrationTerm: null,
            registrationWindowState: RegistrationWindowState.None);
        Assert.Null(valid.RegistrationTerm);
        Assert.Throws<ArgumentException>(() => CreateContext(
            registrationTerm: null,
            registrationWindowState: RegistrationWindowState.Open));
    }

    [Fact]
    public void Active_role_requires_exactly_one_server_authorized_role()
    {
        var contract = ReadContract();
        RepositoryFiles.ContainsAll(
            contract,
            "server-authorized role set",
            "client-supplied role never grants authorization");

        Assert.Throws<ArgumentException>(() => CreateContext(
            authorizedRoles: ["Lecturer", "TeachingAssistant"],
            activeRole: "Lecturer"));
        Assert.Throws<ArgumentException>(() => CreateContext(
            authorizedRoles: ["Student"],
            activeRole: "Admin"));
    }

    private static AppContextDto CreateContext(
        TermSummaryDto? registrationTerm = null,
        RegistrationWindowState registrationWindowState = RegistrationWindowState.None,
        IReadOnlyList<string>? authorizedRoles = null,
        string? activeRole = "Student",
        SessionState sessionState = SessionState.Active) =>
        new(
            new DateTime(2026, 7, 13, 8, 30, 0, DateTimeKind.Utc),
            "Africa/Cairo",
            teachingTerm: null,
            registrationTerm,
            registrationWindowState,
            registrationWindowState is RegistrationWindowState.None
                ? null
                : new RegistrationWindowSummaryDto(
                    "window-summer-all",
                    registrationWindowState,
                    new DateTime(2026, 7, 13, 8, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc),
                    "CQoLDA=="),
            ServiceState.Available,
            "Ahmed Student",
            authorizedRoles ?? ["Student"],
            activeRole,
            sessionState,
            new DateTime(2026, 7, 13, 10, 30, 0, DateTimeKind.Utc),
            "/support/reference");

    private static string ReadContract() =>
        string.Join(
            " ",
            RepositoryFiles.Read(
                    "specs/006-domain-class-api-contracts/contracts/app-context-composition.md")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
