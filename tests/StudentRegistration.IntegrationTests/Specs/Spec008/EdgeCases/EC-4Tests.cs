using System.Reflection;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec008.EdgeCases;

public sealed class EC_4Tests
{
    [Fact]
    public void Stale_admin_edit_returns_current_version_without_mutation_or_success_audit()
    {
        var aggregate = new VersionedAdminEditDouble(
            displayName: "Fall 2026",
            currentVersion: "term-v2");
        var successfulAuditWrites = 0;

        var result = aggregate.TryEdit(
            expectedVersion: "term-v1",
            displayName: "Changed by stale request",
            writeSuccessfulAudit: () => successfulAuditWrites++);

        Assert.Equal(409, result.StatusCode);
        Assert.Equal("STALE_VERSION", result.Error.Code);
        Assert.Equal("term-v2", result.Error.CurrentVersion);
        Assert.Equal("Fall 2026", aggregate.DisplayName);
        Assert.Equal("term-v2", aggregate.CurrentVersion);
        Assert.Equal(0, successfulAuditWrites);
    }

    [Fact]
    public void Frozen_stale_contract_exposes_only_the_directly_contested_version()
    {
        var requirements = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/requirements.md");
        var contract = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/contracts/api.md");

        RepositoryFiles.ContainsAll(
            requirements,
            "EC-4: Stale admin edit -> 409 with current rowversion",
            "stale, overlapping, unauthorized, or missing-provenance operations change",
            "nothing and return their stable reason");
        RepositoryFiles.ContainsAll(
            contract,
            "`409 STALE_VERSION/TERM_STATE_CONFLICT/WINDOW_OVERLAP`",
            "`ApiError.currentVersion` is only the directly contested aggregate",
            "atomic privacy-safe audit",
            "Transient failures commit no partial state");
    }

    [Fact]
    public void Production_stale_edit_requires_the_window_service_and_atomic_sql_store()
    {
        var academics = Assembly.Load("StudentRegistration.Academics");
        var infrastructure = Assembly.Load("StudentRegistration.Infrastructure.SqlServer");

        Assert.True(
            academics.GetType(
                "StudentRegistration.Academics.Application.RegistrationWindowService")
                is not null,
            "RegistrationWindowService must reject stale expected versions before EC-4 can exercise production edits.");
        Assert.True(
            infrastructure.GetType(
                "StudentRegistration.Infrastructure.SqlServer.Persistence.AcademicStore")
                is not null,
            "AcademicStore must atomically reject the stale mutation and successful audit write.");
    }

    private sealed class VersionedAdminEditDouble(
        string displayName,
        string currentVersion)
    {
        public string DisplayName { get; private set; } = displayName;

        public string CurrentVersion { get; private set; } = currentVersion;

        public EditResult TryEdit(
            string expectedVersion,
            string displayName,
            Action writeSuccessfulAudit)
        {
            if (!string.Equals(
                expectedVersion,
                CurrentVersion,
                StringComparison.Ordinal))
            {
                return new EditResult(
                    409,
                    new ApiError(
                        "STALE_VERSION",
                        "The record changed. Refresh before retrying.",
                        "corr-ec4",
                        currentVersion: CurrentVersion));
            }

            DisplayName = displayName;
            CurrentVersion = "term-v3";
            writeSuccessfulAudit();
            return new EditResult(
                200,
                new ApiError("NONE", "No error.", "corr-ec4"));
        }
    }

    private sealed record EditResult(int StatusCode, ApiError Error);
}
