using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class RegistrationPlanModelConfiguration :
    IEntityTypeConfiguration<RegistrationPlan>,
    IEntityTypeConfiguration<RegistrationPlanItem>
{
    private const string Schema = "registration";

    private static readonly ValueConverter<RegistrationPlanState, string>
        StateConverter = new(
            state => StateToToken(state),
            value => StateFromToken(value));

    private static readonly ValueConverter<IReadOnlyList<ScheduleConflict>, string>
        ConflictsConverter = new(
            value => SerializeConflicts(value),
            value => DeserializeConflicts(value));

    private static readonly ValueComparer<IReadOnlyList<ScheduleConflict>>
        ConflictsComparer = new(
            (left, right) => SerializeConflicts(left!) == SerializeConflicts(right!),
            value => SerializeConflicts(value).GetHashCode(StringComparison.Ordinal),
            value => DeserializeConflicts(SerializeConflicts(value)));

    private static readonly ValueConverter<ValidationSnapshot?, string?>
        ValidationConverter = new(
            value => SerializeValidation(value),
            value => DeserializeValidation(value));

    private static readonly ValueComparer<ValidationSnapshot?> ValidationComparer = new(
        (left, right) => SerializeValidation(left) == SerializeValidation(right),
        value => (SerializeValidation(value) ?? string.Empty)
            .GetHashCode(StringComparison.Ordinal),
        value => DeserializeValidation(SerializeValidation(value)));

    public void Configure(EntityTypeBuilder<RegistrationPlan> builder)
    {
        builder.ToTable(
            "RegistrationPlans",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RegistrationPlans_State",
                    "[State] IN ('draft', 'review-blocked')");
                table.HasCheckConstraint(
                    "CK_RegistrationPlans_TotalCredits",
                    "[TotalCredits] >= 0");
                table.HasCheckConstraint(
                    "CK_RegistrationPlans_ConflictsJson",
                    "ISJSON([ConflictsJson]) = 1");
                table.HasCheckConstraint(
                    "CK_RegistrationPlans_ValidationSnapshotJson",
                    "[ValidationSnapshotJson] IS NULL OR ISJSON([ValidationSnapshotJson]) = 1");
            });
        builder.HasKey(plan => plan.Id);
        builder.Property(plan => plan.Id).ValueGeneratedNever();
        builder.Property(plan => plan.StudentId).IsRequired();
        builder.Property(plan => plan.TermId).IsRequired();
        builder.Property(plan => plan.TotalCredits)
            .HasColumnType("decimal(6,2)")
            .IsRequired();
        builder.Property(plan => plan.State)
            .HasConversion(StateConverter)
            .HasMaxLength(30)
            .IsRequired();
        var conflicts = builder.Property(plan => plan.Conflicts)
            .HasColumnName("ConflictsJson")
            .HasColumnType("nvarchar(max)")
            .HasConversion(ConflictsConverter)
            .IsRequired();
        conflicts.Metadata.SetValueComparer(ConflictsComparer);
        var validation = builder.Property(plan => plan.Validation)
            .HasColumnName("ValidationSnapshotJson")
            .HasColumnType("nvarchar(max)")
            .HasConversion(ValidationConverter);
        validation.Metadata.SetValueComparer(ValidationComparer);
        builder.Property(plan => plan.Version).IsRowVersion().IsRequired();
        builder.Ignore(plan => plan.ReviewBlocked);

        builder.HasIndex(plan => new { plan.StudentId, plan.TermId })
            .IsUnique();
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(plan => plan.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(plan => plan.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(plan => plan.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<RegistrationPlanItem> builder)
    {
        builder.ToTable("RegistrationPlanItems", Schema);
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.PlanId).IsRequired();
        builder.Property(item => item.OfferingId).IsRequired();
        builder.Property(item => item.SelectedGroupId).IsRequired();
        builder.Property(item => item.CapturedOfferingVersion)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(item => item.CapturedGroupVersion)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(item => new { item.PlanId, item.OfferingId })
            .IsUnique();
        builder.HasIndex(item => new { item.PlanId, item.SelectedGroupId });
        builder.HasOne<RegistrationPlan>()
            .WithMany(plan => plan.Items)
            .HasForeignKey(item => item.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(item => item.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionGroup>()
            .WithMany()
            .HasForeignKey(item => new { item.OfferingId, item.SelectedGroupId })
            .HasPrincipalKey(group => new { group.OfferingId, group.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static string StateToToken(RegistrationPlanState state) => state switch
    {
        RegistrationPlanState.Draft => "draft",
        RegistrationPlanState.ReviewBlocked => "review-blocked",
        _ => throw new InvalidOperationException("Unknown registration-plan state.")
    };

    private static RegistrationPlanState StateFromToken(string value) => value switch
    {
        "draft" => RegistrationPlanState.Draft,
        "review-blocked" => RegistrationPlanState.ReviewBlocked,
        _ => throw new InvalidOperationException("Unknown persisted registration-plan state.")
    };

    private static string SerializeConflicts(IReadOnlyList<ScheduleConflict> value) =>
        JsonSerializer.Serialize(value);

    private static IReadOnlyList<ScheduleConflict> DeserializeConflicts(string value) =>
        Array.AsReadOnly(JsonSerializer.Deserialize<ScheduleConflict[]>(value) ?? []);

    private static string? SerializeValidation(ValidationSnapshot? value) =>
        value is null ? null : JsonSerializer.Serialize(value);

    private static ValidationSnapshot? DeserializeValidation(string? value) =>
        value is null ? null : JsonSerializer.Deserialize<ValidationSnapshot>(value);
}
