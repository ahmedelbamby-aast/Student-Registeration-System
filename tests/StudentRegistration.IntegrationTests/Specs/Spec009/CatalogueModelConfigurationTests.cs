using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.IntegrationTests.Infrastructure;

namespace StudentRegistration.IntegrationTests.Specs.Spec009;

public sealed class CatalogueModelConfigurationTests
{
    private const string ConnectionString =
        "Server=localhost;Database=Spec009ModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Mapping_contributes_the_nine_owned_catalogue_and_policy_entities()
    {
        using var context = CreateContext();
        var expected = new[]
        {
            typeof(CatalogueDraft),
            typeof(CatalogueVersion),
            typeof(Course),
            typeof(CoursePrerequisite),
            typeof(CurriculumCourse),
            typeof(ImportBatch),
            typeof(PolicyRule),
            typeof(PolicySet),
            typeof(StudentRegistration.Academics.Domain.Program),
        };

        Assert.Equal(
            expected.OrderBy(type => type.FullName, StringComparer.Ordinal),
            context.Model.GetEntityTypes()
                .Where(entity => entity.GetSchema() == "academics" && !entity.IsOwned())
                .Select(entity => entity.ClrType)
                .Where(expected.Contains)
                .OrderBy(type => type.FullName, StringComparer.Ordinal));

        Assert.Equal("CatalogueDrafts", Entity<CatalogueDraft>(context).GetTableName());
        Assert.Equal("CatalogueVersions", Entity<CatalogueVersion>(context).GetTableName());
        Assert.Equal("Programs", Entity<StudentRegistration.Academics.Domain.Program>(context).GetTableName());
        Assert.Equal("Courses", Entity<Course>(context).GetTableName());
        Assert.Equal("CurriculumCourses", Entity<CurriculumCourse>(context).GetTableName());
        Assert.Equal("CoursePrerequisites", Entity<CoursePrerequisite>(context).GetTableName());
        Assert.Equal("PolicySets", Entity<PolicySet>(context).GetTableName());
        Assert.Equal("PolicyRules", Entity<PolicyRule>(context).GetTableName());
        Assert.Equal("ImportBatches", Entity<ImportBatch>(context).GetTableName());
    }

