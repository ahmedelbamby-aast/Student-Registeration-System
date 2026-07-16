using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

public sealed class StudentRegistrationDbContextFactory :
    IDesignTimeDbContextFactory<StudentRegistrationDbContext>
{
    public StudentRegistrationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StudentRegistrationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=StudentRegistration_DesignTime;" +
                "Integrated Security=True;TrustServerCertificate=True")
            .Options;
        return new StudentRegistrationDbContext(options);
    }
}
