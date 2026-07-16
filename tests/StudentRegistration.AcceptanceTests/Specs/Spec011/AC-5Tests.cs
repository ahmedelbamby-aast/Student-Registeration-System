using StudentRegistration.TestSupport;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

public sealed class AC_5Tests
{
    [Fact]
    public async Task Quality_evidence_covers_determinism_parameterization_latency_and_non_color_status()
    {
        var fixture = new Spec011ScenarioBuilder();
        var service = Spec011AcceptanceSupport.Service(fixture);

        var first = Assert.Single((await service.EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);
        var second = Assert.Single((await service.EvaluateTermAsync(
            fixture.ApplicationUserId,
            fixture.TermId)).Items);

        Assert.Equal(first, second);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("docs/release-evidence/SPEC-011-NFR-1.md"),
            "300",
            "p95",
            "300 ms");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("docs/release-evidence/SPEC-011-NFR-2.md"),
            "100",
            "parameterized",
            "literal");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read("docs/release-evidence/SPEC-011-NFR-4.md"),
            "text",
            "icon",
            "color");
    }
}
