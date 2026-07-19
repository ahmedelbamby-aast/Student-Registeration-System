using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;
public sealed class OfferingAdministrationPageVisualContractTests { [Fact] public void Adm_06_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("ADM-06", "T216", "OfferingAdministrationPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class OfferingAdministrationPageVisualTests(VisualRegressionFixture fixture) { [Theory] [MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType=typeof(Spec003RouteVisualAssertions))] public Task Adm_06_matches_approved_baseline(string browser,int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture,"ADM-06","/admin/offerings",browser,width); }
