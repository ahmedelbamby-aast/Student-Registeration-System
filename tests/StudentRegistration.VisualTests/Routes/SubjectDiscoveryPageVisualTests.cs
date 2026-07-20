using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;

public sealed class SubjectDiscoveryPageVisualContractTests { [Fact] public void Stu_02_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STU-02", "T156", "SubjectDiscoveryPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class SubjectDiscoveryPageVisualTests(VisualRegressionFixture fixture) { [Theory][MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType = typeof(Spec003RouteVisualAssertions))] public Task Stu_02_matches_approved_baseline(string browser, int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture, "STU-02", "/student/subjects", browser, width); }
