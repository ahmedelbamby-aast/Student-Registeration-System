using StudentRegistration.Registration.Application;
using StudentRegistration.Scheduling.Application.Ports;

namespace StudentRegistration.TestSupport.Spec011;

// Shared only by Spec 011 test projects through explicit compile links.
public static class Spec011ServiceFactory
{
    public static EligibilityService Service(
        Spec011ScenarioBuilder fixture,
        params EligibilityOfferingSnapshot[] offerings) =>
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
        params EligibilityOfferingSnapshot[] offerings) =>
        new(Service(fixture, offerings));

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
