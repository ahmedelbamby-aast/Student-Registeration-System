using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Audit;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

public sealed class StudentRegistrationDbContext : DbContext, IDataProtectionKeyContext
{
    public StudentRegistrationDbContext(
        DbContextOptions<StudentRegistrationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StudentRegistrationDbContext).Assembly);
    }
}