    [Fact]
    public void Mapping_enforces_normalized_uniqueness_same_version_graph_and_immutable_history_links()
    {
        using var context = CreateContext();
        var program = Entity<StudentRegistration.Academics.Domain.Program>(context);
        var course = Entity<Course>(context);
        var curriculum = Entity<CurriculumCourse>(context);
        var prerequisite = Entity<CoursePrerequisite>(context);
        var draft = Entity<CatalogueDraft>(context);
        var version = Entity<CatalogueVersion>(context);
        var policy = Entity<PolicySet>(context);
        var rule = Entity<PolicyRule>(context);
        var import = Entity<ImportBatch>(context);

        AssertUnique(program, nameof(StudentRegistration.Academics.Domain.Program.CatalogueVersionId), nameof(StudentRegistration.Academics.Domain.Program.Code));
        AssertUnique(course, nameof(Course.CatalogueVersionId), nameof(Course.Code));
        AssertUnique(version, nameof(CatalogueVersion.ScopeCode), nameof(CatalogueVersion.VersionCode));
        AssertUnique(version, nameof(CatalogueVersion.SourceDraftId));
        AssertUnique(policy, nameof(PolicySet.ScopeCode), nameof(PolicySet.VersionCode));
        AssertUnique(rule, nameof(PolicyRule.PolicySetId), nameof(PolicyRule.Code));
        AssertIndex(import, nameof(ImportBatch.CatalogueDraftId), nameof(ImportBatch.State));

        Assert.Equal(
            [nameof(CurriculumCourse.CatalogueVersionId), nameof(CurriculumCourse.ProgramId), nameof(CurriculumCourse.CourseId)],
            curriculum.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Equal(
            [nameof(CoursePrerequisite.CatalogueVersionId), nameof(CoursePrerequisite.CourseId), nameof(CoursePrerequisite.RequiredCourseId)],
            prerequisite.FindPrimaryKey()!.Properties.Select(property => property.Name));

        AssertCompositeForeignKey<CurriculumCourse, StudentRegistration.Academics.Domain.Program>(
            curriculum,
            nameof(CurriculumCourse.CatalogueVersionId),
            nameof(CurriculumCourse.ProgramId));
        AssertCompositeForeignKey<CurriculumCourse, Course>(
            curriculum,
            nameof(CurriculumCourse.CatalogueVersionId),
            nameof(CurriculumCourse.CourseId));
        Assert.Equal(
            2,
            prerequisite.GetForeignKeys().Count(foreignKey =>
                foreignKey.PrincipalEntityType.ClrType == typeof(Course)
                && foreignKey.Properties.Select(property => property.Name).First()
                    == nameof(CoursePrerequisite.CatalogueVersionId)));

        AssertForeignKey<CatalogueDraft, CatalogueVersion>(draft, nameof(CatalogueDraft.BasedOnVersionId));
        AssertForeignKey<CatalogueVersion, CatalogueDraft>(version, nameof(CatalogueVersion.SourceDraftId));
        AssertForeignKey<CatalogueVersion, CatalogueVersion>(version, nameof(CatalogueVersion.SupersedesId));
        AssertForeignKey<ImportBatch, CatalogueDraft>(import, nameof(ImportBatch.CatalogueDraftId));
        AssertForeignKey<ImportBatch, CatalogueVersion>(import, nameof(ImportBatch.PublishedVersionId));
    }

    [Fact]
    public void Mutable_roots_use_rowversion_and_provenance_and_lifecycles_are_bounded()
    {
        using var context = CreateContext();

        AssertRowVersion<CatalogueDraft>(context, nameof(CatalogueDraft.Version));
        AssertRowVersion<CatalogueVersion>(context, nameof(CatalogueVersion.Version));
        AssertRowVersion<StudentRegistration.Academics.Domain.Program>(
            context,
            nameof(StudentRegistration.Academics.Domain.Program.Version));
        AssertRowVersion<Course>(context, nameof(Course.Version));
        AssertRowVersion<PolicySet>(context, nameof(PolicySet.VersionToken));
        AssertRowVersion<ImportBatch>(context, nameof(ImportBatch.Version));

        foreach (var type in new[]
                 {
                     typeof(StudentRegistration.Academics.Domain.Program),
                     typeof(Course),
                     typeof(CurriculumCourse),
                     typeof(CoursePrerequisite),
                 })
        {
            var owned = Assert.Single(
                context.Model.GetEntityTypes(),
                entity => entity.IsOwned()
                    && entity.ClrType == typeof(CatalogueFieldProvenance)
                    && entity.FindOwnership()!.PrincipalEntityType.ClrType == type);
            Assert.NotNull(owned.FindProperty(nameof(CatalogueFieldProvenance.SourceReference)));
            Assert.NotNull(owned.FindProperty(nameof(CatalogueFieldProvenance.AccessedOn)));
            Assert.NotNull(owned.FindProperty(nameof(CatalogueFieldProvenance.SourceKind)));
            Assert.NotNull(owned.FindProperty(nameof(CatalogueFieldProvenance.SyntheticFields)));
        }

        var script = context.Database.GenerateCreateScript();
        Assert.Contains("CK_CatalogueDrafts_State", script, StringComparison.Ordinal);
        Assert.Contains("CK_CatalogueVersions_State", script, StringComparison.Ordinal);
        Assert.Contains("CK_ImportBatches_State", script, StringComparison.Ordinal);
        Assert.Contains("CK_PolicySets_State", script, StringComparison.Ordinal);
        Assert.Contains("CK_Courses_Credits", script, StringComparison.Ordinal);
        Assert.Contains("[Credits] = 3", script, StringComparison.Ordinal);
        Assert.Contains("CK_CoursePrerequisites_NotSelf", script, StringComparison.Ordinal);
        Assert.Contains("CREATE UNIQUE INDEX", script, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Real_sql_materializes_the_catalogue_mapping_without_claiming_the_s2_migration()
    {
        await using var sqlServer = new SqlServerContainerFixture();
        await sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString)
        {
            InitialCatalog = $"StudentRegistration_Test_Spec009_{Guid.NewGuid():N}"
        }.ConnectionString;
        await using var context = CreateContext(connectionString);

        try
        {
            Assert.True(await context.Database.EnsureCreatedAsync());

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            Assert.Equal(
                9,
                await ExecuteCountAsync(
                    connection,
                    """
                    SELECT COUNT(*)
                    FROM sys.tables AS t
                    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                    WHERE s.name = N'academics'
                      AND t.name IN
                      (
                          N'CatalogueDrafts',
                          N'CatalogueVersions',
                          N'Programs',
                          N'Courses',
                          N'CurriculumCourses',
                          N'CoursePrerequisites',
                          N'PolicySets',
                          N'PolicyRules',
                          N'ImportBatches'
                      );
                    """));
            Assert.Equal(
                10,
                await ExecuteCountAsync(
                    connection,
                    """
                    SELECT COUNT(*)
                    FROM sys.check_constraints
                    WHERE name IN
                    (
                        N'CK_CatalogueDrafts_State',
                        N'CK_CatalogueVersions_State',
                        N'CK_Courses_Credits',
                        N'CK_CurriculumCourses_Level',
                        N'CK_CurriculumCourses_RecommendedTerm',
                        N'CK_CoursePrerequisites_NotSelf',
                        N'CK_PolicySets_State',
                        N'CK_PolicySets_EffectiveRange',
                        N'CK_ImportBatches_State',
                        N'CK_ImportBatches_SyntheticFieldCount'
                    );
                    """));
            Assert.Equal(
                6,
                await ExecuteCountAsync(
                    connection,
                    """
                    SELECT COUNT(*)
                    FROM sys.columns AS c
                    INNER JOIN sys.tables AS t ON t.object_id = c.object_id
                    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                    WHERE s.name = N'academics'
                      AND c.system_type_id = 189
                      AND
                      (
                          (t.name = N'CatalogueDrafts' AND c.name = N'Version')
                          OR (t.name = N'CatalogueVersions' AND c.name = N'Version')
                          OR (t.name = N'Programs' AND c.name = N'Version')
                          OR (t.name = N'Courses' AND c.name = N'Version')
                          OR (t.name = N'PolicySets' AND c.name = N'VersionToken')
                          OR (t.name = N'ImportBatches' AND c.name = N'Version')
                      );
                    """));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static StudentRegistrationDbContext CreateContext(
        string connectionString = ConnectionString)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static IEntityType Entity<TEntity>(StudentRegistrationDbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"{typeof(TEntity).Name} is missing.");

    private static async Task<int> ExecuteCountAsync(
        SqlConnection connection,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static void AssertRowVersion<TEntity>(
        StudentRegistrationDbContext context,
        string propertyName)
    {
        var property = Entity<TEntity>(context).FindProperty(propertyName);
        Assert.NotNull(property);
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    private static void AssertUnique(IEntityType entity, params string[] properties)
    {
        var index = FindIndex(entity, properties);
        Assert.True(index.IsUnique);
    }

    private static void AssertIndex(IEntityType entity, params string[] properties) =>
        Assert.NotNull(FindIndex(entity, properties));

    private static IIndex FindIndex(IEntityType entity, params string[] properties) =>
        entity.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(properties));

    private static void AssertForeignKey<TDependent, TPrincipal>(
        IEntityType dependent,
        params string[] properties) =>
        Assert.Contains(
            dependent.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(TPrincipal)
                && foreignKey.Properties.Select(property => property.Name).SequenceEqual(properties));

    private static void AssertCompositeForeignKey<TDependent, TPrincipal>(
        IEntityType dependent,
        params string[] properties) =>
        AssertForeignKey<TDependent, TPrincipal>(dependent, properties);
}
