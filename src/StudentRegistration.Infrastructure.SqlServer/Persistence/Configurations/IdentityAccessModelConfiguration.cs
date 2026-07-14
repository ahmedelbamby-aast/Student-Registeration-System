using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class IdentityAccessModelConfiguration :
    IEntityTypeConfiguration<ApplicationUser>,
    IEntityTypeConfiguration<Staff>,
    IEntityTypeConfiguration<StudentActivation>,
    IEntityTypeConfiguration<AccountRecoveryChallenge>,
    IEntityTypeConfiguration<RoleAssignment>,
    IEntityTypeConfiguration<AuthenticationAbuseState>,
    IEntityTypeConfiguration<IdentityImportBatch>,
    IEntityTypeConfiguration<IdentityImportCandidateRow>,
    IEntityTypeConfiguration<SecurityEvent>,
    IEntityTypeConfiguration<AdminSecurityGuard>
{
    private const string Schema = "auth";

    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers", Schema);
        ConfigureGuidKey(builder);

        builder.Property(user => user.UserName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(user => user.NormalizedUserName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(user => user.UniversityId)
            .HasMaxLength(50);
        builder.Property(user => user.PasswordHash)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(user => user.SecurityStamp)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(user => user.IsEnabled)
            .IsRequired();
        builder.Property(user => user.AccessFailedCount)
            .IsRequired();
        builder.Property(user => user.LockoutEndUtc)
            .HasColumnType("datetime2");
        ConfigureRowVersion(builder.Property(user => user.Version));

        builder.HasIndex(user => user.UserName)
            .IsUnique();
        builder.HasIndex(user => user.NormalizedUserName)
            .IsUnique();
        builder.HasIndex(user => user.UniversityId)
            .IsUnique()
            .HasFilter("[UniversityId] IS NOT NULL");
        builder.HasIndex(user => new { user.IsEnabled, user.LockoutEndUtc });
    }

    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff", Schema);
        ConfigureGuidKey(builder);

        builder.Property(staff => staff.StaffNumber)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(staff => staff.DisplayName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(staff => staff.IsActive)
            .IsRequired();

        builder.HasIndex(staff => staff.ApplicationUserId)
            .IsUnique();
        builder.HasIndex(staff => staff.StaffNumber)
            .IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Staff>(staff => staff.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<StudentActivation> builder)
    {
        builder.ToTable("StudentActivations", Schema);
        ConfigureGuidKey(builder);

        builder.Property(activation => activation.ProvisionedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(activation => activation.ActivatedAtUtc)
            .HasColumnType("datetime2");
        builder.Property(activation => activation.FailedAttemptCount)
            .IsRequired();
        ConfigureRowVersion(builder.Property(activation => activation.Version));

        builder.HasIndex(activation => activation.ApplicationUserId)
            .IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<StudentActivation>(activation => activation.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<AccountRecoveryChallenge> builder)
    {
        builder.ToTable("AccountRecoveryChallenges", Schema);
        ConfigureGuidKey(builder);

        builder.Property(challenge => challenge.TokenHash)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(challenge => challenge.DeliveryReferenceHash)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(challenge => challenge.ExpiresAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(challenge => challenge.FailedAttemptCount)
            .IsRequired();
        builder.Property(challenge => challenge.ConsumedAtUtc)
            .HasColumnType("datetime2");
        ConfigureRowVersion(builder.Property(challenge => challenge.Version));

        builder.HasIndex(challenge => challenge.TokenHash)
            .IsUnique();
        builder.HasIndex(challenge => new
        {
            challenge.ApplicationUserId,
            challenge.ExpiresAtUtc,
            challenge.ConsumedAtUtc
        });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(challenge => challenge.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable(
            "RoleAssignments",
            Schema,
            table => table.HasCheckConstraint(
                "CK_RoleAssignments_EffectiveRange",
                "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]"));
        ConfigureGuidKey(builder);

        builder.Property(assignment => assignment.RoleCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(assignment => assignment.EffectiveFromUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(assignment => assignment.EffectiveToUtc)
            .HasColumnType("datetime2");
        builder.Property(assignment => assignment.AssignedByReference)
            .HasMaxLength(200)
            .IsRequired();
        ConfigureRowVersion(builder.Property(assignment => assignment.Version));

        builder.HasIndex(assignment => new
        {
            assignment.ApplicationUserId,
            assignment.RoleCode,
            assignment.EffectiveFromUtc
        })
            .IsUnique();
        builder.HasIndex(assignment => new
        {
            assignment.RoleCode,
            assignment.EffectiveFromUtc,
            assignment.EffectiveToUtc
        });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assignment => assignment.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<AuthenticationAbuseState> builder)
    {
        builder.ToTable("AuthenticationAbuseStates", Schema);
        ConfigureGuidKey(builder);

        builder.Property(state => state.SubjectKeyHash)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(state => state.Operation)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(state => state.FailureCount)
            .IsRequired();
        builder.Property(state => state.WindowStartedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(state => state.LockedUntilUtc)
            .HasColumnType("datetime2");
        ConfigureRowVersion(builder.Property(state => state.Version));

        builder.HasIndex(state => state.SubjectKeyHash)
            .IsUnique();
        builder.HasIndex(state => new { state.Operation, state.LockedUntilUtc });
    }

    public void Configure(EntityTypeBuilder<IdentityImportBatch> builder)
    {
        builder.ToTable(
            "IdentityImportBatches",
            Schema,
            table => table.HasCheckConstraint(
                "CK_IdentityImportBatches_State",
                "[State] IN ('uploaded', 'invalid', 'validated', 'published', 'failed')"));
        ConfigureGuidKey(builder);

        builder.Property(batch => batch.ClientRequestId)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(batch => batch.SourceName)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(batch => batch.SourceHash)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(batch => batch.State)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(batch => batch.ErrorSummaryJson)
            .HasMaxLength(16_000)
            .IsRequired();
        builder.Property(batch => batch.ResultSummaryJson)
            .HasMaxLength(16_000);
        builder.Property(batch => batch.ImportedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        ConfigureRowVersion(builder.Property(batch => batch.Version));

        builder.HasIndex(batch => batch.SourceHash)
            .IsUnique();
        builder.HasIndex(batch => new
        {
            batch.RequestedByUserId,
            batch.ClientRequestId
        })
            .IsUnique();
        builder.HasIndex(batch => new { batch.State, batch.ImportedAtUtc });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(batch => batch.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<IdentityImportCandidateRow> builder)
    {
        builder.ToTable("IdentityImportCandidateRows", Schema);
        ConfigureGuidKey(builder);

        builder.Property(row => row.Ordinal)
            .IsRequired();
        builder.Property(row => row.ExternalReference)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(row => row.Kind)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(row => row.UniversityId)
            .HasMaxLength(50);
        builder.Property(row => row.UserName)
            .HasMaxLength(200);
        builder.Property(row => row.StaffNumber)
            .HasMaxLength(50);
        builder.Property(row => row.DisplayName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(row => row.Roles)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(row => new { row.IdentityImportBatchId, row.Ordinal })
            .IsUnique();
        builder.HasIndex(row => new
        {
            row.IdentityImportBatchId,
            row.ExternalReference
        })
            .IsUnique();
        builder.HasOne<IdentityImportBatch>()
            .WithMany()
            .HasForeignKey(row => row.IdentityImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.ToTable("SecurityEvents", Schema);
        builder.HasKey(securityEvent => securityEvent.Id);
        builder.Property(securityEvent => securityEvent.Id)
            .ValueGeneratedNever();
        builder.Property(securityEvent => securityEvent.ApplicationUserId);
        builder.Property(securityEvent => securityEvent.EventType)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(securityEvent => securityEvent.ActorReference)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(securityEvent => securityEvent.SubjectReference)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(securityEvent => securityEvent.Reason)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(securityEvent => securityEvent.BeforeSummaryJson)
            .HasMaxLength(8000);
        builder.Property(securityEvent => securityEvent.AfterSummaryJson)
            .HasMaxLength(8000);
        builder.Property(securityEvent => securityEvent.MetadataJson)
            .HasMaxLength(4000);
        builder.Property(securityEvent => securityEvent.CorrelationId)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(securityEvent => securityEvent.OccurredAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(securityEvent => securityEvent.ApplicationUserId);
        builder.HasIndex(securityEvent => securityEvent.CorrelationId);
        builder.HasIndex(securityEvent => new
        {
            securityEvent.EventType,
            securityEvent.OccurredAtUtc
        });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(securityEvent => securityEvent.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<AdminSecurityGuard> builder)
    {
        builder.ToTable(
            "AdminSecurityGuards",
            Schema,
            table => table.HasCheckConstraint(
                "CK_AdminSecurityGuards_Singleton",
                "[Id] = 1"));
        builder.HasKey(guard => guard.Id);
        builder.Property(guard => guard.Id)
            .ValueGeneratedNever();
        ConfigureRowVersion(builder.Property(guard => guard.Version));
    }

    private static void ConfigureGuidKey<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<Guid>("Id")
            .ValueGeneratedNever();
    }

    private static void ConfigureRowVersion(PropertyBuilder<byte[]> property) =>
        property.IsRowVersion().IsRequired();
}
