using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class RegistrationDiscoveryModelConfiguration :
    IEntityTypeConfiguration<OfferingEligibilityReadModel>,
    IEntityTypeConfiguration<EligibilityReasonReadModel>,
    IEntityTypeConfiguration<GroupSummaryReadModel>
{
    private const string Schema = "registration";

    public void Configure(EntityTypeBuilder<OfferingEligibilityReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("OfferingEligibility", Schema);
        builder.Property(item => item.OfferingId);
        builder.Property(item => item.CourseCode).HasMaxLength(50);
        builder.Property(item => item.Eligible);
    }

    public void Configure(EntityTypeBuilder<EligibilityReasonReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("EligibilityReason", Schema);
        builder.Property(item => item.OfferingId);
        builder.Property(item => item.Code).HasMaxLength(100);
        builder.Property(item => item.Blocking);
    }

    public void Configure(EntityTypeBuilder<GroupSummaryReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("GroupSummary", Schema);
        builder.Property(item => item.OfferingId);
        builder.Property(item => item.GroupId);
        builder.Property(item => item.GroupCode).HasMaxLength(50);
        builder.Property(item => item.Selectable);
    }
}

public sealed class OfferingEligibilityReadModel
{
    public Guid OfferingId { get; init; }

    public string CourseCode { get; init; } = string.Empty;

    public bool Eligible { get; init; }
}

public sealed class EligibilityReasonReadModel
{
    public Guid OfferingId { get; init; }

    public string Code { get; init; } = string.Empty;

    public bool Blocking { get; init; }
}

public sealed class GroupSummaryReadModel
{
    public Guid OfferingId { get; init; }

    public Guid GroupId { get; init; }

    public string GroupCode { get; init; } = string.Empty;

    public bool Selectable { get; init; }
}
