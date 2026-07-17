using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class AdministrationAuditModelConfiguration :
    IEntityTypeConfiguration<ExportJob>
{
    private const string Schema = "administration";

    private static readonly ValueConverter<ExportJobState, string> StateConverter =
        new(
            state => StateToToken(state),
            value => StateFromToken(value));

    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcConverter =
        new(
            value => value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value);

    public void Configure(EntityTypeBuilder<ExportJob> builder)
    {
        builder.ToTable(
            "ExportJobs",
            Schema,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_ExportJobs_State",
                    "[State] IN ('pending', 'running', 'complete', 'failed', 'expired')");
                table.HasCheckConstraint(
                    "CK_ExportJobs_AttemptCount",
                    "[AttemptCount] >= 0 AND [AttemptCount] <= 3");
                table.HasCheckConstraint(
                    "CK_ExportJobs_LeaseShape",
                    "(([State] = 'running' AND [LeaseOwnerId] IS NOT NULL AND " +
                    "[LeaseExpiresAtUtc] IS NOT NULL AND [AttemptCount] >= 1) OR " +
                    "([State] <> 'running' AND [LeaseOwnerId] IS NULL AND " +
                    "[LeaseExpiresAtUtc] IS NULL))");
                table.HasCheckConstraint(
                    "CK_ExportJobs_ResultShape",
                    "(([State] IN ('pending', 'running') AND [ArtifactId] IS NULL AND " +
                    "[CompletedAtUtc] IS NULL AND [ExpiresAtUtc] IS NULL AND " +
                    "[FailureCode] IS NULL) OR " +
                    "([State] IN ('complete', 'expired') AND [ArtifactId] IS NOT NULL AND " +
                    "[CompletedAtUtc] IS NOT NULL AND [ExpiresAtUtc] IS NOT NULL AND " +
                    "[ExpiresAtUtc] > [CompletedAtUtc] AND [FailureCode] IS NULL) OR " +
                    "([State] = 'failed' AND [ArtifactId] IS NULL AND " +
                    "[CompletedAtUtc] IS NOT NULL AND [ExpiresAtUtc] IS NULL AND " +
                    "[FailureCode] IS NOT NULL))");
            });

        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id)
            .HasColumnName("JobId")
            .ValueGeneratedNever();
        builder.Property(job => job.OwnerId).IsRequired();
        builder.Property(job => job.ClientRequestId).IsRequired();
        builder.Property(job => job.ScopeHash)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(job => job.RequestHash)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(job => job.State)
            .HasConversion(StateConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(job => job.ArtifactId);
        builder.Property(job => job.FailureCode)
            .HasMaxLength(ExportJob.MaximumFailureCodeLength);
        builder.Property(job => job.CreatedAtUtc)
            .HasColumnType("datetime2")
            .HasConversion(UtcConverter)
            .IsRequired();
        builder.Property(job => job.CompletedAtUtc)
            .HasColumnType("datetime2")
            .HasConversion(NullableUtcConverter);
        builder.Property(job => job.ExpiresAtUtc)
            .HasColumnType("datetime2")
            .HasConversion(NullableUtcConverter);
        builder.Property(job => job.LeaseOwnerId)
            .HasMaxLength(200);
        builder.Property(job => job.LeaseExpiresAtUtc)
            .HasColumnType("datetime2")
            .HasConversion(NullableUtcConverter);
        builder.Property(job => job.AttemptCount).IsRequired();
        builder.Property(job => job.Version)
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(job => new
            {
                job.OwnerId,
                job.ScopeHash,
                job.ClientRequestId
            })
            .IsUnique();
        builder.HasIndex(job => job.ArtifactId)
            .IsUnique()
            .HasFilter("[ArtifactId] IS NOT NULL");
        builder.HasIndex(job => new
        {
            job.State,
            job.LeaseExpiresAtUtc,
            job.AttemptCount,
            job.Id
        });
        builder.HasIndex(job => new { job.ExpiresAtUtc, job.Id });
    }

    private static string StateToToken(ExportJobState state) => state switch
    {
        ExportJobState.Pending => "pending",
        ExportJobState.Running => "running",
        ExportJobState.Complete => "complete",
        ExportJobState.Failed => "failed",
        ExportJobState.Expired => "expired",
        _ => throw new InvalidOperationException("Unknown export-job state.")
    };

    private static ExportJobState StateFromToken(string value) => value switch
    {
        "pending" => ExportJobState.Pending,
        "running" => ExportJobState.Running,
        "complete" => ExportJobState.Complete,
        "failed" => ExportJobState.Failed,
        "expired" => ExportJobState.Expired,
        _ => throw new InvalidOperationException("Unknown persisted export-job state.")
    };
}
