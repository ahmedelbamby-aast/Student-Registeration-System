using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Persistence;

public sealed class RegistrationApprovalModelConfigurationTests
{
    [Fact]
    public void Approval_hold_and_batch_entities_have_owned_tables_keys_constraints_and_versions()
    {
        using var context = CreateContext();

        var line = Entity<RegistrationSubmissionLine>(context);
        Assert.Equal("RegistrationSubmissionLines", line.GetTableName());
        Assert.Equal("registration", line.GetSchema());
        AssertUnique(line, nameof(RegistrationSubmissionLine.SubmissionId), nameof(RegistrationSubmissionLine.OfferingId));
        AssertRowVersion(line, nameof(RegistrationSubmissionLine.Version));

        var hold = Entity<RegistrationSeatHold>(context);
        Assert.Equal("RegistrationSeatHolds", hold.GetTableName());
        AssertUnique(hold, nameof(RegistrationSeatHold.SubmissionLineId));
        AssertRowVersion(hold, nameof(RegistrationSeatHold.Version));

        var decision = Entity<RegistrationApprovalDecision>(context);
        Assert.Equal("RegistrationApprovalDecisions", decision.GetTableName());
        AssertUnique(decision, nameof(RegistrationApprovalDecision.SubmissionLineId));
        AssertUnique(decision,
            nameof(RegistrationApprovalDecision.ActorId),
            nameof(RegistrationApprovalDecision.SubmissionLineId),
            nameof(RegistrationApprovalDecision.ClientRequestId));

        var batch = Entity<FirstTermAutoEnrollmentBatch>(context);
        AssertUnique(batch,
            nameof(FirstTermAutoEnrollmentBatch.TermId),
            nameof(FirstTermAutoEnrollmentBatch.CatalogueVersionId),
            nameof(FirstTermAutoEnrollmentBatch.CohortScope),
            nameof(FirstTermAutoEnrollmentBatch.Purpose));
        AssertRowVersion(batch, nameof(FirstTermAutoEnrollmentBatch.Version));

        var item = Entity<FirstTermAutoEnrollmentItem>(context);
        AssertUnique(item,
            nameof(FirstTermAutoEnrollmentItem.BatchId),
            nameof(FirstTermAutoEnrollmentItem.StudentId));
        AssertRowVersion(item, nameof(FirstTermAutoEnrollmentItem.Version));
    }

    [Fact]
    public void Submission_and_section_group_mapping_include_new_lifecycle_and_occupied_capacity()
    {
        using var context = CreateContext();
        var submission = Entity<RegistrationSubmission>(context);
        Assert.NotNull(submission.FindProperty(nameof(RegistrationSubmission.Origin)));
        Assert.NotNull(submission.FindProperty(nameof(RegistrationSubmission.RequestedCredits)));

        var group = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(SectionGroup))!;
        Assert.NotNull(group.FindProperty(nameof(SectionGroup.HeldSeatCount)));
        var table = StoreObjectIdentifier.Table(group.GetTableName()!, group.GetSchema());
        Assert.Contains(group.GetCheckConstraints(), constraint =>
            constraint.GetName(table) == "CK_SectionGroups_Capacity" &&
            constraint.Sql.Contains("HeldSeatCount", StringComparison.Ordinal));
    }

    [Fact]
    public void DbContext_exposes_approval_and_batch_sets()
    {
        var properties = typeof(StudentRegistrationDbContext).GetProperties();
        foreach (var name in new[]
        {
            nameof(StudentRegistrationDbContext.RegistrationSubmissions),
            nameof(StudentRegistrationDbContext.RegistrationSubmissionLines),
            nameof(StudentRegistrationDbContext.RegistrationSeatHolds),
            nameof(StudentRegistrationDbContext.RegistrationApprovalDecisions),
            nameof(StudentRegistrationDbContext.FirstTermAutoEnrollmentBatches),
            nameof(StudentRegistrationDbContext.FirstTermAutoEnrollmentItems)
        })
        {
            Assert.Contains(properties, property => property.Name == name);
        }
    }

    private static StudentRegistrationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer("Server=localhost;Database=Spec014Model;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new StudentRegistrationDbContext(options);
    }

    private static IEntityType Entity<TEntity>(DbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))!;

    private static void AssertUnique(IEntityType entity, params string[] properties) =>
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(properties));

    private static void AssertRowVersion(IEntityType entity, string propertyName)
    {
        var property = entity.FindProperty(propertyName)!;
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }
}
