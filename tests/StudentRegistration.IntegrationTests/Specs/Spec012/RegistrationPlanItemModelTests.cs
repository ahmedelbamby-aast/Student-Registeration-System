using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec012;

public sealed class RegistrationPlanItemModelTests
{
    private const string ModelConnectionString =
        "Server=localhost;Database=Spec012RegistrationPlanItemModelOnly;User Id=sa;Password=NotUsed!42;TrustServerCertificate=True";

    [Fact]
    public void Item_is_parent_controlled_and_preserves_plan_offering_group_and_versions()
    {
        var id = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var offeringId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var item = Create(
            id,
            planId,
            offeringId,
            groupId,
            "offering/7",
            "group/12");

        Assert.Equal(id, item.Id);
        Assert.Equal(planId, item.PlanId);
        Assert.Equal(offeringId, item.OfferingId);
        Assert.Equal(groupId, item.SelectedGroupId);
        Assert.Equal("offering/7", item.CapturedOfferingVersion);
        Assert.Equal("group/12", item.CapturedGroupVersion);
        AssertPrivateSetter(nameof(RegistrationPlanItem.PlanId));
        AssertPrivateSetter(nameof(RegistrationPlanItem.OfferingId));
        AssertPrivateSetter(nameof(RegistrationPlanItem.SelectedGroupId));
        AssertPrivateSetter(nameof(RegistrationPlanItem.CapturedOfferingVersion));
        AssertPrivateSetter(nameof(RegistrationPlanItem.CapturedGroupVersion));
        Assert.Empty(typeof(RegistrationPlanItem).GetConstructors());
    }

    [Fact]
    public void Item_rejects_missing_identity_ownership_or_versions()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(planId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(offeringId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(groupId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(offeringVersion: " "));
        Assert.Throws<ArgumentException>(() => Create(groupVersion: " "));
    }

    [Fact]
    public void Item_mapping_enforces_unique_plan_offering_and_parent_controlled_updates()
    {
        using var context = CreateContext();
        var item = context.Model.FindEntityType(typeof(RegistrationPlanItem));

        Assert.NotNull(item);
        var planOfferingIndex = Assert.Single(
            item.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(RegistrationPlanItem.PlanId), nameof(RegistrationPlanItem.OfferingId)]));
        Assert.True(planOfferingIndex.IsUnique);

        var parentForeignKey = Assert.Single(
            item.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(RegistrationPlan));
        Assert.Equal(DeleteBehavior.Cascade, parentForeignKey.DeleteBehavior);
        Assert.DoesNotContain(
            item.GetProperties(),
            property => property.IsConcurrencyToken);
        Assert.DoesNotContain(
            typeof(RegistrationPlanItem).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly),
            method => !method.IsSpecialName);
    }

    private static RegistrationPlanItem Create(
        Guid? id = null,
        Guid? planId = null,
        Guid? offeringId = null,
        Guid? groupId = null,
        string offeringVersion = "offering/1",
        string groupVersion = "group/1")
    {
        var plan = new RegistrationPlan(
            planId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            0m,
            RegistrationPlanState.Draft);
        plan.ReplaceSelections(
            [new(
                id ?? Guid.NewGuid(),
                offeringId ?? Guid.NewGuid(),
                groupId ?? Guid.NewGuid(),
                offeringVersion,
                groupVersion)],
            0m,
            RegistrationPlanState.Draft,
            [],
            EmptyValidation());
        return Assert.Single(plan.Items);
    }

    private static ValidationSnapshot EmptyValidation() =>
        new(
            new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
            "academic/1",
            "policy/1",
            "catalogue/1",
            new Dictionary<Guid, string>(),
            new Dictionary<Guid, string>());

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(RegistrationPlanItem).GetProperty(propertyName);
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
