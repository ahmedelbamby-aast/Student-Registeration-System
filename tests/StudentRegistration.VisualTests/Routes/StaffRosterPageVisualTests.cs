using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;

public sealed class StaffRosterPageVisualContractTests { [Fact] public void Stf_03_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STF-03", "T246", "StaffRosterPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class StaffRosterPageVisualTests(VisualRegressionFixture fixture) { [Theory][MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType = typeof(Spec003RouteVisualAssertions))] public Task Stf_03_matches_approved_baseline(string browser, int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture, "STF-03", "/staff/groups/00000000-0000-0000-0000-000000003103/roster", browser, width); }
