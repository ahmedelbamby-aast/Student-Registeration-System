using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using StudentRegistration.LoadTests.Infrastructure;
using StudentRegistration.LoadTests.Specs.Spec008;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec008;

public sealed class NFR_2EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-008-NFR-2.md";
    private const string LoadTestPath =
        "tests/StudentRegistration.LoadTests/Specs/Spec008/AcademicContextLoadTests.cs";
    private const string FixturePath =
        "tests/StudentRegistration.LoadTests/Infrastructure/Spec008TwoReplicaSharedSqlFixture.cs";
    private const string MigrationId =
        "20260713010000_IdentityAcademicFoundation";
    private const long ExpectedStudentCount = 25_000;
    private const long ExpectedDurationSeconds = 600;
    private const long ExpectedReadsPerSecond = 300;
    private const long ExpectedAttemptedReads = 180_000;
    private const long ExpectedReplicaCount = 2;
    private const long ExpectedReadsPerReplica = 90_000;
    private const decimal MaximumP95Milliseconds = 300m;
    private const long MaximumUnexpectedFailures = 179;
    private const decimal MaximumUnexpectedFailurePercent = 0.1m;

    [Fact]
    public void Evidence_records_the_exact_passing_authenticated_two_replica_run()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RejectPendingOrFabricatedEvidence(evidence);

        Assert.Equal(
            ExpectedStudentCount,
            IntegerMetric(evidence, "Synthetic students"));
        Assert.Equal(
            ExpectedDurationSeconds,
            IntegerMetric(evidence, "Duration seconds"));
        Assert.Equal(
            ExpectedReadsPerSecond,
            IntegerMetric(evidence, "Scheduled reads per second"));
        Assert.Equal(
            ExpectedAttemptedReads,
            IntegerMetric(evidence, "Total attempted reads"));
        Assert.Equal(
            ExpectedReplicaCount,
            IntegerMetric(evidence, "Replica count"));
        Assert.Equal(
            ExpectedReadsPerReplica,
            IntegerMetric(evidence, "Replica 1 attempted reads"));
        Assert.Equal(
            ExpectedReadsPerReplica,
            IntegerMetric(evidence, "Replica 2 attempted reads"));

        var p95Milliseconds = DecimalMetric(
            evidence,
            "Actual p95 latency milliseconds");
        Assert.InRange(p95Milliseconds, 0m, MaximumP95Milliseconds);

        var unexpectedFailures = IntegerMetric(
            evidence,
            "Unexpected failure count");
        Assert.InRange(unexpectedFailures, 0, MaximumUnexpectedFailures);
        var unexpectedFailurePercent = PercentMetric(
            evidence,
            "Unexpected failure rate");
        Assert.True(
            unexpectedFailurePercent >= 0m &&
            unexpectedFailurePercent < MaximumUnexpectedFailurePercent,
            $"Unexpected failure rate {unexpectedFailurePercent}% must be below 0.1%.");

        var computedFailurePercent =
            unexpectedFailures * 100m / ExpectedAttemptedReads;
        Assert.InRange(
            Math.Abs(unexpectedFailurePercent - computedFailurePercent),
            0m,
            0.000001m);

        RepositoryFiles.ContainsAll(
            evidence,
            "# SPEC-008 NFR-2 Release Evidence",
            "**Owner:** Ahmed ELbamby",
            "**Result: PASS.**",
            AcademicContextLoadProfile.RequiredGate.ProfileId,
            "GET /api/context",
            "continuous ten-minute run",
            "no retries",
            "ASP.NET Core cookie authentication",
            "IdentityCookieAuthenticationEvents",
            "security-stamp validation",
            "Context.Read",
            "AcademicSessionContextAdapter",
            "shared SQL data-protection key ring",
            "cross-replica ticket",
            "in-process TestServer",
            "does not measure external network or TLS latency",
            MigrationId,
            Spec008TwoReplicaSharedSqlFixture.SqlServerImage,
            Spec008TwoReplicaSharedSqlFixture.SeedProfileVersion,
            "dotnet test tests/StudentRegistration.LoadTests/StudentRegistration.LoadTests.csproj",
            "--filter",
            "AcademicContextLoadTests",
            "aggregate-only",
            "no University IDs",
            "no credentials",
            "no session cookies",
            "no security stamps",
            "no full student profiles",
            "does not replace SPEC-018",
            "mixed-load gate");

        Assert.Matches(
            @"(?im)^\*\*Recorded UTC:\*\*\s+\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z\s*$",
            evidence);
    }

    [Fact]
    public void Evidence_is_cryptographically_bound_to_the_real_load_and_fixture_sources()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        var loadSource = RepositoryFiles.Read(LoadTestPath);
        var fixtureSource = RepositoryFiles.Read(FixturePath);
        var requiredGate = AcademicContextLoadProfile.RequiredGate;

        requiredGate.Validate();
        Assert.Equal(
            "SPEC008-AUTHENTICATED-CONTEXT-1.0",
            requiredGate.ProfileId);
        Assert.Equal(ExpectedStudentCount, (long)requiredGate.IdentityCount);
        Assert.Equal(ExpectedReplicaCount, (long)requiredGate.ReplicaCount);
        Assert.Equal(
            ExpectedDurationSeconds,
            (long)requiredGate.DurationSeconds);
        Assert.Equal(
            ExpectedReadsPerSecond,
            (long)requiredGate.RequestsPerSecond);
        Assert.Equal(
            ExpectedAttemptedReads,
            (long)requiredGate.TotalAttempts);
        Assert.Equal(
            ExpectedReadsPerReplica,
            (long)requiredGate.AttemptsPerReplica);
        Assert.Equal(
            (double)MaximumP95Milliseconds,
            requiredGate.MaximumP95Milliseconds);
        Assert.Equal(
            MaximumUnexpectedFailures,
            (long)requiredGate.MaximumFailures);
        Assert.Equal(
            (double)(MaximumUnexpectedFailurePercent / 100m),
            requiredGate.MaximumFailureRate);

        RepositoryFiles.ContainsAll(
            loadSource,
            "AcademicContextLoadTests",
            "Spec008TwoReplicaSharedSqlFixture",
            "StudentSessions",
            "/api/context");
        RepositoryFiles.ContainsAll(
            fixtureSource,
            "UseTestServer",
            "AddStudentRegistrationIdentitySecurity",
            "TicketDataFormat.Protect",
            "VerifyCrossReplicaTicketAsync",
            "MapSpec008Endpoints");
        Assert.DoesNotContain(
            "TestAuthenticationHandler",
            loadSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "HeaderAuthenticationHandler",
            loadSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "AddScheme<",
            loadSource,
            StringComparison.Ordinal);

        RepositoryFiles.ContainsAll(
            evidence,
            $"Load test normalized-LF SHA-256: `{NormalizedSourceHash(loadSource)}`",
            $"Fixture normalized-LF SHA-256: `{NormalizedSourceHash(fixtureSource)}`");

        Assert.Equal(
            ExpectedStudentCount,
            Spec008TwoReplicaSharedSqlFixture.DefaultStudentCount);
        Assert.Equal(
            "synthetic-fixture/1.0",
            Spec008TwoReplicaSharedSqlFixture.SeedProfileVersion);
        Assert.StartsWith(
            "mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04@sha256:",
            Spec008TwoReplicaSharedSqlFixture.SqlServerImage,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Evidence_contains_only_aggregate_synthetic_results_and_no_session_material()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);

        Assert.DoesNotMatch(@"\bAI26\d{5}\b", evidence);
        Assert.DoesNotMatch(
            @"(?im)^\s*(?:Cookie|Set-Cookie)\s*:",
            evidence);
        Assert.DoesNotMatch(
            @"__Host-StudentRegistration\.Session\s*=",
            evidence);
        Assert.DoesNotMatch(
            @"(?im)\b(?:password|security[_ -]?stamp|protected[_ -]?ticket|session[_ -]?cookie)\s*[:=]\s*\S+",
            evidence);
        Assert.DoesNotMatch(
            @"(?im)\b(?:ApplicationUserId|UniversityId)\s*[:=]\s*[A-Za-z0-9-]+",
            evidence);
    }

    private static void RejectPendingOrFabricatedEvidence(string evidence)
    {
        Assert.DoesNotMatch(
            @"(?i)\b(?:PENDING|TODO|TBD|FIXME|PLACEHOLDER|NOT\s+RUN|NOT\s+EXECUTED)\b",
            evidence);
        Assert.DoesNotMatch(@"\{\{[^}]+\}\}", evidence);
        Assert.DoesNotMatch(@"(?i)<\s*insert\b[^>]*>", evidence);
        Assert.DoesNotContain(
            "**Result: FAIL.**",
            evidence,
            StringComparison.OrdinalIgnoreCase);
    }

    private static long IntegerMetric(string evidence, string label)
    {
        var value = MetricText(evidence, label, expectPercent: false);
        Assert.DoesNotContain('.', value);
        Assert.True(
            long.TryParse(
                value.Replace(",", string.Empty, StringComparison.Ordinal),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var parsed),
            $"Metric '{label}' is not an invariant integer: {value}");
        return parsed;
    }

    private static decimal DecimalMetric(string evidence, string label)
    {
        var value = MetricText(evidence, label, expectPercent: false);
        Assert.True(
            decimal.TryParse(
                value.Replace(",", string.Empty, StringComparison.Ordinal),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var parsed),
            $"Metric '{label}' is not an invariant decimal: {value}");
        return parsed;
    }

    private static decimal PercentMetric(string evidence, string label)
    {
        var value = MetricText(evidence, label, expectPercent: true);
        Assert.True(
            decimal.TryParse(
                value.Replace(",", string.Empty, StringComparison.Ordinal),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var parsed),
            $"Metric '{label}' is not an invariant percentage: {value}%");
        return parsed;
    }

    private static string MetricText(
        string evidence,
        string label,
        bool expectPercent)
    {
        var percent = expectPercent ? @"\s*%" : string.Empty;
        var matches = Regex.Matches(
                evidence,
                $@"(?im)^\|\s*{Regex.Escape(label)}\s*\|\s*(?<value>\d[\d,]*(?:\.\d+)?)" +
                percent +
                @"\s*\|\s*$")
            .Cast<Match>()
            .ToArray();
        var match = Assert.Single(matches);
        return match.Groups["value"].Value;
    }

    private static string NormalizedSourceHash(string source)
    {
        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }
}
