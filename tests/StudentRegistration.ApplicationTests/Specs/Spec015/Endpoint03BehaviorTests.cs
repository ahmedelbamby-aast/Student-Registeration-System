using StudentRegistration.Registration.Application;

namespace StudentRegistration.ApplicationTests.Specs.Spec015;

public sealed class Endpoint03BehaviorTests
{
    [Fact]
    public async Task Empty_current_timetable_preserves_server_context_without_inventing_groups()
    {
        var reader = new Endpoint01And04BehaviorTests.CapturingReader();
        var queries = new RegistrationRecordQueries(reader, new RegistrationReceiptService());
        var result = await queries.ReadCurrentAsync(
            Endpoint01And04BehaviorTests.Principal("Student"));

        Assert.Equal(RegistrationRecordQueryOutcome.Succeeded, result.Outcome);
        Assert.NotNull(result.Timetable);
        Assert.Empty(result.Timetable!.Groups);
        Assert.Equal("none", result.Timetable.TermState);
        Assert.Null(result.Timetable.SubjectDiscoveryPath);
    }
}
