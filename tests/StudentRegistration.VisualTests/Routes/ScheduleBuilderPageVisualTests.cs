using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;

public sealed class ScheduleBuilderPageVisualContractTests { [Fact] public void Stu_04_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STU-04", "T166", "ScheduleBuilderPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class ScheduleBuilderPageVisualTests(VisualRegressionFixture fixture) { [Theory][MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType = typeof(Spec003RouteVisualAssertions))] public Task Stu_04_matches_approved_baseline(string browser, int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture, "STU-04", "/student/schedule", browser, width); }
