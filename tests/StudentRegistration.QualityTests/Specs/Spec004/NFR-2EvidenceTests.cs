using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec004;

public sealed class NFR_2EvidenceTests
{
    private const string EvidencePath =
        "docs/release-evidence/SPEC-004-NFR-2.md";

    [Fact]
    public void Release_evidence_maps_every_NFR_2_control_to_enforced_proof()
    {
        var evidence = RepositoryFiles.Read(EvidencePath);
        RepositoryFiles.ContainsAll(
            evidence,
            "SPEC-004/NFR-2",
            "PASS (non-production demo)",
            "FAIL CLOSED",
            "two replicas",
            "cross-instance authentication",
            "plan state",
            "without sticky sessions",
            "DataProtectionKeys",
            "external certificate",
            "only the application identity",
            "rotation",
            "per-run Testing database",
            "guarded Development reset",
            "synthetic-only",
            "credentials",
            "logs",
            "exports",
            "seven days",
            "Security/DevOps approval");

        AssertCrossInstanceStateEvidence();
        AssertSharedProtectedKeyRingEvidence();
        AssertDatabaseLifecycleEvidence();
        AssertLocalArtifactLifecycleEvidence();
        AssertProductionAuthorityFailsClosed();
    }

    private static void AssertCrossInstanceStateEvidence()
    {
        var acceptance = RepositoryFiles.Read(
            "tests/StudentRegistration.AcceptanceTests/Specs/Spec004/AC-2Tests.cs");
        RepositoryFiles.ContainsAll(
            acceptance,
            "Consecutive_authenticated_requests_can_cross_replicas_without_losing_state",
            "authorization",
            "plan state",
            "without sticky sessions");

        var integration = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec004/DataProtectionRegistrationTests.cs");
        RepositoryFiles.ContainsAll(
            integration,
            "Two_replicas_share_protected_keys_and_fail_closed_without_production_authority",
            "Assert.DoesNotContain(\"sticky\"",
            "Assert.DoesNotContain(\"DistributedMemoryCache\"");
    }

    private static void AssertSharedProtectedKeyRingEvidence()
    {
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs");
        RepositoryFiles.ContainsAll(
            registration,
            "SetApplicationName(applicationName)",
            ".PersistKeysToSqlServer()",
            ".ProtectKeysWithCertificate(certificate)",
            "X509KeyStorageFlags.EphemeralKeySet");

        var context = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs");
        RepositoryFiles.ContainsAll(
            context,
            "IDataProtectionKeyContext",
            "DbSet<DataProtectionKey> DataProtectionKeys");

        var repository = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/DataProtection/SqlDataProtectionKeyRepository.cs");
        RepositoryFiles.ContainsAll(
            repository,
            "PersistKeysToDbContext<StudentRegistrationDbContext>");

        var mapping = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/DataProtectionKeyModelConfiguration.cs");
        RepositoryFiles.ContainsAll(
            mapping,
            "ToTable(\"DataProtectionKeys\")",
            "Property(key => key.Xml).IsRequired()");

        var runbook = RepositoryFiles.Read("ops/runbooks/data-protection-keys.md");
        RepositoryFiles.ContainsAll(
            runbook,
            "two replicas",
            "without sticky sessions",
            "external certificate",
            "## Rotation",
            "## Recovery");

        var requirement = RepositoryFiles.Read(
            "specs/004-architecture-engineering-principles/requirements.md");
        RepositoryFiles.ContainsAll(
            requirement,
            "only the application identity",
            "Security/DevOps institutional decision");
    }

    private static void AssertDatabaseLifecycleEvidence()
    {
        var testingFixture = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Infrastructure/SqlServerContainerFixture.cs");
        RepositoryFiles.ContainsAll(
            testingFixture,
            "RandomNumberGenerator.GetBytes",
            "CREATE DATABASE [StudentRegistration_Test_Spec004]",
            "DROP DATABASE [StudentRegistration_Test_Spec004]",
            "IAsyncDisposable",
            "_container.DisposeAsync()");

        var developmentCompose = RepositoryFiles.Read(
            "infra/docker/compose.development.yml");
        RepositoryFiles.ContainsAll(
            developmentCompose,
            "SRS_SQL_SA_PASSWORD",
            "development-sql-data:/var/opt/mssql",
            "volumes:",
            "development-sql-data:");

        var lifecycleContract = RepositoryFiles.Read(
            "specs/004-architecture-engineering-principles/research.md");
        RepositoryFiles.ContainsAll(
            lifecycleContract,
            "persistent-until-guarded-reset Development database",
            "disposes an isolated Testing database for every",
            "wholly synthetic");
    }

    private static void AssertLocalArtifactLifecycleEvidence()
    {
        var ignore = RepositoryFiles.Read(".gitignore");
        RepositoryFiles.ContainsAll(
            ignore,
            ".local/",
            "credentials/",
            "logs/",
            "exports/");

        var cleanupScript = RepositoryFiles.Read(
            "ops/scripts/Remove-ExpiredLocalArtifacts.ps1");
        RepositoryFiles.ContainsAll(
            cleanupScript,
            "SupportsShouldProcess",
            "RetentionDays = 7",
            ".AddDays(-$RetentionDays)",
            "Resolve-Path -LiteralPath",
            "Remove-Item -LiteralPath");

        var executableCleanupEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.QualityTests/Specs/Spec004/LocalArtifactRetentionTests.cs");
        RepositoryFiles.ContainsAll(
            executableCleanupEvidence,
            "expired.log",
            "current.log",
            "Assert.False(File.Exists(expired))",
            "Assert.True(File.Exists(current))");
    }

    private static void AssertProductionAuthorityFailsClosed()
    {
        var registration = RepositoryFiles.Read(
            "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs");
        RepositoryFiles.ContainsAll(
            registration,
            "ValidateProductionAuthority",
            "ProductionRepositoryApproved",
            "ProductionEncryptionApproved",
            "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED",
            "Security/DevOps approval");

        var executableEvidence = RepositoryFiles.Read(
            "tests/StudentRegistration.IntegrationTests/Specs/Spec004/DataProtectionRegistrationTests.cs");
        RepositoryFiles.ContainsAll(
            executableEvidence,
            "Production_without_repository_and_encryption_approval_fails_closed",
            "Assert.Throws<InvalidOperationException>",
            "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED");
    }
}
