using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec007;

public sealed class IdentityAccessModelConfigurationTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec007ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Identity_mapping_contributes_the_auth_schema_and_relational_invariants()
    {
        using var context = CreateContext(ModelConnectionString);

        var applicationUser = RequiredEntity<ApplicationUser>(context);
        var staff = RequiredEntity<Staff>(context);
        var activation = RequiredEntity<StudentActivation>(context);
        var recovery = RequiredEntity<AccountRecoveryChallenge>(context);
        var role = RequiredEntity<RoleAssignment>(context);
        var abuse = RequiredEntity<AuthenticationAbuseState>(context);
        var importBatch = RequiredEntity<IdentityImportBatch>(context);
        var importCandidate = RequiredEntity<IdentityImportCandidateRow>(context);
        var securityEvent = RequiredEntity<SecurityEvent>(context);
        var adminGuard = RequiredEntity<AdminSecurityGuard>(context);

        Assert.All(
            new[]
            {
                applicationUser,
                staff,
                activation,
                recovery,
                role,
                abuse,
                importBatch,
                importCandidate,
                securityEvent,
                adminGuard
            },
            entity => Assert.Equal("auth", entity.GetSchema()));

        AssertUniqueIndex(applicationUser, nameof(ApplicationUser.NormalizedUserName));
        AssertUniqueIndex(applicationUser, nameof(ApplicationUser.UniversityId));
        Assert.Contains(
            "IS NOT NULL",
            FindIndex(applicationUser, nameof(ApplicationUser.UniversityId)).GetFilter(),
            StringComparison.OrdinalIgnoreCase);
        AssertUniqueIndex(staff, nameof(Staff.ApplicationUserId));
        AssertUniqueIndex(staff, nameof(Staff.StaffNumber));
        AssertUniqueIndex(activation, nameof(StudentActivation.ApplicationUserId));
        AssertUniqueIndex(recovery, nameof(AccountRecoveryChallenge.TokenHash));
        AssertUniqueIndex(
            role,
            nameof(RoleAssignment.ApplicationUserId),
            nameof(RoleAssignment.RoleCode),
            nameof(RoleAssignment.EffectiveFromUtc));
        AssertUniqueIndex(abuse, nameof(AuthenticationAbuseState.SubjectKeyHash));
        AssertUniqueIndex(importBatch, nameof(IdentityImportBatch.SourceHash));
        AssertUniqueIndex(
            importBatch,
            nameof(IdentityImportBatch.RequestedByUserId),
            nameof(IdentityImportBatch.ClientRequestId));
        AssertUniqueIndex(
            importCandidate,
            nameof(IdentityImportCandidateRow.IdentityImportBatchId),
            nameof(IdentityImportCandidateRow.Ordinal));

        AssertRowVersion<ApplicationUser>(applicationUser, nameof(ApplicationUser.Version));
        AssertRowVersion<StudentActivation>(activation, nameof(StudentActivation.Version));
        AssertRowVersion<AccountRecoveryChallenge>(recovery, nameof(AccountRecoveryChallenge.Version));
        AssertRowVersion<RoleAssignment>(role, nameof(RoleAssignment.Version));
        AssertRowVersion<AuthenticationAbuseState>(abuse, nameof(AuthenticationAbuseState.Version));
        AssertRowVersion<IdentityImportBatch>(importBatch, nameof(IdentityImportBatch.Version));
        AssertRowVersion<AdminSecurityGuard>(adminGuard, nameof(AdminSecurityGuard.Version));

        AssertForeignKey<Staff, ApplicationUser>(staff, nameof(Staff.ApplicationUserId));
        AssertForeignKey<StudentActivation, ApplicationUser>(
            activation,
            nameof(StudentActivation.ApplicationUserId));
        AssertForeignKey<AccountRecoveryChallenge, ApplicationUser>(
            recovery,
            nameof(AccountRecoveryChallenge.ApplicationUserId));
        AssertForeignKey<RoleAssignment, ApplicationUser>(
            role,
            nameof(RoleAssignment.ApplicationUserId));
        AssertForeignKey<IdentityImportBatch, ApplicationUser>(
            importBatch,
            nameof(IdentityImportBatch.RequestedByUserId));
        AssertForeignKey<SecurityEvent, ApplicationUser>(
            securityEvent,
            nameof(SecurityEvent.ApplicationUserId));
        var candidateBatchForeignKey = importCandidate.GetForeignKeys().Single(
            candidate => candidate.PrincipalEntityType.ClrType == typeof(IdentityImportBatch));
        Assert.Equal(DeleteBehavior.Cascade, candidateBatchForeignKey.DeleteBehavior);

        var designTimeAdminGuard = context.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(AdminSecurityGuard));
        Assert.NotNull(designTimeAdminGuard);
        Assert.Equal(
            "[Id] = 1",
            designTimeAdminGuard.GetCheckConstraints().Single().Sql);
    }

    [Fact]
    public void Credential_and_recovery_storage_exposes_hashes_only()
    {
        using var context = CreateContext(ModelConnectionString);

        var userProperties = RequiredEntity<ApplicationUser>(context)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.Contains(nameof(ApplicationUser.PasswordHash), userProperties);
        Assert.DoesNotContain(
            userProperties,
            property => string.Equals(property, "Password", StringComparison.OrdinalIgnoreCase)
                || property.Contains("Plain", StringComparison.OrdinalIgnoreCase)
                || property.Contains("Pin", StringComparison.OrdinalIgnoreCase));

        var activationProperties = RequiredEntity<StudentActivation>(context)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.DoesNotContain(
            activationProperties,
            property => property.Contains("Password", StringComparison.OrdinalIgnoreCase)
                || property.Contains("Pin", StringComparison.OrdinalIgnoreCase));

        var recoveryProperties = RequiredEntity<AccountRecoveryChallenge>(context)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        Assert.Contains(nameof(AccountRecoveryChallenge.TokenHash), recoveryProperties);
        Assert.Contains(nameof(AccountRecoveryChallenge.DeliveryReferenceHash), recoveryProperties);
        Assert.DoesNotContain("Token", recoveryProperties);
        Assert.DoesNotContain("DeliveryReference", recoveryProperties);
    }

    [Fact]
    public void Security_event_mapping_preserves_the_append_only_domain_surface()
    {
        using var context = CreateContext(ModelConnectionString);
        var securityEvent = RequiredEntity<SecurityEvent>(context);

        Assert.Equal(ValueGenerated.Never, securityEvent.FindProperty(nameof(SecurityEvent.Id))!.ValueGenerated);
        Assert.All(
            typeof(SecurityEvent).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.DoesNotContain(
            securityEvent.GetProperties(),
            property => property.IsConcurrencyToken);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_materializes_the_identity_mapping_without_claiming_a_migration()
    {
        await using var fixture = new SqlServerContainerFixture();
        await fixture.StartAsync();

        var databaseName = $"StudentRegistration_Test_{Guid.NewGuid():N}";
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = databaseName
        }.ConnectionString;

        await using var context = CreateContext(connectionString);
        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM sys.tables AS t
                INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                WHERE s.name = N'auth'
                  AND t.name IN
                  (
                      N'ApplicationUsers',
                      N'Staff',
                      N'StudentActivations',
                      N'AccountRecoveryChallenges',
                      N'RoleAssignments',
                      N'AuthenticationAbuseStates',
                      N'IdentityImportBatches',
                      N'IdentityImportCandidateRows',
                      N'SecurityEvents',
                      N'AdminSecurityGuards'
                  );
                """;

            Assert.Equal(10, Convert.ToInt32(await command.ExecuteScalarAsync()));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static IEntityType RequiredEntity<TEntity>(StudentRegistrationDbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new Xunit.Sdk.XunitException(
            $"The relational model does not contain {typeof(TEntity).Name}.");

    private static void AssertUniqueIndex(IEntityType entity, params string[] propertyNames) =>
        Assert.True(
            FindIndex(entity, propertyNames).IsUnique,
            $"{entity.ClrType.Name}({string.Join(", ", propertyNames)}) must be unique.");

    private static IIndex FindIndex(IEntityType entity, params string[] propertyNames) =>
        entity.GetIndexes().SingleOrDefault(
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(propertyNames, StringComparer.Ordinal))
        ?? throw new Xunit.Sdk.XunitException(
            $"Missing {entity.ClrType.Name} index ({string.Join(", ", propertyNames)}).");

    private static void AssertRowVersion<TEntity>(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName)
            ?? throw new Xunit.Sdk.XunitException(
                $"Missing {typeof(TEntity).Name}.{propertyName}.");
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        Assert.Equal("rowversion", property.GetColumnType());
    }

    private static void AssertForeignKey<TDependent, TPrincipal>(
        IEntityType dependent,
        string propertyName)
    {
        var foreignKey = dependent.GetForeignKeys().SingleOrDefault(
            candidate => candidate.PrincipalEntityType.ClrType == typeof(TPrincipal)
                && candidate.Properties.Count == 1
                && candidate.Properties[0].Name == propertyName);
        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }
}
