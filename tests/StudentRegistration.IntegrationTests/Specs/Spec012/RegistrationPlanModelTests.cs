using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012;

public sealed class RegistrationPlanModelTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec012RegistrationPlanModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Plan_preserves_owner_term_total_review_state_and_root_version()
    {
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var termId = Guid.NewGuid();

        var plan = new RegistrationPlan(
            id,
            studentId,
            termId,
            15m,
            RegistrationPlanState.ReviewBlocked);

        Assert.Equal(id, plan.Id);
        Assert.Equal(studentId, plan.StudentId);
        Assert.Equal(termId, plan.TermId);
        Assert.Equal(15m, plan.TotalCredits);
        Assert.Equal(RegistrationPlanState.ReviewBlocked, plan.State);
        Assert.True(plan.ReviewBlocked);
        Assert.Empty(plan.Version);
        AssertPrivateSetter(nameof(RegistrationPlan.StudentId));
        AssertPrivateSetter(nameof(RegistrationPlan.TermId));
        AssertPrivateSetter(nameof(RegistrationPlan.TotalCredits));
        AssertPrivateSetter(nameof(RegistrationPlan.State));
        AssertPrivateSetter(nameof(RegistrationPlan.Version));
    }

    [Fact]
    public void Plan_rejects_missing_scope_negative_total_and_unknown_state()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(studentId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(termId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(totalCredits: -1m));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(state: (RegistrationPlanState)999));
    }

    [Fact]
    public void Plan_mapping_enforces_unique_student_term_and_database_rowversion()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(RegistrationPlan));

        Assert.NotNull(entity);
        var scopeIndex = Assert.Single(
            entity.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(RegistrationPlan.StudentId), nameof(RegistrationPlan.TermId)]));
        Assert.True(scopeIndex.IsUnique);

        var version = entity.FindProperty(nameof(RegistrationPlan.Version));
        Assert.NotNull(version);
        Assert.True(version.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
        Assert.Equal("rowversion", version.GetColumnType());
    }

    private static RegistrationPlan Create(
        Guid? id = null,
        Guid? studentId = null,
        Guid? termId = null,
        decimal totalCredits = 0m,
        RegistrationPlanState state = RegistrationPlanState.Draft) =>
        new(
            id ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            totalCredits,
            state);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(RegistrationPlan).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(ModelConnectionString)
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}
