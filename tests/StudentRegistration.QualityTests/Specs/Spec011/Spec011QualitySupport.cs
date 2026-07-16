using System.Text.Json;
using StudentRegistration.Registration.Application;
using StudentRegistration.TestSupport.Spec011;

namespace StudentRegistration.QualityTests.Specs.Spec011;

internal static class Spec011QualitySupport
{
    public const string EvidenceDirectory =
        "docs/release-evidence";

    public static EligibilityService Service(Spec011ScenarioBuilder fixture) =>
        Spec011ServiceFactory.Service(fixture);

    public static OfferingSearchQuery Search(Spec011ScenarioBuilder fixture) =>
        Spec011ServiceFactory.Search(fixture);

    public static string CanonicalJson<T>(T value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
}
