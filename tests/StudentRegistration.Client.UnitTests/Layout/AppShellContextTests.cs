using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Layout;

public sealed class AppShellContextTests
{
    private const string IdentityBoundaryPath =
        "specs/003-ux-storyboard-accessibility/design/pages/identity-boundary.md";
    private const string AuthenticatedShellPath =
        "specs/003-ux-storyboard-accessibility/design/components/authenticated-shell.md";

    [Fact]
    public void Identity_contract_keeps_student_and_staff_entries_distinct_without_a_role_picker()
    {
        var contract = RepositoryFiles.Read(IdentityBoundaryPath);

        RepositoryFiles.ContainsAll(
            contract,
            "/student/login",
            "/student/activate",
            "/account/recovery",
            "/staff/login",
            "University ID",
            "Admin, Lecturer, or Teaching Assistant",
            "no client role picker",
            "one server-authorized role",
            "INVALID_ROLE_CONFIGURATION",
            "no 2FA");
    }

    [Fact]
    public void Authenticated_shell_contract_maps_every_authoritative_context_field_and_failure_state()
    {
        var contract = RepositoryFiles.Read(AuthenticatedShellPath);

        RepositoryFiles.ContainsAll(
            contract,
            "Server date/time",
            "timezone",
            "Teaching term",
            "Registration term",
            "registration window",
            "Display name",
            "Authorized roles",
            "Active role",
            "Session state",
            "session expiry",
            "Service state",
            "supportReferencePath",
            "Exactly one role",
            "server-authoritative",
            "session-expired",
            "offline");
    }
}
