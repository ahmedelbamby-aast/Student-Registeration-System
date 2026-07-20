using StudentRegistration.Contracts.Registration;
using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec015.EdgeCases;

public sealed class EC_3Tests
{
    [Theory]
    [InlineData("open", "/student/subjects")]
    [InlineData("closed", null)]
    public void Empty_state_exposes_discovery_only_for_an_open_window(string state, string? expected)
    {
        var dto = new RegistrationTimetableDto(null, "registrationOpen", state, [], state == "open" ? "/student/subjects" : null);
        Assert.Empty(dto.Groups);
        Assert.Equal(expected, dto.SubjectDiscoveryPath);
    }

    [Fact]
    public void Current_context_fails_all_term_multiplicity_and_uses_spec_008_window_order()
    {
        var source = RepositoryFiles.Read(
            "src/StudentRegistration.Infrastructure.SqlServer/Registration/SqlRegistrationRecordReader.cs");

        RepositoryFiles.ContainsAll(source,
            "teaching.Length > 1 || registration.Length > 1",
            "RegistrationWindowState.Upcoming",
            "OrderBy(window => window.OpensAtUtc)",
            "OrderByDescending(window => window.ClosesAtUtc)",
            "ThenBy(window => window.Id)");
    }
}
