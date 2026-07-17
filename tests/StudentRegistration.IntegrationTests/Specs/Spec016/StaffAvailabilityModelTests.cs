using Microsoft.EntityFrameworkCore;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec016;

public sealed class StaffAvailabilityModelTests
{
    [Fact]
    public void Canonical_child_has_no_independent_version_or_StaffAdministration_mapping()
    {
        Assert.Equal("StudentRegistration.Scheduling", typeof(StaffAvailability).Assembly.GetName().Name);
        Assert.Null(typeof(StaffAvailability).GetProperty("Version"));
        Assert.Equal(
            [AvailabilityKind.Available, AvailabilityKind.Unavailable],
            Enum.GetValues<AvailabilityKind>());

        using var context = new StudentRegistrationDbContext(
            new DbContextOptionsBuilder<StudentRegistrationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Spec016ModelOnly;Trusted_Connection=True")
                .Options);
        var entity = context.Model.FindEntityType(typeof(StaffAvailability));
        Assert.NotNull(entity);
        Assert.Equal("StaffAvailabilities", entity.GetTableName());
        Assert.Equal("scheduling", entity.GetSchema());
    }
}
