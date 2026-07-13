using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class DataProtectionArchitectureTests
{
    private const string RegistrationPath =
        "src/StudentRegistration.Api/Composition/DataProtectionRegistration.cs";
    private const string RepositoryPath =
        "src/StudentRegistration.Infrastructure.SqlServer/DataProtection/SqlDataProtectionKeyRepository.cs";
    private const string ConfigurationPath =
        "src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/DataProtectionKeyModelConfiguration.cs";

    [Fact]
    public void Shared_key_ring_is_sql_backed_certificate_protected_and_single_app_name()
    {
        var registration = RepositoryFiles.Read(RegistrationPath);
        RepositoryFiles.ContainsAll(
            registration,
            "SetApplicationName",
            "PersistKeysToSqlServer",
            "ProtectKeysWithCertificate",
            "ExternalCertificate",
            "X509CertificateLoader",
            "PRODUCTION_DATA_PROTECTION_AUTHORITY_REQUIRED");
        Assert.DoesNotContain("PersistKeysToFileSystem", registration, StringComparison.Ordinal);

        var repository = RepositoryFiles.Read(RepositoryPath);
        RepositoryFiles.ContainsAll(
            repository,
            "IDataProtectionBuilder",
            "PersistKeysToDbContext<StudentRegistrationDbContext>");

        var configuration = RepositoryFiles.Read(ConfigurationPath);
        RepositoryFiles.ContainsAll(
            configuration,
            "IEntityTypeConfiguration<DataProtectionKey>",
            "ToTable(\"DataProtectionKeys\")",
            "HasKey",
            "IsRequired");

        var context = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Persistence/StudentRegistrationDbContext.cs");
        RepositoryFiles.ContainsAll(
            context,
            "IDataProtectionKeyContext",
            "DbSet<DataProtectionKey>");
    }

    [Fact]
    public void Replica_contract_requires_rotation_recovery_and_no_sticky_sessions()
    {
        var runbook = RepositoryFiles.Read("ops/runbooks/data-protection-keys.md");
        RepositoryFiles.ContainsAll(
            runbook,
            "two replicas",
            "without sticky sessions",
            "rotation",
            "recovery",
            "external certificate",
            "Production",
            "Security/DevOps approval");
    }
}
