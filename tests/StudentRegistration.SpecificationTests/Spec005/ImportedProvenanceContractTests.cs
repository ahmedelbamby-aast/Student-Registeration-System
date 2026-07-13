using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.SpecificationTests.Spec005;

public sealed class ImportedProvenanceContractTests
{
    private const string ContractPath = "docs/data/import-provenance-contract.md";
    private const string ManifestPath = ".specify/persistence-manifest.json";

    [Fact]
    public void Fr8_contract_requires_import_and_synthetic_fixture_provenance()
    {
        var contract = Spec005ContractTestSupport.ReadBoundedContract(ContractPath);
        Spec005ContractTestSupport.AssertWorkstream(
            "Imported provenance",
            ContractPath,
            "tests/StudentRegistration.SpecificationTests/Spec005/ImportedProvenanceContractTests.cs",
            "FR-8");
        Spec005ContractTestSupport.AssertContainsNormalized(
            contract,
            "import-provenance-contract/1.0",
            "FR-8",
            "Import source/reference: required",
            "Import source hash: required",
            "Import batch: required",
            "Import actor evidence: required",
            "Import access/import time: required",
            "OfficialAASTMT", "AhmedApprovedDemo", "SyntheticDemo",
            "UnresolvedInstitutional",
            "Missing prerequisites reject preview and keep publication blocked",
            "SeedProfileVersion",
            "FixtureOrdinal",
            "The same seed-profile version and fixture ordinal reproduce the same logical identity and academic values",
            "Reseeding the same profile version is idempotent",
            "Password hash bytes need not be deterministic",
            "ASP.NET Core Identity verifies generated credentials",
            "Migrations run before seed contributors",
            "Reset is explicit and allowed only for Development or Testing",
            "Every other environment or connection target is rejected before mutation",
            "Real institutional or student data is prohibited in demo profiles",
            "IdentityImportBatch.RequestedByUserId",
            "AuditEvent.ActorReference correlated to catalogue ImportBatch",
            "ImportBatch.RequestedByUserId is not introduced",
            "Git-ignored local credential, log, and export artifacts are removed within seven days");

        using var manifest = JsonDocument.Parse(RepositoryFiles.Read(ManifestPath));
        foreach (var profile in manifest.RootElement.GetProperty("nonProductionDataProfiles").EnumerateArray())
        {
            Spec005ContractTestSupport.AssertContainsNormalized(
                contract,
                profile.GetProperty("id").GetString()!,
                profile.GetProperty("databasePattern").GetString()!,
                profile.GetProperty("resetMode").GetString()!,
                profile.GetProperty("identityContributorPath").GetString()!,
                profile.GetProperty("academicContributorPath").GetString()!);
        }
    }
}
