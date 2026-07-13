using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec004;

public sealed class NFR_3EvidenceTests
{
    private const string EvidencePath = "docs/release-evidence/SPEC-004-NFR-3.md";

    [Fact]
    public void Two_replica_manifest_uses_shared_sql_and_shared_encrypted_keys_without_affinity()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-004/NFR-3",
            "at least two application replicas",
            "stateless",
            "shared SQL",
            "shared Data Protection",
            "no sticky sessions",
            "ExternalCertificate");

        var replicas = ParseReplicaRows(evidence);
        Assert.Empty(ValidateTopology(replicas));

        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs");
        RepositoryFiles.ContainsAll(
            registration,
            "SetApplicationName(applicationName)",
            "PersistKeysToSqlServer()",
            "ProtectKeysWithCertificate(certificate)",
            "SqlServer",
            "ExternalCertificate");

        var keyRepository = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/DataProtection/SqlDataProtectionKeyRepository.cs");
        RepositoryFiles.ContainsAll(
            keyRepository,
            "PersistKeysToDbContext<StudentRegistrationDbContext>");

        var apiSource = ReadApiSource();
        string[] forbiddenAffinityOrInstanceStateTokens =
        [
            "AddSession(",
            "UseSession(",
            "HttpContext.Session",
            "AddMemoryCache(",
            "IMemoryCache",
            "ARRAffinity",
            "SessionAffinity",
            "StickySession"
        ];

        Assert.All(
            forbiddenAffinityOrInstanceStateTokens,
            token => Assert.DoesNotContain(token, apiSource, StringComparison.Ordinal));
    }

    [Fact]
    public void Topology_validator_rejects_one_replica_split_shared_state_and_affinity()
    {
        var oneReplica =
            new[]
            {
                new ReplicaEvidence(
                    "api-01",
                    "StudentRegistration",
                    "AASTMT.StudentRegistration",
                    "StudentRegistrationDbContext",
                    "ExternalCertificate",
                    "None")
            };
        Assert.Contains(
            ValidateTopology(oneReplica),
            violation => violation.Contains(
                "at least two distinct replicas",
                StringComparison.Ordinal));

        ReplicaEvidence[] splitStateAndAffinity =
        [
            oneReplica[0],
            new(
                "api-02",
                "ReplicaLocalDatabase",
                "Different.Application",
                "LocalFileSystem",
                "Unprotected",
                "Required")
        ];
        var violations = ValidateTopology(splitStateAndAffinity);

        RepositoryFiles.ContainsAll(
            string.Join(Environment.NewLine, violations),
            "one shared SQL database",
            "one Data Protection application name",
            "one shared key repository",
            "external certificate protection",
            "request affinity must be None");
    }

    private static IReadOnlyList<ReplicaEvidence> ParseReplicaRows(string markdown) =>
        markdown
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("| `api-", StringComparison.Ordinal))
            .Select(line => line.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries))
            .Select(cells =>
            {
                Assert.Equal(6, cells.Length);
                return new ReplicaEvidence(
                    TrimCode(cells[0]),
                    TrimCode(cells[1]),
                    TrimCode(cells[2]),
                    TrimCode(cells[3]),
                    TrimCode(cells[4]),
                    TrimCode(cells[5]));
            })
            .ToArray();

    private static IReadOnlyList<string> ValidateTopology(
        IReadOnlyCollection<ReplicaEvidence> replicas)
    {
        var violations = new List<string>();
        if (replicas.Select(replica => replica.Name).Distinct(StringComparer.Ordinal).Count() < 2)
        {
            violations.Add("Topology must declare at least two distinct replicas.");
        }

        AddSharedValueViolation(
            replicas,
            replica => replica.SqlDatabase,
            "All replicas must use one shared SQL database.",
            violations);
        AddSharedValueViolation(
            replicas,
            replica => replica.DataProtectionApplicationName,
            "All replicas must use one Data Protection application name.",
            violations);
        AddSharedValueViolation(
            replicas,
            replica => replica.KeyRepository,
            "All replicas must use one shared key repository.",
            violations);

        if (replicas.Any(replica =>
                !string.Equals(
                    replica.KeyEncryption,
                    "ExternalCertificate",
                    StringComparison.Ordinal)))
        {
            violations.Add("Every replica must use external certificate protection.");
        }

        if (replicas.Any(replica =>
                !string.Equals(replica.RequestAffinity, "None", StringComparison.Ordinal)))
        {
            violations.Add("Replica request affinity must be None.");
        }

        return violations;
    }

    private static void AddSharedValueViolation(
        IReadOnlyCollection<ReplicaEvidence> replicas,
        Func<ReplicaEvidence, string> selector,
        string message,
        ICollection<string> violations)
    {
        if (replicas.Select(selector).Distinct(StringComparer.Ordinal).Count() != 1)
        {
            violations.Add(message);
        }
    }

    private static string ReadApiSource() =>
        string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(
                    RepositoryFiles.PathTo("src/StudentRegistration.Api"),
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(path => !path.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));

    private static string TrimCode(string value) => value.Trim().Trim('`');

    private sealed record ReplicaEvidence(
        string Name,
        string SqlDatabase,
        string DataProtectionApplicationName,
        string KeyRepository,
        string KeyEncryption,
        string RequestAffinity);
}
