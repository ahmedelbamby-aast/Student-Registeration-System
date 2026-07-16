using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.AcceptanceTests.Specs.Spec011;

internal static class Spec011AcceptanceSupport
{
    public static EligibilityService Service(
        Spec011ScenarioBuilder fixture,
        params StudentRegistration.Scheduling.Application.Ports.EligibilityOfferingSnapshot[] offerings) =>
        new(
            fixture.AcademicReader(),
            offerings.Length == 0
                ? fixture.OfferingReader()
                : fixture.OfferingReader(offerings),
            fixture.CurrentPlanReader(),
            new GroupSummaryProjection(),
            new FixedTimeProvider(fixture.EvaluatedAtUtc));

    public static OfferingSearchQuery Search(
        Spec011ScenarioBuilder fixture,
        params StudentRegistration.Scheduling.Application.Ports.EligibilityOfferingSnapshot[] offerings) =>
        new(Service(fixture, offerings));

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
