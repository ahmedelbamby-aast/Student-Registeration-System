using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;
public sealed class StaffTimetablePageVisualContractTests { [Fact] public void Stf_02_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STF-02", "T241", "StaffTimetablePage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class StaffTimetablePageVisualTests(VisualRegressionFixture fixture) { [Theory] [MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType=typeof(Spec003RouteVisualAssertions))] public Task Stf_02_matches_approved_baseline(string browser,int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture,"STF-02","/staff/timetable",browser,width); }
