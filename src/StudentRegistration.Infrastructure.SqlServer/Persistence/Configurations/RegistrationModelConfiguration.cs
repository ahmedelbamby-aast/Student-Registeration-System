using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Registration.Domain;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class RegistrationModelConfiguration :
    IEntityTypeConfiguration<RegistrationSubmission>,
    IEntityTypeConfiguration<Enrollment>
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

    public void Configure(EntityTypeBuilder<RegistrationSubmission> builder)
    {
        builder.ToTable(
            "RegistrationSubmissions",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_State",
                    "[ProcessingState] IN ('processing', 'accepted', 'rejected')");
                table.HasCheckConstraint(
                    "CK_RegistrationSubmissions_ResultShape",
                    "[UpdatedAtUtc] >= [ReceivedAtUtc] AND " +
                    "([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [ReceivedAtUtc]) AND (" +
                    "([ProcessingState] = 'processing' AND [ResultCode] IS NULL AND " +
                    "[Reference] IS NULL AND [ReceiptSnapshotJson] IS NULL AND " +
                    "[DecisionSnapshotJson] IS NULL AND [CompletedAtUtc] IS NULL) OR " +
                    "([ProcessingState] = 'accepted' AND [ResultCode] IS NOT NULL AND " +
                    "[Reference] IS NOT NULL AND [ReceiptSnapshotJson] IS NOT NULL AND " +
                    "[DecisionSnapshotJson] IS NOT NULL AND [CompletedAtUtc] IS NOT NULL) OR " +
                    "([ProcessingState] = 'rejected' AND [ResultCode] IS NOT NULL AND " +
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
            RegistrationSubmissionState.Accepted => "accepted",
            RegistrationSubmissionState.Rejected => "rejected",
            _ => throw new InvalidOperationException(
                "Unknown registration-submission state.")
        };

    private static RegistrationSubmissionState SubmissionStateFromToken(
        string value) => value switch
        {
            "processing" => RegistrationSubmissionState.Processing,
            "accepted" => RegistrationSubmissionState.Accepted,
            "rejected" => RegistrationSubmissionState.Rejected,
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
}
