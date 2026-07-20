using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Audit;
using StudentRegistration.Registration.Domain;
using StudentRegistration.StaffAdministration.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

public sealed class StudentRegistrationDbContext : DbContext, IDataProtectionKeyContext
{
    public StudentRegistrationDbContext(
        DbContextOptions<StudentRegistrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    public DbSet<RegistrationSubmission> RegistrationSubmissions => Set<RegistrationSubmission>();
    public DbSet<RegistrationSubmissionLine> RegistrationSubmissionLines => Set<RegistrationSubmissionLine>();
    public DbSet<RegistrationSeatHold> RegistrationSeatHolds => Set<RegistrationSeatHold>();
    public DbSet<RegistrationApprovalDecision> RegistrationApprovalDecisions => Set<RegistrationApprovalDecision>();
    public DbSet<FirstTermAutoEnrollmentBatch> FirstTermAutoEnrollmentBatches => Set<FirstTermAutoEnrollmentBatch>();
    public DbSet<FirstTermAutoEnrollmentItem> FirstTermAutoEnrollmentItems => Set<FirstTermAutoEnrollmentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StudentRegistrationDbContext).Assembly);
    }
}
