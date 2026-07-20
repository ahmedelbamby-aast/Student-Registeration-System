using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;

public sealed class RegistrationHistoryPageVisualContractTests { [Fact] public void Stu_07_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STU-07", "T181", "RegistrationHistoryPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class RegistrationHistoryPageVisualTests(VisualRegressionFixture fixture) { [Theory][MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType = typeof(Spec003RouteVisualAssertions))] public Task Stu_07_matches_approved_baseline(string browser, int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture, "STU-07", "/student/registrations", browser, width); }
