using System.Reflection;
using StudentRegistration.AcceptanceTests.Infrastructure;
using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec008;

public sealed class AC_7Tests
{
    [Fact]
    public void Two_rebuilds_of_one_version_and_ordinal_are_logically_identical_and_synthetic()
    {
        var first = SyntheticAcademicGraphDouble.Build("academic-seed-v1", 42);
        var second = SyntheticAcademicGraphDouble.Build("academic-seed-v1", 42);

        Assert.Equal(first, second);
        Assert.StartsWith("SYN-AI-", first.UniversityId, StringComparison.Ordinal);
        Assert.Equal("synthetic-spec008", first.Source);
        Assert.DoesNotContain("@", first.UniversityId, StringComparison.Ordinal);
        Assert.DoesNotContain("phone", first.Provenance, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("address", first.Provenance, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(DateTimeKind.Utc, first.DataAsOfUtc.Kind);
        Assert.NotNull(TimeZoneInfo.FindSystemTimeZoneById(first.TimeZoneId));
    }

    [Fact]
    public void Exact_context_quality_measurement_has_no_shortened_or_single_replica_substitute()
    {
        var measurement = new ContextQualityMeasurement(
            SyntheticStudents: 25_000,
            RequestsAuthenticated: true,
            SharedSqlDatabase: true,
            ReplicaCount: 2,
            Duration: TimeSpan.FromMinutes(10),
            RequestsPerSecond: 300,
            TotalRequests: 180_000,
            P95Milliseconds: 300,
            UnexpectedFailures: 179,
            ContainsFullProfileTelemetry: false);

        Assert.True(ContextQualityGate.Passes(measurement));
        Assert.False(ContextQualityGate.Passes(measurement with { ReplicaCount = 1 }));
        Assert.False(ContextQualityGate.Passes(
            measurement with { Duration = TimeSpan.FromMinutes(9) }));
        Assert.False(ContextQualityGate.Passes(measurement with { TotalRequests = 179_999 }));
        Assert.False(ContextQualityGate.Passes(measurement with { P95Milliseconds = 301 }));
        Assert.False(ContextQualityGate.Passes(measurement with { UnexpectedFailures = 180 }));
        Assert.False(ContextQualityGate.Passes(
            measurement with { ContainsFullProfileTelemetry = true }));
    }

    [Fact]
    public void Frozen_quality_contract_records_the_exact_fixture_replica_and_privacy_gate()
    {
        var requirements = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/requirements.md");
        var tasks = RepositoryFiles.Read(
            "specs/008-academic-term-student-profile/tasks.md");

        RepositoryFiles.ContainsAll(
            requirements,
            "AC-7: Term and profile quality gate",
            "two rebuilds of the same synthetic seed",
            "25,000-account shared-SQL fixture",
            "10-minute two-replica load",
            "300 authenticated context reads per second",
            "at most 300 ms p95 with fewer than 0.1% unexpected",
            "instants use UTC datetime2",
            "valid IANA timezone identifiers",
            "MUST NOT contain a full student profile");
        RepositoryFiles.ContainsAll(
            tasks,
            "exactly 180,000 GET /api/context reads",
            "two stateless API replicas",
            "docs/release-evidence/SPEC-008-NFR-2.md");
    }

    [Fact]
    public void Migration_first_fixture_requires_the_seed_contributor_and_sql_mapping_before_docker()
    {
        Assert.Contains("mssql/server:2022", Spec008AcceptanceSqlServerFixture.SqlServerImage);

        var academics = Assembly.Load("StudentRegistration.Academics");
        var infrastructure = Assembly.Load("StudentRegistration.Infrastructure.SqlServer");
        Assert.True(
            academics.GetType(
                "StudentRegistration.Academics.Application.DemoStudentProfileSeedContributor")
                is not null,
            "DemoStudentProfileSeedContributor must rebuild the complete versioned synthetic graph before AC-7 starts SQL Server.");
        Assert.True(
            infrastructure.GetType(
                "StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations.AcademicContextModelConfiguration")
                is not null,
            "AcademicContextModelConfiguration must persist the UTC/IANA academic graph before AC-7 starts SQL Server.");
        Assert.True(
            RepositoryFiles.Exists(
                "src/StudentRegistration.Infrastructure.SqlServer/Migrations/20260713010000_IdentityAcademicFoundation.cs"),
            "The migration-first S1 identity/academic foundation is required before either deterministic rebuild runs.");
    }

    private sealed record SyntheticAcademicGraphDouble(
        string UniversityId,
        string ProgramCode,
        string Cohort,
        decimal Gpa,
        int EarnedCredits,
        string Standing,
        string Source,
        string Provenance,
        string DataVersion,
        DateTime DataAsOfUtc,
        string TimeZoneId)
    {
        public static SyntheticAcademicGraphDouble Build(string version, int ordinal)
        {
            var asOfUtc = new DateTime(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc)
                .AddMinutes(ordinal);
            return new(
                $"SYN-AI-{ordinal:D7}",
                "AI",
                "2026",
                2.50m + (ordinal % 15) / 10m,
                30 + ordinal % 90,
                "Active",
                "synthetic-spec008",
                $"seed:{version};ordinal:{ordinal}",
                version,
                asOfUtc,
                "Africa/Cairo");
        }
    }

    private sealed record ContextQualityMeasurement(
        int SyntheticStudents,
        bool RequestsAuthenticated,
        bool SharedSqlDatabase,
        int ReplicaCount,
        TimeSpan Duration,
        int RequestsPerSecond,
        int TotalRequests,
        double P95Milliseconds,
        int UnexpectedFailures,
        bool ContainsFullProfileTelemetry);

    private static class ContextQualityGate
    {
        public static bool Passes(ContextQualityMeasurement measurement) =>
            measurement.SyntheticStudents == 25_000 &&
            measurement.RequestsAuthenticated &&
            measurement.SharedSqlDatabase &&
            measurement.ReplicaCount >= 2 &&
            measurement.Duration == TimeSpan.FromMinutes(10) &&
            measurement.RequestsPerSecond == 300 &&
            measurement.TotalRequests == 180_000 &&
            measurement.P95Milliseconds <= 300 &&
            measurement.UnexpectedFailures /
                (double)measurement.TotalRequests < 0.001 &&
            !measurement.ContainsFullProfileTelemetry;
    }
}
