using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;
using StudentRegistration.StaffAdministration.Application;

namespace StudentRegistration.IntegrationTests.Specs.Spec016;

public sealed class StaffTermAvailabilityModelTests
{
    [Fact]
    public void Workspace_consumes_the_single_Scheduling_owned_aggregate_through_its_port()
    {
        Assert.Equal("StudentRegistration.Scheduling", typeof(StaffTermAvailability).Assembly.GetName().Name);
        Assert.Equal("StudentRegistration.Scheduling", typeof(IStaffAvailabilityPort).Assembly.GetName().Name);

        var field = typeof(StaffAvailabilityFacade)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Single(candidate => candidate.FieldType == typeof(IStaffAvailabilityPort));

        Assert.NotNull(field);
        Assert.Null(typeof(StaffAvailabilityFacade).Assembly.GetType(
            "StudentRegistration.StaffAdministration.Domain.StaffTermAvailability"));
    }
}
