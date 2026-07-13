using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentRegistration.Infrastructure.SqlServer.Audit;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence.Configurations;

public sealed class AuditEventModelConfiguration :
    IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents", "audit");
        builder.HasKey(auditEvent => auditEvent.Id);
        builder.Property(auditEvent => auditEvent.Id).ValueGeneratedNever();
        builder.Property(auditEvent => auditEvent.ActorReference)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.SubjectReference)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.Action)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.EntityType)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.EntityId)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.Reason)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.BeforeSummaryJson)
            .HasColumnType("nvarchar(max)");
        builder.Property(auditEvent => auditEvent.AfterSummaryJson)
            .HasColumnType("nvarchar(max)");
        builder.Property(auditEvent => auditEvent.CorrelationId)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(auditEvent => auditEvent.OccurredAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(auditEvent => auditEvent.CorrelationId);
        builder.HasIndex(auditEvent => auditEvent.OccurredAtUtc);
    }
}
