using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;
public sealed class StaffDashboardPageVisualContractTests { [Fact] public void Stf_01_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STF-01", "T236", "StaffDashboardPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class StaffDashboardPageVisualTests(VisualRegressionFixture fixture) { [Theory] [MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType=typeof(Spec003RouteVisualAssertions))] public Task Stf_01_matches_approved_baseline(string browser,int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture,"STF-01","/staff",browser,width); }
