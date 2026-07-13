using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec005;

public sealed class CodeFirstOwnershipMapTests
{
    private const string ContractPath = "docs/data/code-first-ownership-map.md";
    private const string ManifestPath = ".specify/persistence-manifest.json";

    [Fact]
    public void Fr1_contract_declares_ownership_and_fail_closed_nonproduction_profiles()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(ContractPath);
        Spec005ContractTestSupport.AssertWorkstream(
            "Code First model",
            ContractPath,
            "tests/StudentRegistration.SpecificationTests/Spec005/CodeFirstOwnershipMapTests.cs",
            "FR-1");

        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        var dbContext = manifest.RootElement.GetProperty("dbContext");
        Assert.Equal("004", dbContext.GetProperty("owner").GetString());
        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "code-first-ownership-map/1.0",
            "FR-1",
            "Infrastructure.SqlServer owns the sole DbContext and migrations",
            "Canonical feature owners own mappings and seed rows",
            $"Sole DbContext owner: SPEC-{dbContext.GetProperty("owner").GetString()}",
            dbContext.GetProperty("path").GetString()!,
            "SQL Server 2022 Developer",
            "compatibility level 160",
            "Docker Development",
            "Testcontainers Testing",
            "Schema: auth", "Schema: academics", "Schema: scheduling",
            "Schema: registration", "Schema: audit",
            "Migrations run before seed contributors",
            "Seed rows in migrations: prohibited",
            "Stable fixture identifiers and ordinals",
            "Reseeding the same profile version is idempotent",
            "Plaintext credentials are transient",
            "SQL contains ASP.NET Core Identity hashes only",
            "Production bootstrap/reset path: prohibited",
            "This demo contract does not approve a production SQL Server edition or topology",
            "Unknown environment or connection targets reject bootstrap/reset without mutation");

        foreach (var contribution in manifest.RootElement.GetProperty("contributions").EnumerateObject())
        {
            Spec005ContractTestSupport.AssertContainsNormalized(
                contract,
                $"SPEC-{contribution.Name}",
                contribution.Value.GetProperty("mode").GetString()!,
                contribution.Value.GetProperty("path").GetString()!);
        }

        foreach (var profile in manifest.RootElement.GetProperty("nonProductionDataProfiles").EnumerateArray())
        {
            Spec005ContractTestSupport.AssertContainsNormalized(
                contract,
                profile.GetProperty("id").GetString()!,
                profile.GetProperty("databasePattern").GetString()!,
                profile.GetProperty("orchestratorPath").GetString()!,
                profile.GetProperty("identityContributorPath").GetString()!,
                profile.GetProperty("academicContributorPath").GetString()!,
                profile.GetProperty("resetMode").GetString()!);

            foreach (var environment in profile.GetProperty("allowedEnvironments").EnumerateArray())
            {
                Spec005ContractTestSupport.AssertContainsNormalized(
                    contract,
                    $"Allowed environment: {environment.GetString()}");
            }
        }

        foreach (var migration in manifest.RootElement.GetProperty("migrations").EnumerateArray())
        {
            Spec005ContractTestSupport.AssertContainsNormalized(
                contract,
                "Planned migration contribution",
                migration.GetProperty("id").GetString()!,
                migration.GetProperty("path").GetString()!,
                $"SPEC-{migration.GetProperty("owner").GetString()}");
        }
    }
}
