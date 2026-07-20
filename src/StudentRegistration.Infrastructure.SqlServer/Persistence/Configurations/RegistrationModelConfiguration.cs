using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class RegistrationModelConfiguration :
    IEntityTypeConfiguration<RegistrationSubmission>,
    IEntityTypeConfiguration<Enrollment>,
    IEntityTypeConfiguration<RegistrationSubmissionLine>,
    IEntityTypeConfiguration<RegistrationSeatHold>,
    IEntityTypeConfiguration<RegistrationApprovalDecision>,
    IEntityTypeConfiguration<FirstTermAutoEnrollmentBatch>,
    IEntityTypeConfiguration<FirstTermAutoEnrollmentItem>
{
    private const string Schema = "registration";

    private static readonly ValueConverter<RegistrationSubmissionState, string>
        SubmissionStateConverter = new(
            state => SubmissionStateToToken(state),
            value => SubmissionStateFromToken(value));

    private static readonly ValueConverter<EnrollmentState, string>
        EnrollmentStateConverter = new(
            state => EnrollmentStateToToken(state),
            value => EnrollmentStateFromToken(value));

    private static readonly ValueConverter<RegistrationSubmissionOrigin, string>
        SubmissionOriginConverter = new(
            value => SubmissionOriginToToken(value),
            value => SubmissionOriginFromToken(value));

    public void Configure(EntityTypeBuilder<RegistrationSubmission> builder)
    {
        builder.ToTable(
            "RegistrationSubmissions",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_State",
                    "[ProcessingState] IN ('processing', 'pending-approval', 'accepted', 'rejected', 'expired')");
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_RequestedCredits",
                    "[RequestedCredits] >= 0");
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_Origin",
                    "[Origin] IN ('student-self-service', 'first-term-automatic')");
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_ResultShape",
                    "[UpdatedAtUtc] >= [ReceivedAtUtc] AND " +
                    "([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [ReceivedAtUtc]) AND (" +
                    "([ProcessingState] = 'processing' AND [ResultCode] IS NULL AND " +
                    "[Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND " +
                    "[DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR " +
                    "([ProcessingState] = 'pending-approval' AND [ResultCode] = 'PENDING_APPROVAL' AND " +
                    "[Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND " +
                    "[DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR " +
                    "([ProcessingState] = 'accepted' AND [ResultCode] IS NOT NULL AND " +
                    "[Reference] IS NOT NULL AND [ReceiptSnapshotJson] IS NOT NULL AND " +
                    "[DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR " +
                    "([ProcessingState] = 'rejected' AND [ResultCode] IS NOT NULL AND " +
                    "[Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND " +
                    "[DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR " +
                    "([ProcessingState] = 'expired' AND [ResultCode] IS NOT NULL AND " +
                    "[Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND " +
                    "[DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL))");
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_ReceiptSnapshotJson",
                    "[ReceiptSnapshotJson] IS NULL OR ISJSON([ReceiptSnapshotJson]) = 1");
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_DecisionSnapshotJson",
                    "[DecisionSnapshotJson] IS NULL OR ISJSON([DecisionSnapshotJson]) = 1");
            });

        builder.HasKey(submission => submission.Id);
        builder.Property(submission => submission.Id).ValueGeneratedNever();
        builder.Property(submission => submission.StudentId).IsRequired();
        builder.Property(submission => submission.TermId).IsRequired();
        builder.Property(submission => submission.ClientRequestId).IsRequired();
        builder.Property(submission => submission.PayloadHash)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(submission => submission.Origin)
            .HasConversion(SubmissionOriginConverter)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(submission => submission.RequestedCredits)
            .HasPrecision(5, 2)
            .IsRequired();
        builder.Property(submission => submission.ProcessingState)
            .HasConversion(SubmissionStateConverter)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(submission => submission.ResultCode)
            .HasMaxLength(100);
        builder.Property(submission => submission.Reference)
            .HasMaxLength(100);
        builder.Property(submission => submission.ReceiptSnapshotJson)
            .HasColumnType("nvarchar(max)");
        builder.Property(submission => submission.DecisionSnapshotJson)
            .HasColumnType("nvarchar(max)");
        builder.Property(submission => submission.ReceivedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(submission => submission.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(submission => submission.CompletedAtUtc)
            .HasColumnType("datetime2");
        builder.Ignore(submission => submission.IsFinal);

        builder.HasIndex(submission => new
        {
            submission.StudentId,
            submission.TermId,
            submission.ClientRequestId
        })
            .IsUnique();
        builder.HasIndex(submission => new
        {
            submission.StudentId,
            submission.TermId,
            submission.ProcessingState,
            submission.ReceivedAtUtc,
            submission.Id
        })
            .HasDatabaseName("IX_RegistrationSubmissions_StudentTermStateReceived");
        builder.HasIndex(submission => submission.Reference)
            .IsUnique()
            .HasFilter("[Reference] IS NOT NULL");
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(submission => submission.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(submission => submission.TermId)
            .OnDelete(DeleteBehavior.Restrict);

        var lines = builder.HasMany(submission => submission.Lines)
            .WithOne()
            .HasForeignKey(line => line.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
        lines.Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<RegistrationSubmissionLine> builder)
    {
        builder.ToTable("RegistrationSubmissionLines", Schema, table =>
        {
            table.HasCheckConstraint("CK_RegistrationSubmissionLines_State",
                "[State] IN ('pending-approval', 'approved', 'rejected', 'expired')");
            table.HasCheckConstraint("CK_RegistrationSubmissionLines_Credits", "[Credits] = 3");
        });
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).ValueGeneratedNever();
        builder.Property(line => line.SubmissionId).IsRequired();
        builder.Property(line => line.OfferingId).IsRequired();
        builder.Property(line => line.GroupId).IsRequired();
        builder.Property(line => line.CourseCode).HasMaxLength(50).IsRequired();
        builder.Property(line => line.SubjectTitle).HasMaxLength(300).IsRequired();
        builder.Property(line => line.Credits).HasPrecision(5, 2).IsRequired();
        builder.Property(line => line.State)
            .HasConversion(
                value => LineStateToToken(value),
                value => LineStateFromToken(value))
            .HasMaxLength(30).IsRequired();
        builder.Property(line => line.Version).IsRowVersion().IsRequired();
        builder.HasIndex(line => new { line.SubmissionId, line.OfferingId }).IsUnique();
        builder.HasOne<CourseOffering>().WithMany()
            .HasForeignKey(line => line.OfferingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionGroup>().WithMany()
            .HasForeignKey(line => new { line.OfferingId, line.GroupId })
            .HasPrincipalKey(group => new { group.OfferingId, group.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<RegistrationSeatHold> builder)
    {
        builder.ToTable("RegistrationSeatHolds", Schema, table =>
        {
            table.HasCheckConstraint("CK_RegistrationSeatHolds_State",
                "[State] IN ('active', 'consumed', 'released', 'expired')");
            table.HasCheckConstraint("CK_RegistrationSeatHolds_ResultShape",
                "([State] = 'active' AND [ReleasedAtUtc] IS NULL AND [ReleaseReason] IS NULL) OR " +
                "([State] = 'consumed' AND [ReleasedAtUtc] IS NOT NULL AND [ReleaseReason] IS NULL) OR " +
                "([State] IN ('released', 'expired') AND [ReleasedAtUtc] IS NOT NULL AND [ReleaseReason] IS NOT NULL)");
            table.HasCheckConstraint("CK_RegistrationSeatHolds_Time",
                "[ReleasedAtUtc] IS NULL OR [ReleasedAtUtc] >= [HeldAtUtc]");
        });
        builder.HasKey(hold => hold.Id);
        builder.Property(hold => hold.Id).ValueGeneratedNever();
        builder.Property(hold => hold.State)
            .HasConversion(value => HoldStateToToken(value), value => HoldStateFromToken(value))
            .HasMaxLength(20).IsRequired();
        builder.Property(hold => hold.HeldAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(hold => hold.ReleasedAtUtc).HasColumnType("datetime2");
        builder.Property(hold => hold.ReleaseReason).HasMaxLength(100);
        builder.Property(hold => hold.Version).IsRowVersion().IsRequired();
        builder.HasIndex(hold => hold.SubmissionLineId).IsUnique();
        builder.HasIndex(hold => new { hold.StudentId, hold.TermId, hold.OfferingId })
            .IsUnique().HasFilter("[State] = 'active'");
        builder.HasOne<RegistrationSubmissionLine>().WithOne()
            .HasForeignKey<RegistrationSeatHold>(hold => hold.SubmissionLineId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Student>().WithMany().HasForeignKey(hold => hold.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AcademicTerm>().WithMany().HasForeignKey(hold => hold.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionGroup>().WithMany()
            .HasForeignKey(hold => new { hold.OfferingId, hold.GroupId })
            .HasPrincipalKey(group => new { group.OfferingId, group.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<RegistrationApprovalDecision> builder)
    {
        builder.ToTable("RegistrationApprovalDecisions", Schema, table =>
        {
            table.HasCheckConstraint("CK_RegistrationApprovalDecisions_Role",
                "[ActorRole] IN ('admin', 'lecturer', 'teaching-assistant')");
            table.HasCheckConstraint("CK_RegistrationApprovalDecisions_Decision",
                "[Decision] IN ('approved', 'rejected')");
            table.HasCheckConstraint("CK_RegistrationApprovalDecisions_Scope",
                "([ActorRole] = 'admin' AND [AssignmentScopeGroupId] IS NULL) OR " +
                "([ActorRole] <> 'admin' AND [AssignmentScopeGroupId] IS NOT NULL)");
        });
        builder.HasKey(decision => decision.Id);
        builder.Property(decision => decision.Id).ValueGeneratedNever();
        builder.Property(decision => decision.ActorRole)
            .HasConversion(value => ApprovalRoleToToken(value), value => ApprovalRoleFromToken(value))
            .HasMaxLength(30).IsRequired();
        builder.Property(decision => decision.Decision)
            .HasConversion(value => ApprovalDecisionToToken(value), value => ApprovalDecisionFromToken(value))
            .HasMaxLength(20).IsRequired();
        builder.Property(decision => decision.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(decision => decision.PayloadHash).HasMaxLength(200).IsRequired();
        builder.Property(decision => decision.PolicyVersion).HasMaxLength(100).IsRequired();
        builder.Property(decision => decision.CorrelationId).HasMaxLength(100).IsRequired();
        builder.Property(decision => decision.DecidedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(decision => decision.SubmissionLineId).IsUnique();
        builder.HasIndex(decision => new
        {
            decision.ActorId,
            decision.SubmissionLineId,
            decision.ClientRequestId
        }).IsUnique();
        builder.HasOne<RegistrationSubmissionLine>().WithOne()
            .HasForeignKey<RegistrationApprovalDecision>(decision => decision.SubmissionLineId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PolicySet>().WithMany().HasForeignKey(decision => decision.PolicySetId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(decision => decision.ActorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionGroup>().WithMany()
            .HasForeignKey(decision => decision.AssignmentScopeGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<FirstTermAutoEnrollmentBatch> builder)
    {
        builder.ToTable("FirstTermAutoEnrollmentBatches", Schema, table =>
        {
            table.HasCheckConstraint("CK_FirstTermAutoEnrollmentBatches_State",
                "[State] IN ('pending', 'running', 'complete', 'completed-with-failures', 'failed')");
            table.HasCheckConstraint("CK_FirstTermAutoEnrollmentBatches_Lease",
                "([State] = 'running' AND [LeaseOwner] IS NOT NULL AND [LeaseExpiresAtUtc] IS NOT NULL) OR " +
                "([State] <> 'running' AND [LeaseOwner] IS NULL AND [LeaseExpiresAtUtc] IS NULL)");
        });
        builder.HasKey(batch => batch.Id);
        builder.Property(batch => batch.Id).ValueGeneratedNever();
        builder.Property(batch => batch.CohortScope).HasMaxLength(100).IsRequired();
        builder.Property(batch => batch.Purpose).HasMaxLength(100).IsRequired();
        builder.Property(batch => batch.State)
            .HasConversion(value => BatchStateToToken(value), value => BatchStateFromToken(value))
            .HasMaxLength(40).IsRequired();
        builder.Property(batch => batch.LeaseOwner).HasMaxLength(200);
        builder.Property(batch => batch.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(batch => batch.StartedAtUtc).HasColumnType("datetime2");
        builder.Property(batch => batch.LeaseExpiresAtUtc).HasColumnType("datetime2");
        builder.Property(batch => batch.CompletedAtUtc).HasColumnType("datetime2");
        builder.Property(batch => batch.Version).IsRowVersion().IsRequired();
        builder.Ignore(batch => batch.TotalStudents);
        builder.Ignore(batch => batch.AcceptedStudents);
        builder.Ignore(batch => batch.FailedStudents);
        builder.HasIndex(batch => new
        {
            batch.TermId,
            batch.CatalogueVersionId,
            batch.CohortScope,
            batch.Purpose
        }).IsUnique();
        builder.HasOne<AcademicTerm>().WithMany().HasForeignKey(batch => batch.TermId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CatalogueVersion>().WithMany()
            .HasForeignKey(batch => batch.CatalogueVersionId).OnDelete(DeleteBehavior.Restrict);
        var items = builder.HasMany(batch => batch.Items).WithOne()
            .HasForeignKey(item => item.BatchId).OnDelete(DeleteBehavior.Cascade);
        items.Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<FirstTermAutoEnrollmentItem> builder)
    {
        builder.ToTable("FirstTermAutoEnrollmentItems", Schema, table =>
        {
            table.HasCheckConstraint("CK_FirstTermAutoEnrollmentItems_State",
                "[State] IN ('pending', 'running', 'accepted', 'failed')");
            table.HasCheckConstraint("CK_FirstTermAutoEnrollmentItems_ResultShape",
                "([State] IN ('pending', 'running') AND [SubmissionId] IS NULL AND [FailureCode] IS NULL AND [CompletedAtUtc] IS NULL) OR " +
                "([State] = 'accepted' AND [SubmissionId] IS NOT NULL AND [FailureCode] IS NULL AND [CompletedAtUtc] IS NOT NULL) OR " +
                "([State] = 'failed' AND [SubmissionId] IS NULL AND [FailureCode] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL)");
        });
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.State)
            .HasConversion(value => BatchItemStateToToken(value), value => BatchItemStateFromToken(value))
            .HasMaxLength(20).IsRequired();
        builder.Property(item => item.FailureCode).HasMaxLength(100);
        builder.Property(item => item.StartedAtUtc).HasColumnType("datetime2");
        builder.Property(item => item.CompletedAtUtc).HasColumnType("datetime2");
        builder.Property(item => item.Version).IsRowVersion().IsRequired();
        builder.HasIndex(item => new { item.BatchId, item.StudentId }).IsUnique();
        builder.HasIndex(item => new { item.StudentId, item.ClientRequestId }).IsUnique();
        builder.HasOne<Student>().WithMany().HasForeignKey(item => item.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RegistrationSubmission>().WithMany()
            .HasForeignKey(item => item.SubmissionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable(
            "Enrollments",
            Schema,
            table => table.HasCheckConstraint(
                "CK_Enrollments_State",
                "[State] IN ('active')"));

        builder.HasKey(enrollment => enrollment.Id);
        builder.Property(enrollment => enrollment.Id).ValueGeneratedNever();
        builder.Property(enrollment => enrollment.StudentId).IsRequired();
        builder.Property(enrollment => enrollment.OfferingId).IsRequired();
        builder.Property(enrollment => enrollment.GroupId).IsRequired();
        builder.Property(enrollment => enrollment.SubmissionId).IsRequired();
        builder.Property(enrollment => enrollment.State)
            .HasConversion(EnrollmentStateConverter)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(enrollment => enrollment.RegisteredAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
        builder.Property(enrollment => enrollment.Version)
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(enrollment => new
        {
            enrollment.StudentId,
            enrollment.OfferingId
        })
            .IsUnique();
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(enrollment => enrollment.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RegistrationSubmission>()
            .WithMany()
            .HasForeignKey(enrollment => enrollment.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SectionGroup>()
            .WithMany()
            .HasForeignKey(enrollment => new
            {
                enrollment.OfferingId,
                enrollment.GroupId
            })
            .HasPrincipalKey(group => new
            {
                group.OfferingId,
                group.Id
            })
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static string SubmissionStateToToken(
        RegistrationSubmissionState state) => state switch
        {
            RegistrationSubmissionState.Processing => "processing",
            RegistrationSubmissionState.PendingApproval => "pending-approval",
            RegistrationSubmissionState.Accepted => "accepted",
            RegistrationSubmissionState.Rejected => "rejected",
            RegistrationSubmissionState.Expired => "expired",
            _ => throw new InvalidOperationException(
                "Unknown registration-submission state.")
        };

    private static string SubmissionOriginToToken(RegistrationSubmissionOrigin value) =>
        value switch
        {
            RegistrationSubmissionOrigin.StudentSelfService => "student-self-service",
            RegistrationSubmissionOrigin.FirstTermAutomatic => "first-term-automatic",
            _ => throw new InvalidOperationException("Unknown registration origin.")
        };

    private static RegistrationSubmissionOrigin SubmissionOriginFromToken(string value) =>
        value switch
        {
            "student-self-service" => RegistrationSubmissionOrigin.StudentSelfService,
            "first-term-automatic" => RegistrationSubmissionOrigin.FirstTermAutomatic,
            _ => throw new InvalidOperationException("Unknown registration origin.")
        };

    private static RegistrationSubmissionState SubmissionStateFromToken(
        string value) => value switch
        {
            "processing" => RegistrationSubmissionState.Processing,
            "pending-approval" => RegistrationSubmissionState.PendingApproval,
            "accepted" => RegistrationSubmissionState.Accepted,
            "rejected" => RegistrationSubmissionState.Rejected,
            "expired" => RegistrationSubmissionState.Expired,
            _ => throw new InvalidOperationException(
                "Unknown persisted registration-submission state.")
        };

    private static string EnrollmentStateToToken(EnrollmentState state) =>
        state switch
        {
            EnrollmentState.Active => "active",
            _ => throw new InvalidOperationException("Unknown enrollment state.")
        };

    private static EnrollmentState EnrollmentStateFromToken(string value) =>
        value switch
        {
            "active" => EnrollmentState.Active,
            _ => throw new InvalidOperationException(
                "Unknown persisted enrollment state.")
        };

    private static string LineStateToToken(RegistrationSubmissionLineState value) =>
        value switch
        {
            RegistrationSubmissionLineState.PendingApproval => "pending-approval",
            RegistrationSubmissionLineState.Approved => "approved",
            RegistrationSubmissionLineState.Rejected => "rejected",
            RegistrationSubmissionLineState.Expired => "expired",
            _ => throw new InvalidOperationException("Unknown registration-line state.")
        };
    private static RegistrationSubmissionLineState LineStateFromToken(string value) =>
        value switch
        {
            "pending-approval" => RegistrationSubmissionLineState.PendingApproval,
            "approved" => RegistrationSubmissionLineState.Approved,
            "rejected" => RegistrationSubmissionLineState.Rejected,
            "expired" => RegistrationSubmissionLineState.Expired,
            _ => throw new InvalidOperationException("Unknown registration-line state.")
        };
    private static string HoldStateToToken(RegistrationSeatHoldState value) => value.ToString().ToLowerInvariant();
    private static RegistrationSeatHoldState HoldStateFromToken(string value) => Enum.Parse<RegistrationSeatHoldState>(value, true);
    private static string ApprovalRoleToToken(RegistrationApprovalActorRole value) => value switch
    {
        RegistrationApprovalActorRole.Admin => "admin",
        RegistrationApprovalActorRole.Lecturer => "lecturer",
        RegistrationApprovalActorRole.TeachingAssistant => "teaching-assistant",
        _ => throw new InvalidOperationException("Unknown approval actor role.")
    };
    private static RegistrationApprovalActorRole ApprovalRoleFromToken(string value) => value switch
    {
        "admin" => RegistrationApprovalActorRole.Admin,
        "lecturer" => RegistrationApprovalActorRole.Lecturer,
        "teaching-assistant" => RegistrationApprovalActorRole.TeachingAssistant,
        _ => throw new InvalidOperationException("Unknown approval actor role.")
    };
    private static string ApprovalDecisionToToken(RegistrationApprovalDecisionValue value) => value.ToString().ToLowerInvariant();
    private static RegistrationApprovalDecisionValue ApprovalDecisionFromToken(string value) => Enum.Parse<RegistrationApprovalDecisionValue>(value, true);
    private static string BatchStateToToken(FirstTermAutoEnrollmentBatchState value) => value switch
    {
        FirstTermAutoEnrollmentBatchState.Pending => "pending",
        FirstTermAutoEnrollmentBatchState.Running => "running",
        FirstTermAutoEnrollmentBatchState.Complete => "complete",
        FirstTermAutoEnrollmentBatchState.CompletedWithFailures => "completed-with-failures",
        FirstTermAutoEnrollmentBatchState.Failed => "failed",
        _ => throw new InvalidOperationException("Unknown auto-enrollment batch state.")
    };
    private static FirstTermAutoEnrollmentBatchState BatchStateFromToken(string value) => value switch
    {
        "pending" => FirstTermAutoEnrollmentBatchState.Pending,
        "running" => FirstTermAutoEnrollmentBatchState.Running,
        "complete" => FirstTermAutoEnrollmentBatchState.Complete,
        "completed-with-failures" => FirstTermAutoEnrollmentBatchState.CompletedWithFailures,
        "failed" => FirstTermAutoEnrollmentBatchState.Failed,
        _ => throw new InvalidOperationException("Unknown auto-enrollment batch state.")
    };
    private static string BatchItemStateToToken(FirstTermAutoEnrollmentItemState value) => value.ToString().ToLowerInvariant();
    private static FirstTermAutoEnrollmentItemState BatchItemStateFromToken(string value) => Enum.Parse<FirstTermAutoEnrollmentItemState>(value, true);
}
