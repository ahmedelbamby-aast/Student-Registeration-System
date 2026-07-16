using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class SchedulingModelConfiguration :
    IEntityTypeConfiguration<CourseOffering>,
    IEntityTypeConfiguration<SectionGroup>,
    IEntityTypeConfiguration<MeetingSlot>,
    IEntityTypeConfiguration<Room>,
    IEntityTypeConfiguration<GroupStaffAssignment>,
    IEntityTypeConfiguration<StaffTermAvailability>,
    IEntityTypeConfiguration<StaffAvailability>,
    IEntityTypeConfiguration<ScheduleImpactAlert>
{
    private const string Schema = "scheduling";

    private static readonly ValueConverter<CourseOfferingState, string>
        OfferingStateConverter = new(
            value => OfferingStateToToken(value),
            value => OfferingStateFromToken(value));

    private static readonly ValueConverter<SectionGroupState, string>
        GroupStateConverter = new(
            value => GroupStateToToken(value),
            value => GroupStateFromToken(value));

    private static readonly ValueConverter<ActivityType, string>
        ActivityTypeConverter = new(
            value => ActivityTypeToToken(value),
            value => ActivityTypeFromToken(value));

    private static readonly ValueConverter<RoomAvailabilityState, string>
        RoomStateConverter = new(
            value => RoomStateToToken(value),
            value => RoomStateFromToken(value));

    private static readonly ValueConverter<TeachingRole, string>
        TeachingRoleConverter = new(
            value => TeachingRoleToToken(value),
            value => TeachingRoleFromToken(value));

    private static readonly ValueConverter<AvailabilityKind, string>
        AvailabilityKindConverter = new(
            value => AvailabilityKindToToken(value),
            value => AvailabilityKindFromToken(value));

    private static readonly ValueConverter<ScheduleImpactAlertState, string>
        AlertStateConverter = new(
            value => AlertStateToToken(value),
            value => AlertStateFromToken(value));

    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcConverter =
        new(
            value => value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value);

    public void Configure(EntityTypeBuilder<CourseOffering> builder)
    {
        builder.ToTable(
            "CourseOfferings",
            Schema,
            table => table.HasCheckConstraint(
                "CK_CourseOfferings_State",
                "[State] IN ('draft', 'published', 'closed', 'cancelled')"));
        ConfigureGuidKey(builder);
        builder.Property(offering => offering.TermId).IsRequired();
        builder.Property(offering => offering.CourseId).IsRequired();
        builder.Property(offering => offering.State)
            .HasConversion(OfferingStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        ConfigureRowVersion(builder.Property(offering => offering.Version));

        builder.HasIndex(offering => new { offering.TermId, offering.CourseId })
            .IsUnique();
        builder.HasIndex(offering => new
        {
            offering.TermId,
            offering.State,
            offering.Id
        });
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(offering => offering.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(offering => offering.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<SectionGroup> builder)
    {
        builder.ToTable(
            "SectionGroups",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_SectionGroups_Capacity",
                    "[Capacity] >= 0 AND [EnrolledCount] >= 0 AND [EnrolledCount] <= [Capacity]");
                table.HasCheckConstraint(
                    "CK_SectionGroups_State",
                    "[State] IN ('draft', 'published', 'closed', 'cancelled')");
            });
        ConfigureGuidKey(builder);
        builder.Property(group => group.OfferingId).IsRequired();
        builder.Property(group => group.GroupCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(group => group.Capacity).IsRequired();
        builder.Property(group => group.EnrolledCount).IsRequired();
        builder.Property(group => group.State)
            .HasConversion(GroupStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(group => group.RegistrationPaused).IsRequired();
        ConfigureRowVersion(builder.Property(group => group.Version));

        builder.HasAlternateKey(group => new { group.Id, group.OfferingId });
        builder.HasIndex(group => new { group.OfferingId, group.GroupCode })
            .IsUnique();
        builder.HasIndex(group => new
        {
            group.OfferingId,
            group.State,
            group.RegistrationPaused,
            group.Id
        });
        builder.HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(group => group.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        var meetings = builder.HasMany(group => group.Meetings)
            .WithOne()
            .HasForeignKey(meeting => meeting.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        meetings.Metadata.PrincipalToDependent!.SetPropertyAccessMode(
            PropertyAccessMode.Field);

        var assignments = builder.HasMany(group => group.StaffAssignments)
            .WithOne()
            .HasForeignKey(assignment => assignment.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        assignments.Metadata.PrincipalToDependent!.SetPropertyAccessMode(
            PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<MeetingSlot> builder)
    {
        builder.ToTable(
            "MeetingSlots",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_MeetingSlots_Range",
                    "[EndLocal] > [StartLocal]");
                table.HasCheckConstraint(
                    "CK_MeetingSlots_DayOfWeek",
                    "[DayOfWeek] >= 0 AND [DayOfWeek] <= 6");
                table.HasCheckConstraint(
                    "CK_MeetingSlots_ActivityType",
                    "[ActivityType] IN ('lecture', 'tutorial', 'laboratory')");
            });
        ConfigureGuidKey(builder);
        builder.Property(meeting => meeting.GroupId).IsRequired();
        builder.Property(meeting => meeting.RoomId).IsRequired();
        builder.Property(meeting => meeting.ActivityType)
            .HasConversion(ActivityTypeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(meeting => meeting.DayOfWeek).IsRequired();
        builder.Property(meeting => meeting.StartLocal)
            .HasColumnType("time")
            .IsRequired();
        builder.Property(meeting => meeting.EndLocal)
            .HasColumnType("time")
            .IsRequired();

        builder.HasAlternateKey(meeting => new { meeting.Id, meeting.GroupId });
        builder.HasIndex(meeting => new
        {
            meeting.RoomId,
            meeting.DayOfWeek,
            meeting.StartLocal,
            meeting.EndLocal,
            meeting.GroupId
        });
        builder.HasIndex(meeting => new
        {
            meeting.GroupId,
            meeting.ActivityType,
            meeting.DayOfWeek,
            meeting.StartLocal,
            meeting.Id
        });
        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(meeting => meeting.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable(
            "Rooms",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Rooms_Capacity",
                    "[Capacity] >= 0");
                table.HasCheckConstraint(
                    "CK_Rooms_AvailabilityState",
                    "[AvailabilityState] IN ('available', 'unavailable')");
            });
        ConfigureGuidKey(builder);
        builder.Property(room => room.Code)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(room => room.Location)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(room => room.Capacity).IsRequired();
        builder.Property(room => room.AvailabilityState)
            .HasConversion(RoomStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        ConfigureRowVersion(builder.Property(room => room.Version));

        builder.HasIndex(room => room.Code).IsUnique();
        builder.HasIndex(room => new
        {
            room.AvailabilityState,
            room.Capacity,
            room.Code,
            room.Id
        });
    }

    public void Configure(EntityTypeBuilder<GroupStaffAssignment> builder)
    {
        builder.ToTable(
            "GroupStaffAssignments",
            Schema,
            table => table.HasCheckConstraint(
                "CK_GroupStaffAssignments_RoleActivity",
                "([ActivityType] = 'lecture' AND [TeachingRole] = 'lecturer') OR " +
                "([ActivityType] IN ('tutorial', 'laboratory') AND [TeachingRole] = 'teaching-assistant')"));
        builder.HasKey(assignment => new
        {
            assignment.MeetingSlotId,
            assignment.StaffId,
            assignment.TeachingRole
        });
        builder.Property(assignment => assignment.GroupId).IsRequired();
        builder.Property(assignment => assignment.MeetingSlotId).IsRequired();
        builder.Property(assignment => assignment.ActivityType)
            .HasConversion(ActivityTypeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(assignment => assignment.StaffId).IsRequired();
        builder.Property(assignment => assignment.TeachingRole)
            .HasConversion(TeachingRoleConverter)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(assignment => new
        {
            assignment.StaffId,
            assignment.GroupId,
            assignment.MeetingSlotId
        });
        builder.HasIndex(assignment => new
        {
            assignment.GroupId,
            assignment.ActivityType,
            assignment.MeetingSlotId,
            assignment.StaffId
        });
        builder.HasOne<MeetingSlot>()
            .WithMany()
            .HasForeignKey(assignment => new
            {
                assignment.MeetingSlotId,
                assignment.GroupId
            })
            .HasPrincipalKey(meeting => new { meeting.Id, meeting.GroupId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(assignment => assignment.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<StaffTermAvailability> builder)
    {
        builder.ToTable("StaffTermAvailabilities", Schema);
        ConfigureGuidKey(builder);
        builder.Property(availability => availability.StaffId).IsRequired();
        builder.Property(availability => availability.TermId).IsRequired();
        ConfigureUtc(builder.Property(availability => availability.DeadlineUtc));
        ConfigureRowVersion(builder.Property(availability => availability.Version));

        builder.HasIndex(availability => new
        {
            availability.StaffId,
            availability.TermId
        }).IsUnique();
        builder.HasIndex(availability => new
        {
            availability.TermId,
            availability.DeadlineUtc,
            availability.Id
        });
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(availability => availability.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(availability => availability.TermId)
            .OnDelete(DeleteBehavior.Restrict);

        var ranges = builder.HasMany(availability => availability.Ranges)
            .WithOne()
            .HasForeignKey(range => range.StaffTermAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);
        ranges.Metadata.PrincipalToDependent!.SetPropertyAccessMode(
            PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<StaffAvailability> builder)
    {
        builder.ToTable(
            "StaffAvailabilities",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_StaffAvailabilities_Range",
                    "[EndLocal] > [StartLocal]");
                table.HasCheckConstraint(
                    "CK_StaffAvailabilities_DayOfWeek",
                    "[DayOfWeek] >= 0 AND [DayOfWeek] <= 6");
                table.HasCheckConstraint(
                    "CK_StaffAvailabilities_Kind",
                    "[Kind] IN ('available', 'unavailable')");
            });
        ConfigureGuidKey(builder);
        builder.Property(range => range.StaffTermAvailabilityId).IsRequired();
        builder.Property(range => range.DayOfWeek).IsRequired();
        builder.Property(range => range.StartLocal)
            .HasColumnType("time")
            .IsRequired();
        builder.Property(range => range.EndLocal)
            .HasColumnType("time")
            .IsRequired();
        builder.Property(range => range.Kind)
            .HasConversion(AvailabilityKindConverter)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(range => new
        {
            range.StaffTermAvailabilityId,
            range.DayOfWeek,
            range.StartLocal,
            range.EndLocal
        });
    }

    public void Configure(EntityTypeBuilder<ScheduleImpactAlert> builder)
    {
        builder.ToTable(
            "ScheduleImpactAlerts",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_ScheduleImpactAlerts_State",
                    "[State] IN ('open', 'revalidated', 'resolved')");
                table.HasCheckConstraint(
                    "CK_ScheduleImpactAlerts_Resource",
                    "[StaffTermAvailabilityId] IS NOT NULL OR [RoomId] IS NOT NULL");
            });
        ConfigureGuidKey(builder);
        builder.Property(alert => alert.GroupId).IsRequired();
        builder.Property(alert => alert.StaffTermAvailabilityId);
        builder.Property(alert => alert.RoomId);
        builder.Property(alert => alert.ReasonCode)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(alert => alert.DetectedGroupVersion)
            .HasColumnType("varbinary(8)")
            .IsRequired();
        builder.Property(alert => alert.DetectedResourceVersion)
            .HasColumnType("varbinary(8)")
            .IsRequired();
        builder.Property(alert => alert.State)
            .HasConversion(AlertStateConverter)
            .HasMaxLength(20)
            .IsRequired();
        ConfigureUtc(builder.Property(alert => alert.DetectedAtUtc));
        ConfigureNullableUtc(builder.Property(alert => alert.RevalidatedAtUtc));
        ConfigureNullableUtc(builder.Property(alert => alert.ResolvedAtUtc));
        builder.Property(alert => alert.LastValidationJson)
            .HasColumnType("nvarchar(max)");
        builder.Property(alert => alert.LastRevalidationPassed).IsRequired();
        builder.Property(alert => alert.ResolutionReason).HasMaxLength(1000);
        ConfigureRowVersion(builder.Property(alert => alert.Version));

        builder.HasIndex(alert => new
        {
            alert.State,
            alert.DetectedAtUtc,
            alert.Id
        });
        builder.HasIndex(alert => new
        {
            alert.GroupId,
            alert.State,
            alert.Id
        });
        builder.HasIndex(alert => new
        {
            alert.StaffTermAvailabilityId,
            alert.State,
            alert.Id
        });
        builder.HasIndex(alert => new
        {
            alert.RoomId,
            alert.State,
            alert.Id
        });
        builder.HasOne<SectionGroup>()
            .WithMany()
            .HasForeignKey(alert => alert.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StaffTermAvailability>()
            .WithMany()
            .HasForeignKey(alert => alert.StaffTermAvailabilityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(alert => alert.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureGuidKey<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<Guid>("Id").ValueGeneratedNever();
    }

    private static void ConfigureRowVersion(PropertyBuilder<byte[]> property) =>
        property.IsRowVersion().IsRequired();

    private static void ConfigureUtc(PropertyBuilder<DateTime> property) =>
        property.HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

    private static void ConfigureNullableUtc(PropertyBuilder<DateTime?> property) =>
        property.HasConversion(NullableUtcConverter)
            .HasColumnType("datetime2");

    private static string OfferingStateToToken(CourseOfferingState value) => value switch
    {
        CourseOfferingState.Draft => "draft",
        CourseOfferingState.Published => "published",
        CourseOfferingState.Closed => "closed",
        CourseOfferingState.Cancelled => "cancelled",
        _ => throw new InvalidOperationException("Unknown course-offering state.")
    };

    private static CourseOfferingState OfferingStateFromToken(string value) => value switch
    {
        "draft" => CourseOfferingState.Draft,
        "published" => CourseOfferingState.Published,
        "closed" => CourseOfferingState.Closed,
        "cancelled" => CourseOfferingState.Cancelled,
        _ => throw new InvalidOperationException("Unknown persisted course-offering state.")
    };

    private static string GroupStateToToken(SectionGroupState value) => value switch
    {
        SectionGroupState.Draft => "draft",
        SectionGroupState.Published => "published",
        SectionGroupState.Closed => "closed",
        SectionGroupState.Cancelled => "cancelled",
        _ => throw new InvalidOperationException("Unknown section-group state.")
    };

    private static SectionGroupState GroupStateFromToken(string value) => value switch
    {
        "draft" => SectionGroupState.Draft,
        "published" => SectionGroupState.Published,
        "closed" => SectionGroupState.Closed,
        "cancelled" => SectionGroupState.Cancelled,
        _ => throw new InvalidOperationException("Unknown persisted section-group state.")
    };

    private static string ActivityTypeToToken(ActivityType value) => value switch
    {
        ActivityType.Lecture => "lecture",
        ActivityType.Tutorial => "tutorial",
        ActivityType.Laboratory => "laboratory",
        _ => throw new InvalidOperationException("Unknown activity type.")
    };

    private static ActivityType ActivityTypeFromToken(string value) => value switch
    {
        "lecture" => ActivityType.Lecture,
        "tutorial" => ActivityType.Tutorial,
        "laboratory" => ActivityType.Laboratory,
        _ => throw new InvalidOperationException("Unknown persisted activity type.")
    };

    private static string RoomStateToToken(RoomAvailabilityState value) => value switch
    {
        RoomAvailabilityState.Available => "available",
        RoomAvailabilityState.Unavailable => "unavailable",
        _ => throw new InvalidOperationException("Unknown room availability state.")
    };

    private static RoomAvailabilityState RoomStateFromToken(string value) => value switch
    {
        "available" => RoomAvailabilityState.Available,
        "unavailable" => RoomAvailabilityState.Unavailable,
        _ => throw new InvalidOperationException("Unknown persisted room availability state.")
    };

    private static string TeachingRoleToToken(TeachingRole value) => value switch
    {
        TeachingRole.Lecturer => "lecturer",
        TeachingRole.TeachingAssistant => "teaching-assistant",
        _ => throw new InvalidOperationException("Unknown teaching role.")
    };

    private static TeachingRole TeachingRoleFromToken(string value) => value switch
    {
        "lecturer" => TeachingRole.Lecturer,
        "teaching-assistant" => TeachingRole.TeachingAssistant,
        _ => throw new InvalidOperationException("Unknown persisted teaching role.")
    };

    private static string AvailabilityKindToToken(AvailabilityKind value) => value switch
    {
        AvailabilityKind.Available => "available",
        AvailabilityKind.Unavailable => "unavailable",
        _ => throw new InvalidOperationException("Unknown staff availability kind.")
    };

    private static AvailabilityKind AvailabilityKindFromToken(string value) => value switch
    {
        "available" => AvailabilityKind.Available,
        "unavailable" => AvailabilityKind.Unavailable,
        _ => throw new InvalidOperationException("Unknown persisted staff availability kind.")
    };

    private static string AlertStateToToken(ScheduleImpactAlertState value) => value switch
    {
        ScheduleImpactAlertState.Open => "open",
        ScheduleImpactAlertState.Revalidated => "revalidated",
        ScheduleImpactAlertState.Resolved => "resolved",
        _ => throw new InvalidOperationException("Unknown schedule-impact alert state.")
    };

    private static ScheduleImpactAlertState AlertStateFromToken(string value) => value switch
    {
        "open" => ScheduleImpactAlertState.Open,
        "revalidated" => ScheduleImpactAlertState.Revalidated,
        "resolved" => ScheduleImpactAlertState.Resolved,
        _ => throw new InvalidOperationException("Unknown persisted schedule-impact alert state.")
    };
}
