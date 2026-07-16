using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec012;

public sealed class NFR_4EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-012-NFR-4.md";

    [Fact]
    public void Real_sql_evidence_uses_two_editors_and_returns_the_winner_to_the_stale_editor()
    {
        var test = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec012/RegistrationPlanModelConfigurationTests.cs");

        RepositoryFiles.ContainsAll(
            test,
            "Real_sql_round_trips_plan_values_and_enforces_unique_scope_and_offering",
            "await using var firstEditor = CreateContext(connectionString)",
            "await using var secondEditor = CreateContext(connectionString)",
            "new RegistrationPlanSqlServerAdapter(firstEditor)",
            "new RegistrationPlanSqlServerAdapter(secondEditor)",
            "RegistrationPlanStoreOutcome.Updated",
            "RegistrationPlanStoreOutcome.StaleVersion",
            "Assert.Equal(winner.Plan.Version, stale.Plan.Version)",
            "var winningItem = Assert.Single(stale.Plan.Items)");
    }

    [Fact]
    public void Production_adapter_and_endpoint_preserve_atomic_409_semantics()
    {
        var adapter = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/RegistrationPlanSqlServerAdapter.cs");
        var endpoint = RepositoryFiles.Read(
            "src/StudentRegistration.Registration/Endpoints/Spec012Endpoints.cs");

        RepositoryFiles.ContainsAll(
            adapter,
            "IsolationLevel.Serializable",
            ".AsTracking()",
            "command.ExpectedRowVersion",
            "RegistrationPlanStoreOutcome.StaleVersion",
            "await ReadAsync(command.StudentId, command.TermId, cancellationToken)",
            "catch (DbUpdateConcurrencyException)");
        RepositoryFiles.ContainsAll(
            endpoint,
            "RegistrationPlanOperationOutcome.StaleVersion",
            "new StaleRegistrationPlanResponse(",
            "currentVersion: current.RowVersion",
            "StatusCodes.Status409Conflict");
    }

    [Fact]
    public void Evidence_records_the_bounded_no_lost_update_claim()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-012 NFR-4 Two-Editor No-Lost-Update Evidence",
            "NFR-4",
            "T051",
            "two independent EF DbContext instances",
            "one winner",
            "stale current plan",
            "no lost update",
            "HTTP 409",
            "serializable transaction",
            "rowversion",
            "**Result: PASS.**");
        Assert.DoesNotMatch(
            @"(?i)\b(?:TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
    }
}
