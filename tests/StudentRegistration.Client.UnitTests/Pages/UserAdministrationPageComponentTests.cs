using StudentRegistration.TestSupport;

namespace StudentRegistration.Client.UnitTests.Pages;

public sealed class UserAdministrationPageComponentTests
{
    [Fact]
    public void Adm_03_has_complete_states_focus_restoration_and_duplicate_command_guards()
    {
        var page = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor");
        var logic = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs");

        RepositoryFiles.ContainsAll(
            page,
            "AuthenticatedPage",
            "CurrentRouteId=\"ADM-03\"",
            "role=\"search\"",
            "aria-label=\"@(LocalizedUiText.Get(\"User results\"))\"",
            "aria-busy=",
            "AccessibleValidationSummary",
            "ConfirmationDialog",
            "FocusRestoreRequested=\"RestoreStatusFocusAsync\"",
            "disabled=\"@IsBusy\"");
        RepositoryFiles.ContainsAll(
            logic,
            "if (_isLoadingUsers || page < 1)",
            "if (_selectedUser is null || _isChangingStatus)",
            "if (_selectedUser is null || IsStudentIdentity || _isReplacingRoles)",
            "if (_isCreatingImport ||",
            "if (_importBatch is null || _isRefreshingImport)",
            "if (!CanPublishImport || _isPublishingImport",
            "ApplySuccess(",
            "ApplyFailure(",
            "RestoreStatusFocusAsync");
    }

    [Fact]
    public void Adm_03_validation_stale_final_admin_and_unauthorized_states_are_safe()
    {
        var logic = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Pages/UserAdministrationPage.razor.cs");

        RepositoryFiles.ContainsAll(
            logic,
            "Enter a reason for the status change.",
            "Enter a reason for the role change.",
            "FINAL_ADMIN_REQUIRED",
            "This change would remove the final enabled Admin. No change was made.",
            "STALE_VERSION",
            "Refresh and review the current user or import before submitting again.",
            "UNAUTHORIZED",
            "FORBIDDEN",
            "ClearSelection()");
        Assert.DoesNotContain("PasswordHash", logic, StringComparison.Ordinal);
        Assert.DoesNotContain("SecurityStamp", logic, StringComparison.Ordinal);
    }
}
